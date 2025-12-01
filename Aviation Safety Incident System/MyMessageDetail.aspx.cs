using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebFormsOptions;

namespace WebForms
{
    public partial class MyMessageDetail : System.Web.UI.Page
    {
        // 连接字符串方法
        private string GetConnString()
        {
            var cs = ConfigurationManager.ConnectionStrings["AviationDb"];
            return cs == null ? string.Empty : cs.ConnectionString;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["User_id"] == null || Session["User_type"] == null)
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                int incidentId = 0;
                if (!int.TryParse(Request.QueryString["id"], out incidentId) || incidentId <= 0)
                {
                    WriteMessage("参数错误：缺少有效的事件ID", true);
                    return;
                }

                if (!EnsureOwnership(incidentId))
                {
                    WriteMessage("无权限查看该消息：该事件不属于当前用户", true);
                    pnlView.Visible = false;
                    pnlEdit.Visible = false;
                    return;
                }

                Session["MessagesViewed"] = true;
                BindEditDropdowns();
                BindIncident(incidentId);
                // 隐藏审核处理记录组件，不再加载日志
                var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
                var pnlAudit = cph == null ? null : cph.FindControl("pnlAuditLogs") as System.Web.UI.WebControls.Panel;
                if (pnlAudit != null) pnlAudit.Visible = false;
            }
        }

        /// <summary>
        /// 绑定编辑下拉（小白讲解）：统一的事件类型与地点选项，首项为“请选择”。
        /// </summary>
        private void BindEditDropdowns()
        {
            DropdownOptions.BindIncidentTypes(ddlEditType, includeAll: false);
            ddlEditType.Items.Insert(0, new ListItem("请选择", ""));

            DropdownOptions.BindLocations(ddlEditLocation, includeCustom: false);
            ddlEditLocation.Items.Insert(0, new ListItem("请选择", ""));
        }

        private bool EnsureOwnership(int incidentId)
        {
            try
            {
                var typeObj = Session["User_type"];
                int ut = 0;
                if (typeObj != null && int.TryParse(typeObj.ToString(), out ut) && ut == 1)
                {
                    return true; // 管理员放行
                }

                int userId = Convert.ToInt32(Session["User_id"]);
                using (var conn = new SqlConnection(GetConnString()))
                using (var cmd = new SqlCommand("SELECT COUNT(1) FROM incident_info WHERE Incident_id=@id AND User_id=@uid", conn))
                {
                    cmd.Parameters.AddWithValue("@id", incidentId);
                    cmd.Parameters.AddWithValue("@uid", userId);
                    conn.Open();
                    int cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    return cnt > 0;
                }
            }
            catch { return false; }
        }

        private void BindIncident(int incidentId)
        {
            try
            {
                using (var conn = new SqlConnection(GetConnString()))
                using (var cmd = new SqlCommand("SELECT Incident_id, Incident_type, Occur_time, Location, Description, Incident_status FROM incident_info WHERE Incident_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", incidentId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string status = reader["Incident_status"].ToString();
                            string idStr = reader["Incident_id"].ToString();
                            DateTime occurTime = Convert.ToDateTime(reader["Occur_time"]);

                            // --- 核心逻辑：根据状态切换视图 ---
                            if (status == "待补充" || status == "已驳回" || status == "待重新提交")
                            {
                                // 【编辑模式】
                                pnlView.Visible = false;
                                pnlEdit.Visible = true;

                                // 填充编辑控件
                                hidIncidentId.Value = idStr;
                                lblEditIncidentIdDisplay.Text = idStr;
                                ddlEditType.SelectedValue = reader["Incident_type"].ToString();
                                txtEditOccurTime.Text = occurTime.ToString("yyyy-MM-ddTHH:mm");
                                ddlEditLocation.SelectedValue = reader["Location"].ToString();
                                txtEditDescription.Text = reader["Description"].ToString();

                                SetStatusBadge(status);
                            }
                            else
                            {
                                // 【只读模式】(默认)
                                pnlView.Visible = true;
                                pnlEdit.Visible = false;

                                lblIncidentId.Text = idStr;
                                lblType.Text = reader["Incident_type"].ToString();
                                lblOccurTime.Text = occurTime.ToString("yyyy-MM-dd HH:mm");
                                lblLocation.Text = reader["Location"].ToString();
                                lblStatus.Text = status;
                                lblDescription.Text = reader["Description"].ToString();
                                SetStatusBadge(status);
                            }
                        }
                        else
                        {
                            WriteMessage("未找到指定的事件信息", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteMessage("加载事件信息失败：" + ex.Message, true);
            }
        }

        private void SetStatusBadge(string status)
        {
            lblStatusBadge.Text = status;
            string badgeClass = "badge badge-secondary";
            switch (status)
            {
                case "已驳回": badgeClass = "badge badge-danger"; break;
                case "待补充": badgeClass = "badge badge-warning"; break;
                case "待重新提交": badgeClass = "badge badge-warning"; break;
                case "已通过": badgeClass = "badge badge-success"; break;
                case "待审核": badgeClass = "badge badge-primary"; break;
            }
            lblStatusBadge.CssClass = badgeClass;
        }

        private void BindLogs(int incidentId)
        {
            try
            {
                using (var conn = new SqlConnection(GetConnString()))
                using (var da = new SqlDataAdapter(@"SELECT l.Action_time, l.Action, ISNULL(u.User_name, '未知') AS Auditor_name, l.Reason
                                                      FROM incident_audit_log AS l LEFT JOIN users_info AS u ON l.Auditor_id = u.User_id
                                                      WHERE l.Incident_id = @id ORDER BY l.Action_time DESC", conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@id", incidentId);
                    var dt = new DataTable();
                    da.Fill(dt);
                    rptLogs.DataSource = dt;
                    rptLogs.DataBind();
                    lblNoLogs.Visible = (dt.Rows.Count == 0);
                }
            }
            catch (Exception ex)
            {
                WriteMessage("加载审核日志失败：" + ex.Message, true);
            }
        }

        /// <summary>
        /// 小白讲解：重新提交按钮点击
        /// 作用：当你在“待补充”状态下修改了类型、时间、地点和描述，点击这个按钮，系统会：
        /// 1）校验你填的内容是否完整且时间格式正确；
        /// 2）把事件更新为你修改后的值，并把状态改为“待审核”；
        /// 3）写一条审核日志，说明“用户已补充信息，重新提交审核”；
        /// 4）刷新页面，显示最新信息。
        /// </summary>
        protected void btnResubmit_Click(object sender, EventArgs e)
        {
            // 1. 获取并验证输入
            int incidentId = Convert.ToInt32(hidIncidentId.Value);
            string newType = ddlEditType.SelectedValue;
            string newLocation = ddlEditLocation.SelectedValue;
            string newDescription = txtEditDescription.Text.Trim();
            DateTime newOccurTime;

            if (string.IsNullOrEmpty(newType) || string.IsNullOrEmpty(newLocation) || string.IsNullOrEmpty(newDescription) || !DateTime.TryParse(txtEditOccurTime.Text, out newOccurTime))
            {
                WriteMessage("请填写所有必要信息，并确保日期格式正确。", true);
                return;
            }

            int userId = Convert.ToInt32(Session["User_id"]);
            SqlTransaction transaction = null;

            try
            {
                using (var conn = new SqlConnection(GetConnString()))
                {
                    conn.Open();
                    // 开启事务，确保状态更新和日志插入同时成功
                    transaction = conn.BeginTransaction();

                    // 2. 更新 incident_info 表，并将状态改为"待审核"
                    string updateSql = @"UPDATE incident_info 
                                         SET Incident_type = @type, 
                                             Occur_time = @occur, 
                                             Location = @loc, 
                                             Description = @desc, 
                                             Incident_status = N'待审核' 
                                         WHERE Incident_id = @id AND User_id = @uid";

                    using (var cmdUpdate = new SqlCommand(updateSql, conn, transaction))
                    {
                        cmdUpdate.Parameters.AddWithValue("@type", newType);
                        cmdUpdate.Parameters.AddWithValue("@occur", newOccurTime);
                        cmdUpdate.Parameters.AddWithValue("@loc", newLocation);
                        cmdUpdate.Parameters.AddWithValue("@desc", newDescription);
                        cmdUpdate.Parameters.AddWithValue("@id", incidentId);
                        cmdUpdate.Parameters.AddWithValue("@uid", userId); // 确保只能修改自己的事件
                        cmdUpdate.ExecuteNonQuery();
                    }

                    // 3. 插入一条新的审核日志（操作人是当前用户自己）
                    string logSql = @"INSERT INTO incident_audit_log (Incident_id, Auditor_id, Action, Reason, Action_time) 
                                      VALUES (@id, @uid, N'重新提交', N'用户已补充信息，重新提交审核。', GETDATE())";

                    using (var cmdLog = new SqlCommand(logSql, conn, transaction))
                    {
                        cmdLog.Parameters.AddWithValue("@id", incidentId);
                        cmdLog.Parameters.AddWithValue("@uid", userId);
                        cmdLog.ExecuteNonQuery();
                    }

                    // 提交事务
                    transaction.Commit();

                    // 4. 成功后提示并刷新页面（刷新后状态变为待审核，会自动切回只读模式）
                    WriteMessage("修改成功！已重新提交给审核员。", false);
                    BindIncident(incidentId);
                    BindLogs(incidentId);
                }
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                WriteMessage("提交失败：" + ex.Message, true);
            }
        }

        /// <summary>
        /// 小白讲解：页面顶部提示消息
        /// 传入提示文本 `msg` 和是否为错误 `isError`，我们会根据类型切换红色/绿色样式。
        /// </summary>
        private void WriteMessage(string msg, bool isError)
        {
            lblMessage.CssClass = isError ? "alert alert-danger" : "alert alert-success";
            lblMessage.Text = msg;
        }
    }
}
