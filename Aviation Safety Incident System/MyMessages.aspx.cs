using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class MyMessages : System.Web.UI.Page
    {
        /// <summary>
        /// 获取数据库连接字符串（小白解读）：
        /// 我们从 Web.config 的 ConnectionStrings 里读名为 AviationDb 的配置，用它连数据库。
        /// </summary>
        private string GetConnString()
        {
            // 建议：后续可以把这个方法提取到公共类中，避免每个页面都写一遍
            ConnectionStringSettings cs = ConfigurationManager.ConnectionStrings["AviationDb"];
            return cs == null ? string.Empty : cs.ConnectionString;
        }

        /// <summary>
        /// 页面加载（小白讲解）：
        /// 第一次打开时查询你的事件中状态为“已驳回/待补充”的记录，
        /// 并且取出最近一次审核日志（包含动作与理由），展示在表格里。
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["User_id"] == null || Session["User_type"] == null)
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }
                // 小白讲解：你进入“我的消息”页面，就视为已查看提醒，
                // 我们在 Session 里记一个标记，回到首页就不会显示“(1)”数字。
                Session["MessagesViewed"] = true;
                BindMessages();
            }
        }

        /// <summary>
        /// 绑定消息列表（小白讲解）：
        /// 【优化点】这里使用了 OUTER APPLY 来优化查询性能，代替了原来的子查询。
        /// 它能更高效地一次性找出每个事件对应的最新那条审核日志。
        /// </summary>
        private void BindMessages()
        {
            try
            {
                int userId = Convert.ToInt32(Session["User_id"]);
                // 读取筛选状态（小白讲解）：不选则视为“全部”
                var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
                var ddl = cph != null ? cph.FindControl("ddlStatusFilter") as DropDownList : null;
                string status = ddl == null ? string.Empty : ddl.SelectedValue;

                using (var conn = new SqlConnection(GetConnString()))
                {
                    // 【优化后的 SQL】使用 OUTER APPLY 解决 N+1 查询性能问题
                    string sql = @"
                        SELECT 
                            i.Incident_id, 
                            i.Incident_type, 
                            i.Occur_time, 
                            i.Incident_status,
                            log.Action AS LastAction,
                            log.Reason AS Reason
                        FROM incident_info AS i
                        OUTER APPLY (
                            SELECT TOP 1 l.Action, l.Reason 
                            FROM incident_audit_log AS l 
                            WHERE l.Incident_id = i.Incident_id AND l.Action = N'Reject'
                            ORDER BY l.Action_time DESC
                        ) AS log
                        WHERE i.User_id = @uid 
                          AND i.Incident_status IN (N'待重新提交', N'待补充', N'已驳回')
                        ORDER BY i.Occur_time DESC";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);
                            // 如果选择了具体状态，则在内存中过滤（小白讲解）：简化 SQL 拼接，易维护
                            if (!string.IsNullOrWhiteSpace(status))
                            {
                                var dv = dt.DefaultView;
                                // 注意：Replace("'", "''") 是为了防止简单的内存过滤表达式注入
                                dv.RowFilter = "Incident_status = '" + status.Replace("'", "''") + "'";
                                dt = dv.ToTable();
                            }
                            var cphGrid = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
                            var grid = cphGrid != null ? cphGrid.FindControl("gvMessages") as GridView : null;
                            if (grid != null)
                            {
                                grid.DataSource = dt;
                                grid.DataBind();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 【优化点】直接调用 WriteMessage 方法，代码更简洁
                WriteMessage("加载消息失败：" + ex.Message, true);
            }
        }

        /// <summary>
        /// 筛选按钮点击（小白讲解）：
        /// 当你选择“已驳回/待补充/全部”并点击筛选时，重新绑定列表。
        /// </summary>
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            // 小白讲解：筛选列表同样视为已查看，保持不显示数字
            Session["MessagesViewed"] = true;
            BindMessages();
        }

        /// <summary>
        /// 统一输出消息（小白讲解）：
        /// 在页面顶部显示提示或错误，不会影响其它区域的显示。
        /// </summary>
        private void WriteMessage(string msg, bool isError)
        {
            var cphMsg = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var lbl = cphMsg != null ? cphMsg.FindControl("lblMessage") as Label : null;
            if (lbl != null)
            {
                lbl.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;
                lbl.Text = msg;
            }
            else
            {
                // 如果找不到标签控件的后备方案
                Response.Write("<div style='color:" + (isError ? "red" : "green") + ";padding:10px;'>" + msg + "</div>");
            }
        }

        /// <summary>
        /// 删除消息对应的事件（小白讲解）：
        /// 【优化点】加入了数据库事务 (SqlTransaction)，确保删除日志和删除事件这两个操作要么一起成功，要么一起回滚，保证数据一致性。
        /// </summary>
        protected void gvMessages_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "DeleteMsg", StringComparison.OrdinalIgnoreCase)) return;

            int incidentId = 0;
            int.TryParse(e.CommandArgument == null ? "0" : e.CommandArgument.ToString(), out incidentId);
            if (incidentId <= 0) { WriteMessage("参数错误：Incident_id无效", true); return; }

            int userType = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);
            int currentUserId = Session["User_id"] == null ? 0 : Convert.ToInt32(Session["User_id"]);

            SqlTransaction transaction = null;

            try
            {
                using (var conn = new SqlConnection(GetConnString()))
                {
                    conn.Open();
                    // 【关键】开启事务
                    transaction = conn.BeginTransaction();

                    // 1. 权限校验（注意：校验查询也要关联事务）
                    // 管理员(1)放行；普通用户(2)必须是事件上报人
                    if (userType != 1)
                    {
                        string checkSql = "SELECT COUNT(1) FROM incident_info WHERE Incident_id=@id AND User_id=@uid";
                        // 注意这里要把 transaction 传给 SqlCommand
                        using (var checkCmd = new SqlCommand(checkSql, conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@id", incidentId);
                            checkCmd.Parameters.AddWithValue("@uid", currentUserId);
                            int cnt = Convert.ToInt32(checkCmd.ExecuteScalar());
                            if (cnt <= 0)
                            {
                                // 权限不足，回滚事务并退出
                                transaction.Rollback();
                                WriteMessage("无权限删除该事件：只能删除自己上报的事件", true);
                                return;
                            }
                        }
                    }

                    // 2. 执行删除操作（注意：所有操作都要关联同一个 transaction）

                    // 先删审核日志
                    using (var delLog = new SqlCommand("DELETE FROM incident_audit_log WHERE Incident_id=@id", conn, transaction))
                    {
                        delLog.Parameters.AddWithValue("@id", incidentId);
                        delLog.ExecuteNonQuery();
                    }

                    // 再删事件本体
                    using (var delIncident = new SqlCommand("DELETE FROM incident_info WHERE Incident_id=@id", conn, transaction))
                    {
                        delIncident.Parameters.AddWithValue("@id", incidentId);
                        delIncident.ExecuteNonQuery();
                    }

                    // 3. 一切顺利，提交事务
                    transaction.Commit();
                    WriteMessage("删除成功", false);
                    // 刷新列表
                    BindMessages();
                }
            }
            catch (Exception ex)
            {
                // 4. 发生异常，回滚事务，撤销之前的删除操作
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                WriteMessage("删除失败，系统已回滚操作：" + ex.Message, true);
            }
            finally
            {
                // 确保事务对象被释放
                if (transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }
    }
}
