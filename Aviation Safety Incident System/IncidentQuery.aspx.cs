using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class IncidentQuery : Page
    {
        // ========================================================================
        // 1. 工具方法与辅助类
        // ========================================================================

        // 获取数据库连接字符串
        private string GetConnString()
        {
            var connStr = ConfigurationManager.ConnectionStrings["AviationDb"];
            return connStr != null ? connStr.ConnectionString : string.Empty;
        }

        // 获取当前用户类型 (1=管理员, 2=普通用户)
        private int GetUserType()
        {
            int userType;
            if (Session["User_type"] != null && int.TryParse(Session["User_type"].ToString(), out userType))
            {
                return userType;
            }
            return 0; // 未登录或未知类型
        }

        // 获取当前用户ID
        private int GetUserId()
        {
            int userId;
            if (Session["User_id"] != null && int.TryParse(Session["User_id"].ToString(), out userId))
            {
                return userId;
            }
            return 0;
        }

        // 统一提示输出
        private void WriteMessage(string message, bool isError)
        {
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var lblMessage = cph == null ? null : cph.FindControl("lblMessage") as Label;

            if (lblMessage != null)
            {
                lblMessage.Text = message;
                lblMessage.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;
                lblMessage.Style["border-left-color"] = isError ? "#ff4d4f" : "#52c41a";
                lblMessage.Style["background-color"] = isError ? "#fff2f0" : "#f6ffed";
            }
        }

        // 清空提示
        private void ClearMessage()
        {
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var lbl = cph == null ? null : cph.FindControl("lblMessage") as Label;
            if (lbl != null)
            {
                lbl.Text = string.Empty;
                lbl.Style.Remove("border-left-color");
                lbl.Style.Remove("background-color");
            }
        }

        // 状态样式映射
        protected string StatusClass(object status)
        {
            var s = status == null ? string.Empty : status.ToString();
            switch (s)
            {
                case "已公开": return "status-badge status-public";
                case "处理中": return "status-badge status-processing";
                case "已驳回": return "status-badge status-reject";
                case "待补充": return "status-badge status-more";
                case "已上报": return "status-badge status-pending";
                case "已下架": return "status-badge status-offline"; // 【新增】已下架样式
                default: return "status-badge";
            }
        }

        // 下拉框选项辅助类
        public class DropdownOption
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        /// <summary>
        /// 绑定统一的“事件类型”下拉（小白讲解）：
        /// 使用 App_Code/DropdownOptions 的集中配置，保持与上报页完全一致；
        /// 查询页需要“全部”选项，故 includeAll=true。
        /// </summary>
        private void BindIncidentTypes(DropDownList ddl)
        {
            WebFormsOptions.DropdownOptions.BindIncidentTypes(ddl, includeAll: true);
        }

        // 【新增】获取事件状态列表（根据权限）
        private List<DropdownOption> GetIncidentStatus()
        {
            var options = new List<DropdownOption>
            {
                new DropdownOption { Text = "-- 全部状态 --", Value = "" },
                new DropdownOption { Text = "已上报", Value = "已上报" },
                new DropdownOption { Text = "处理中", Value = "处理中" },
                new DropdownOption { Text = "已驳回", Value = "已驳回" }
            };

            // 统一移除“已下架”选项：不在筛选下拉中提供该状态

            return options;
        }

        // ========================================================================
        // 2. 页面初始化与数据加载
        // ========================================================================

        /// <summary>
        /// 页面加载（小白讲解）：
        /// 现在允许未登录用户也能访问“事件查询”页面。
        /// 说明：管理员功能（例如行内删除按钮）仍然只对管理员显示；普通或未登录用户不会看到这些操作。
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ClearMessage();
                BindDropdowns(); // 统一绑定所有下拉框
                BindIncidents();
            }
        }

        // 【修改】统一绑定下拉框
        private void BindDropdowns()
        {
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            if (cph == null) return;

            // 绑定类型（统一来源）
            var ddlType = cph.FindControl("ddlType") as DropDownList;
            if (ddlType != null)
            {
                BindIncidentTypes(ddlType);
            }

            // 绑定状态
            var ddlStatus = cph.FindControl("ddlStatus") as DropDownList;
            if (ddlStatus != null)
            {
                ddlStatus.DataSource = GetIncidentStatus();
                ddlStatus.DataTextField = "Text";
                ddlStatus.DataValueField = "Value";
                ddlStatus.DataBind();
            }

            // 绑定地点
            var ddlLocation = cph.FindControl("ddlLocation") as DropDownList;
            if (ddlLocation != null)
            {
                WebFormsOptions.DropdownOptions.BindLocations(ddlLocation, includeCustom: false);
                ddlLocation.Items.Insert(0, new ListItem("-- 全部地点 --", ""));
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            ClearMessage();
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var grid = cph != null ? cph.FindControl("gvIncidents") as GridView : null;
            if (grid != null) grid.PageIndex = 0;
            BindIncidents();
        }

        protected void cvDateRange_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var txtStartCtrl = cph != null ? cph.FindControl("txtStartDate") as TextBox : null;
            var txtEndCtrl = cph != null ? cph.FindControl("txtEndDate") as TextBox : null;

            DateTime start = DateTime.MinValue;
            DateTime end = DateTime.MinValue;
            bool hasStart = txtStartCtrl != null && DateTime.TryParse(txtStartCtrl.Text, out start);
            bool hasEnd = txtEndCtrl != null && DateTime.TryParse(txtEndCtrl.Text, out end);

            if (!hasStart || !hasEnd)
            {
                args.IsValid = true;
                return;
            }
            args.IsValid = start.Date <= end.Date;
        }

        // 【修改】绑定事件列表，增加下架逻辑
        private void BindIncidents()
        {
            string connStr = GetConnString();
            if (string.IsNullOrEmpty(connStr))
            {
                WriteMessage("数据库连接失败。", true);
                return;
            }

            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var grid = cph != null ? cph.FindControl("gvIncidents") as GridView : null;

            var ddlTypeCtrl = cph != null ? cph.FindControl("ddlType") as DropDownList : null;
            var txtStartCtrl = cph != null ? cph.FindControl("txtStartDate") as TextBox : null;
            var txtEndCtrl = cph != null ? cph.FindControl("txtEndDate") as TextBox : null;
            var ddlLocationCtrl = cph != null ? cph.FindControl("ddlLocation") as DropDownList : null;
            var ddlStatusCtrl = null as DropDownList; // 简化：不再使用状态筛选

            string type = ddlTypeCtrl != null ? ddlTypeCtrl.SelectedValue : null;
            string location = ddlLocationCtrl != null ? ddlLocationCtrl.SelectedValue : null;
            string status = "已公开"; // 固定只查询“已公开”

            DateTime? start = null;
            DateTime? end = null;
            DateTime tmpStart;
            DateTime tmpEnd;
            if (txtStartCtrl != null && DateTime.TryParse(txtStartCtrl.Text, out tmpStart)) start = tmpStart.Date;
            if (txtEndCtrl != null && DateTime.TryParse(txtEndCtrl.Text, out tmpEnd)) end = tmpEnd.Date;

            // 构建 SQL
            var sql = "SELECT Incident_id, Incident_type, Occur_time, Location, Description, Incident_status FROM incident_info WHERE Incident_status = N'已公开'";
            var parameters = new List<SqlParameter>();

            // 固定仅展示“已公开”，无需再排除“已下架”

            if (!string.IsNullOrWhiteSpace(type))
            {
                sql += " AND Incident_type = @type";
                parameters.Add(new SqlParameter("@type", type));
            }
            if (start.HasValue)
            {
                sql += " AND Occur_time >= @start";
                parameters.Add(new SqlParameter("@start", start.Value));
            }
            if (end.HasValue)
            {
                // 结束日期加一天，实现包含当天的查询
                sql += " AND Occur_time < @end";
                parameters.Add(new SqlParameter("@end", end.Value.AddDays(1)));
            }
            if (!string.IsNullOrWhiteSpace(location))
            {
                sql += " AND Location = @loc";
                parameters.Add(new SqlParameter("@loc", location));
            }
            // 状态固定为“已公开”，不再追加条件

            sql += " ORDER BY Occur_time DESC";

            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        if (grid != null)
                        {
                            grid.DataSource = dt;
                            grid.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteMessage("查询数据失败：" + ex.Message, true);
            }
        }

        // ========================================================================
        // 3. GridView 事件处理
        // ========================================================================

        protected void gvIncidents_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ClearMessage();
            var grid = (GridView)sender;
            grid.PageIndex = e.NewPageIndex;
            BindIncidents();
        }

        protected void btnNextPage_Click(object sender, EventArgs e)
        {
            ClearMessage();
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var grid = cph != null ? cph.FindControl("gvIncidents") as GridView : null;
            if (grid == null) return;

            if (grid.PageIndex < grid.PageCount - 1)
            {
                grid.PageIndex++;
                BindIncidents();
            }
            else
            {
                WriteMessage("已经是最后一页了。", false);
            }
        }

        // 【修改】行数据绑定，控制下架按钮显示
        protected void gvIncidents_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // 1. 计算序号
                GridView grid = (GridView)sender;
                int serial = e.Row.RowIndex + 1 + (grid.PageIndex * grid.PageSize);
                var lbl = e.Row.FindControl("lblSerial") as Label;
                if (lbl != null) lbl.Text = serial.ToString();

                // 2. 控制“删除”按钮显示：仅管理员显示
                if (GetUserType() == 1)
                {
                    var btnDelete = e.Row.FindControl("btnDeleteRow") as Button;
                    if (btnDelete != null) btnDelete.Visible = true;
                }
            }
        }

        // 【新增】处理 GridView 按钮命令（如下架）
        protected void gvIncidents_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // 小白讲解：如果是“查看全文”，在页面底部弹窗展示详情，不跳转新页面
            if (e.CommandName == "ViewDetail")
            {
                int incidentId;
                if (!int.TryParse(e.CommandArgument == null ? "0" : e.CommandArgument.ToString(), out incidentId) || incidentId <= 0)
                {
                    WriteMessage("无效的事件 ID。", true);
                    return;
                }
                ShowDetail(incidentId);
                return;
            }
            if (e.CommandName == "DeleteIncident")
            {
                if (GetUserType() != 1)
                {
                    WriteMessage("无权执行此操作。", true);
                    return;
                }
                int incidentId = Convert.ToInt32(e.CommandArgument);
                if (DeleteIncident(incidentId))
                {
                    WriteMessage("已删除事件 (ID: " + incidentId + ")。", false);
                    BindIncidents();
                }
                else
                {
                    WriteMessage("删除失败，请重试。", true);
                }
            }
        }

        /// <summary>
        /// ShowDetail（小白讲解）：
        /// 根据事件编号，从数据库读取完整信息并填充到页面的弹窗中；
        /// 这样你点击“查看全文”就能在当前页看到完整描述，无需跳转。
        /// </summary>
        private void ShowDetail(int incidentId)
        {
            string connStr = GetConnString();
            if (string.IsNullOrEmpty(connStr)) { WriteMessage("无法获取数据库连接字符串。", true); return; }

            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand(@"SELECT Incident_id, Incident_type, Occur_time, Report_time, Location, Description FROM incident_info WHERE Incident_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", incidentId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
                            var pnl = cph == null ? null : cph.FindControl("pnlDetail") as System.Web.UI.WebControls.Panel;
                            var lblHeader = cph == null ? null : cph.FindControl("lblDetailHeader") as System.Web.UI.WebControls.Label;
                            var lit = cph == null ? null : cph.FindControl("litDetail") as System.Web.UI.WebControls.Literal;

                            if (pnl != null && lblHeader != null && lit != null)
                            {
                                int id = Convert.ToInt32(reader["Incident_id"]);
                                string type = reader["Incident_type"].ToString();
                                DateTime occur = Convert.ToDateTime(reader["Occur_time"]);
                                DateTime report = reader["Report_time"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Report_time"]);
                                string loc = reader["Location"].ToString();
                                string desc = reader["Description"].ToString();

                                string reportText = report == DateTime.MinValue ? "—" : report.ToString("yyyy-MM-dd HH:mm");
                                lblHeader.Text = string.Format("事件ID {0} · {1} · 发生时间 {2:yyyy-MM-dd HH:mm} · 上报时间 {3} · 地点 {4}", id, type, occur, reportText, loc);
                                lit.Text = Server.HtmlEncode(desc);
                                pnl.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteMessage("加载详情失败：" + ex.Message, true);
            }
        }

        /// <summary>
        /// btnCloseDetail_Click（小白讲解）：点击“关闭”按钮隐藏详情弹窗。
        /// </summary>
        protected void btnCloseDetail_Click(object sender, EventArgs e)
        {
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var pnl = cph == null ? null : cph.FindControl("pnlDetail") as System.Web.UI.WebControls.Panel;
            if (pnl != null) pnl.Visible = false;
        }

        // 【新增】执行下架操作（小白讲解）
        // 作用：把“已公开”的事件直接从数据库删除（包括相关审核日志），删除后在任何查询里都不会再显示
        private bool OfflineIncident(int incidentId)
        {
            string connStr = GetConnString();
            int userId = GetUserId();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1）预检当前状态，确保是“已公开”
                        string checkSql = "SELECT TOP 1 Incident_status FROM incident_info WHERE Incident_id = @id";
                        string currentStatus = null;
                        using (var cmdCheck = new SqlCommand(checkSql, conn, trans))
                        {
                            cmdCheck.Parameters.AddWithValue("@id", incidentId);
                            var obj = cmdCheck.ExecuteScalar();
                            currentStatus = obj == null ? null : obj.ToString();
                        }
                        if (!string.Equals(currentStatus, "已公开", StringComparison.Ordinal))
                        {
                            trans.Rollback();
                            WriteMessage("当前状态不是已公开，无法下架。", true);
                            return false;
                        }

                        // 2）先删除相关审核日志
                        string delLogSql = "DELETE FROM incident_audit_log WHERE Incident_id = @id";
                        using (var cmdDelLog = new SqlCommand(delLogSql, conn, trans))
                        {
                            cmdDelLog.Parameters.AddWithValue("@id", incidentId);
                            cmdDelLog.ExecuteNonQuery();
                        }

                        // 3）删除事件本体
                        string delIncidentSql = "DELETE FROM incident_info WHERE Incident_id = @id";
                        using (var cmdDelIncident = new SqlCommand(delIncidentSql, conn, trans))
                        {
                            cmdDelIncident.Parameters.AddWithValue("@id", incidentId);
                            int rows = cmdDelIncident.ExecuteNonQuery();
                            if (rows == 0)
                            {
                                trans.Rollback();
                                WriteMessage("删除失败：未找到事件。", true);
                                return false;
                            }
                        }

                        trans.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        WriteMessage("系统异常：" + ex.Message, true);
                        return false;
                    }
                }
            }
        }
        /// <summary>
        /// 小白讲解：批量清理“已下架”事件
        /// 作用：一次性删除所有状态为“已下架”的事件及其审核日志；仅管理员可操作。
        /// 步骤：先删日志，再删事件；放入事务中保证要么都成功要么都失败。
        /// </summary>
        protected void btnCleanupOffline_Click(object sender, EventArgs e)
        {
            ClearMessage();
            if (GetUserType() != 1)
            {
                WriteMessage("无权执行此操作。", true);
                return;
            }

            string connStr = GetConnString();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 统计将要删除的事件数量
                        int count = 0;
                        using (var cmdCount = new SqlCommand("SELECT COUNT(1) FROM incident_info WHERE Incident_status=@st", conn, trans))
                        {
                            cmdCount.Parameters.AddWithValue("@st", "已下架");
                            object obj = cmdCount.ExecuteScalar();
                            count = obj == null ? 0 : Convert.ToInt32(obj);
                        }

                        // 先删除日志
                        using (var cmdDelLog = new SqlCommand("DELETE FROM incident_audit_log WHERE Incident_id IN (SELECT Incident_id FROM incident_info WHERE Incident_status=@st)", conn, trans))
                        {
                            cmdDelLog.Parameters.AddWithValue("@st", "已下架");
                            cmdDelLog.ExecuteNonQuery();
                        }

                        // 删除事件
                        using (var cmdDelIncident = new SqlCommand("DELETE FROM incident_info WHERE Incident_status=@st", conn, trans))
                        {
                            cmdDelIncident.Parameters.AddWithValue("@st", "已下架");
                            cmdDelIncident.ExecuteNonQuery();
                        }

                        trans.Commit();
                        WriteMessage("已清理已下架事件数量：" + count, false);
                        BindIncidents();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        WriteMessage("清理失败：" + ex.Message, true);
                    }
                }
            }
        }
        /// <summary>
        /// 小白讲解：删除单条“已下架”事件
        /// 作用：仅管理员能删除指定的已下架事件，包含删除其审核日志；用事务保证一致性。
        /// </summary>
        private bool DeleteOfflineIncident(int incidentId)
        {
            string connStr = GetConnString();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 检查是否是已下架
                        using (var cmdCheck = new SqlCommand("SELECT TOP 1 Incident_status FROM incident_info WHERE Incident_id=@id", conn, trans))
                        {
                            cmdCheck.Parameters.AddWithValue("@id", incidentId);
                            object obj = cmdCheck.ExecuteScalar();
                            string st = obj == null ? null : obj.ToString();
                            if (!string.Equals(st, "已下架", StringComparison.Ordinal))
                            {
                                trans.Rollback();
                                return false;
                            }
                        }

                        // 删除日志
                        using (var cmdDelLog = new SqlCommand("DELETE FROM incident_audit_log WHERE Incident_id=@id", conn, trans))
                        {
                            cmdDelLog.Parameters.AddWithValue("@id", incidentId);
                            cmdDelLog.ExecuteNonQuery();
                        }
                        // 删除事件
                        using (var cmdDelIncident = new SqlCommand("DELETE FROM incident_info WHERE Incident_id=@id", conn, trans))
                        {
                            cmdDelIncident.Parameters.AddWithValue("@id", incidentId);
                            int rows = cmdDelIncident.ExecuteNonQuery();
                            if (rows == 0)
                            {
                                trans.Rollback();
                                return false;
                            }
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }
        /// <summary>
        /// 小白讲解：批量下架（删除）所有“已公开”事件
        /// 作用：一次性删除状态为“已公开”的事件及其审核日志；仅管理员可操作。
        /// 步骤：查询数量→删日志→删事件；事务保证一致性。
        /// </summary>
        protected void btnBulkDeletePublic_Click(object sender, EventArgs e)
        {
            ClearMessage();
            if (GetUserType() != 1)
            {
                WriteMessage("无权执行此操作。", true);
                return;
            }

            string connStr = GetConnString();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        int count = 0;
                        using (var cmdCount = new SqlCommand("SELECT COUNT(1) FROM incident_info WHERE Incident_status=@st", conn, trans))
                        {
                            cmdCount.Parameters.AddWithValue("@st", "已公开");
                            object obj = cmdCount.ExecuteScalar();
                            count = obj == null ? 0 : Convert.ToInt32(obj);
                        }

                        using (var cmdDelLog = new SqlCommand("DELETE FROM incident_audit_log WHERE Incident_id IN (SELECT Incident_id FROM incident_info WHERE Incident_status=@st)", conn, trans))
                        {
                            cmdDelLog.Parameters.AddWithValue("@st", "已公开");
                            cmdDelLog.ExecuteNonQuery();
                        }
                        using (var cmdDelIncident = new SqlCommand("DELETE FROM incident_info WHERE Incident_status=@st", conn, trans))
                        {
                            cmdDelIncident.Parameters.AddWithValue("@st", "已公开");
                            cmdDelIncident.ExecuteNonQuery();
                        }

                        trans.Commit();
                        WriteMessage("已删除已公开事件数量：" + count, false);
                        BindIncidents();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        WriteMessage("批量下架失败：" + ex.Message, true);
                    }
                }
            }
        }
        /// <summary>
        /// 小白讲解：删除单条“处理中”事件
        /// 作用：仅管理员能删除指定的处理中事件，同时删除其审核日志；用事务保证一致性。
        /// </summary>
        private bool DeleteProcessingIncident(int incidentId)
        {
            string connStr = GetConnString();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 检查是否是处理中
                        using (var cmdCheck = new SqlCommand("SELECT TOP 1 Incident_status FROM incident_info WHERE Incident_id=@id", conn, trans))
                        {
                            cmdCheck.Parameters.AddWithValue("@id", incidentId);
                            object obj = cmdCheck.ExecuteScalar();
                            string st = obj == null ? null : obj.ToString();
                            if (!string.Equals(st, "处理中", StringComparison.Ordinal))
                            {
                                trans.Rollback();
                                return false;
                            }
                        }

                        // 删除日志
                        using (var cmdDelLog = new SqlCommand("DELETE FROM incident_audit_log WHERE Incident_id=@id", conn, trans))
                        {
                            cmdDelLog.Parameters.AddWithValue("@id", incidentId);
                            cmdDelLog.ExecuteNonQuery();
                        }
                        // 删除事件
                        using (var cmdDelIncident = new SqlCommand("DELETE FROM incident_info WHERE Incident_id=@id", conn, trans))
                        {
                            cmdDelIncident.Parameters.AddWithValue("@id", incidentId);
                            int rows = cmdDelIncident.ExecuteNonQuery();
                            if (rows == 0)
                            {
                                trans.Rollback();
                                return false;
                            }
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }
        /// <summary>
        /// 小白讲解：批量删除“处理中”事件
        /// 作用：一次性删除状态为“处理中”的事件及其审核日志；仅管理员可操作。
        /// </summary>
        protected void btnBulkDeleteProcessing_Click(object sender, EventArgs e)
        {
            ClearMessage();
            if (GetUserType() != 1)
            {
                WriteMessage("无权执行此操作。", true);
                return;
            }

            string connStr = GetConnString();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        int count = 0;
                        using (var cmdCount = new SqlCommand("SELECT COUNT(1) FROM incident_info WHERE Incident_status=@st", conn, trans))
                        {
                            cmdCount.Parameters.AddWithValue("@st", "处理中");
                            object obj = cmdCount.ExecuteScalar();
                            count = obj == null ? 0 : Convert.ToInt32(obj);
                        }

                        using (var cmdDelLog = new SqlCommand("DELETE FROM incident_audit_log WHERE Incident_id IN (SELECT Incident_id FROM incident_info WHERE Incident_status=@st)", conn, trans))
                        {
                            cmdDelLog.Parameters.AddWithValue("@st", "处理中");
                            cmdDelLog.ExecuteNonQuery();
                        }
                        using (var cmdDelIncident = new SqlCommand("DELETE FROM incident_info WHERE Incident_status=@st", conn, trans))
                        {
                            cmdDelIncident.Parameters.AddWithValue("@st", "处理中");
                            cmdDelIncident.ExecuteNonQuery();
                        }

                        trans.Commit();
                        WriteMessage("已删除处理中事件数量：" + count, false);
                        BindIncidents();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        WriteMessage("批量删除处理中失败：" + ex.Message, true);
                    }
                }
            }
        }
        
        // 类结束标记移除（确保后续方法仍在类内）
        /// <summary>
        /// 小白讲解：删除任意状态的单条事件（管理员）
        /// 作用：无论是已公开、处理中、已下架等状态，都统一删除该事件及其审核日志；用事务保证一致性。
        /// </summary>
        private bool DeleteIncident(int incidentId)
        {
            string connStr = GetConnString();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmdDelLog = new SqlCommand("DELETE FROM incident_audit_log WHERE Incident_id=@id", conn, trans))
                        {
                            cmdDelLog.Parameters.AddWithValue("@id", incidentId);
                            cmdDelLog.ExecuteNonQuery();
                        }
                        using (var cmdDelIncident = new SqlCommand("DELETE FROM incident_info WHERE Incident_id=@id", conn, trans))
                        {
                            cmdDelIncident.Parameters.AddWithValue("@id", incidentId);
                            int rows = cmdDelIncident.ExecuteNonQuery();
                            if (rows == 0)
                            {
                                trans.Rollback();
                                return false;
                            }
                        }
                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }
        // 统一将类与命名空间的结束标记放在文件末尾
    }
}
