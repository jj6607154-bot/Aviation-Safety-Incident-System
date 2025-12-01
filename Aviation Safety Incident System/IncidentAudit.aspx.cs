using System;
using System.Collections.Generic; // 用于 List<T>
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class IncidentAudit : System.Web.UI.Page
    {
        // ========================================================================
        // 1. 工具方法
        // ========================================================================

        /// <summary>
        /// 获取数据库连接字符串（小白讲解）：
        /// 从配置文件 web.config 的 ConnectionStrings 里读取名为 AviationDb 的连接字符串。
        /// 这个字符串告诉系统去哪个数据库、用什么账号连接。
        /// </summary>
        private string GetConnString()
        {
            var connStr = ConfigurationManager.ConnectionStrings["AviationDb"];
            return connStr != null ? connStr.ConnectionString : string.Empty;
        }

        /// <summary>
        /// WriteMessage（小白讲解）：在页面顶部显示一条提示信息。
        /// isError=true 用红色样式；isError=false 用绿色样式。
        /// </summary>
        private void WriteMessage(string message, bool isError)
        {
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var lblMessage = cph == null ? null : cph.FindControl("lblMessage") as System.Web.UI.WebControls.Label;

            if (lblMessage != null)
            {
                lblMessage.Text = message;
                lblMessage.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;
                lblMessage.Style["border-left-color"] = isError ? "#ff4d4f" : "#52c41a";
                lblMessage.Style["background-color"] = isError ? "#fff2f0" : "#f6ffed";
            }
            else
            {
                Response.Write(isError ?
                    ("<div style='color:red;padding:10px;border:1px solid red;margin:10px;'>" + message + "</div>") :
                    ("<div style='color:green;padding:10px;border:1px solid green;margin:10px;'>" + message + "</div>"));
            }
        }

        /// <summary>
        /// ClearMessage（小白讲解）：把页面上的提示信息清空并恢复为灰色背景。
        /// </summary>
        private void ClearMessage()
        {
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var lbl = cph == null ? null : cph.FindControl("lblMessage") as System.Web.UI.WebControls.Label;
            if (lbl != null)
            {
                lbl.Text = string.Empty;
                lbl.Style["border-left-color"] = "#ddd";
                lbl.Style["background-color"] = "#f9f9f9";
            }
        }

        /// <summary>
        /// StatusClass（小白讲解）：根据事件状态返回不同的徽章样式类名，方便前端显示颜色。
        /// </summary>
        protected string StatusClass(object status)
        {
            var s = status == null ? string.Empty : status.ToString();
            switch (s)
            {
                case "已公开": return "status-badge status-public";
                case "处理中": return "status-badge status-processing";
                case "已驳回": return "status-badge status-reject";
                case "待补充": return "status-badge status-more";
                case "待重新提交": return "status-badge status-more";
                case "待审核": return "status-badge status-pending";
                case "已上报": return "status-badge status-pending";
                default: return "status-badge status-pending";
            }
        }

        // ========================================================================
        // 2. 页面事件
        // ========================================================================

        /// <summary>
        /// Page_Load（小白讲解）：
        /// 检查权限，只允许管理员(1)或审核人员(3)进入；首次进入时加载待审核列表。
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            int userType = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);

            if (userType != 3)
            {
                WriteMessage("无权限访问：仅审核人员可审核事件。", true);
                var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
                var panel = cph == null ? null : cph.FindControl("pnlAuditContent") as System.Web.UI.WebControls.Panel;
                if (panel != null) { panel.Visible = false; }
                return;
            }

            if (!IsPostBack)
            {
                ClearMessage();
                BindPending();
            }
        }

        /// <summary>
        /// BindPending（小白讲解）：
        /// 加载普通用户提交的事件，且过滤掉状态为“已驳回”的记录，
        /// 并按上报时间（无上报则按发生时间）倒序绑定到审核列表。
        /// </summary>
        private void BindPending()
        {
            string connStr = GetConnString();
            if (string.IsNullOrEmpty(connStr))
            {
                WriteMessage("无法获取数据库连接字符串，请检查配置文件。", true);
                return;
            }

            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT i.Incident_id, i.Incident_type, i.Occur_time, i.Location, i.Description, i.Incident_status, i.Report_time
                        FROM incident_info AS i
                        WHERE ISNULL(i.Incident_status, N'') <> N'已驳回'
                        ORDER BY ISNULL(i.Report_time, i.Occur_time) DESC";

                    using (var da = new SqlDataAdapter(sql, conn))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
                        var grid = cph == null ? null : cph.FindControl("gvPending") as System.Web.UI.WebControls.GridView;

                        if (grid != null)
                        {
                            grid.DataSource = dt;
                            grid.DataBind();
                        }
                        else
                        {
                            WriteMessage("页面结构异常：找不到 GridView 控件 gvPending。", true);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                WriteMessage("数据库错误：" + sqlEx.Message, true);
            }
            catch (Exception ex)
            {
                WriteMessage("加载待审核事件失败：" + ex.Message, true);
            }
        }



        /// <summary>
        /// gvPending_PageIndexChanging（小白讲解）：翻页时触发，设置新页索引并重新绑定数据。
        /// </summary>
        protected void gvPending_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var grid = cph == null ? null : cph.FindControl("gvPending") as System.Web.UI.WebControls.GridView;

            if (grid == null)
            {
                WriteMessage("页面结构异常：找不到 gvPending。", true);
                return;
            }

            ClearMessage();
            grid.PageIndex = e.NewPageIndex;
            BindPending();
        }

        /// <summary>
        /// gvPending_RowDataBound（小白讲解）：每行数据绑定时，计算序号并按状态控制按钮显示。
        /// </summary>
        protected void gvPending_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridView grid = (GridView)sender;
                int serial = e.Row.RowIndex + 1 + (grid.PageIndex * grid.PageSize);

                var lbl = e.Row.FindControl("lblSerial") as Label;
                if (lbl != null)
                {
                    lbl.Text = serial.ToString();
                }

                // 小白讲解：根据当前状态控制按钮显示
                var drv = e.Row.DataItem as System.Data.DataRowView;
                string status = drv == null ? string.Empty : (drv["Incident_status"].ToString());
                int userType = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);
                var btnApprovePublic = e.Row.FindControl("btnApprovePublic") as Button;
                var btnApproveProcessing = e.Row.FindControl("btnApproveProcessing") as Button;
                var btnReject = e.Row.FindControl("btnReject") as Button;
                var btnDeleteAny = e.Row.FindControl("btnDeleteAny") as Button;

                bool isPublic = string.Equals(status, "已公开", StringComparison.Ordinal);
                if (btnApprovePublic != null) btnApprovePublic.Visible = !isPublic;
                if (btnApproveProcessing != null) btnApproveProcessing.Visible = false;
                if (btnReject != null) btnReject.Visible = !isPublic;
                if (btnDeleteAny != null) btnDeleteAny.Visible = (userType == 1 || userType == 3);

                if (btnDeleteAny != null)
                {
                    btnDeleteAny.Visible = (userType == 1 || userType == 3);
                }
            }
        }

        /// <summary>
        /// btnRefresh_Click（小白讲解）：点击“刷新列表”按钮时重新加载数据。
        /// </summary>
        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearMessage();
            BindPending();
        }

        // ========================================================================
        // 3. 审核操作核心逻辑
        // ========================================================================

        /// <summary>
        /// gvPending_RowCommand（小白讲解）：处理每一行的审核操作按钮。
        /// - 通过-已公开：状态改为“已公开”
        /// - 通过-处理中：状态改为“处理中”
        /// - 驳回：必须填写理由，状态改为“已驳回”
        /// - 下架-取消公开：仅管理员可执行，删除事件及其审核日志
        /// </summary>
        protected void gvPending_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            string action = e.CommandName ?? string.Empty;
            ClearMessage();

            if (string.Equals(action, "Page", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Sort", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            int incidentId = 0;
            int.TryParse(e.CommandArgument == null ? "0" : e.CommandArgument.ToString(), out incidentId);
            if (incidentId <= 0)
            {
                WriteMessage("无效的事件 ID。", true);
                return;
            }

            // 查看全文（小白讲解）：点“查看全文”时，在页面下方展开详情，不跳转新页面。
            if (string.Equals(action, "ViewDetail", StringComparison.Ordinal))
            {
                ShowDetail(incidentId);
                return;
            }

            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var txtReason = cph == null ? null : cph.FindControl("txtReason") as System.Web.UI.WebControls.TextBox;
            string reason = txtReason == null ? string.Empty : (txtReason.Text ?? string.Empty).Trim();

            string newStatus = null;
            switch (action)
            {
                case "ApprovePublic":
                    newStatus = "已公开";
                    break;
                case "ApproveProcessing":
                    newStatus = "处理中";
                    break;
                case "Unpublish":
                    // 小白讲解：下架（不删除）。管理员或审核员将状态改为“已下架”，同时记录审核日志。
                    {
                        int userTypeX = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);
                        if (userTypeX != 1 && userTypeX != 3)
                        {
                            WriteMessage("无权限：仅管理员或审核人员可以下架。", true);
                            return;
                        }

                        string connStrX = GetConnString();
                        if (string.IsNullOrEmpty(connStrX)) { WriteMessage("无法获取数据库连接字符串。", true); return; }

                        try
                        {
                            using (var connX = new SqlConnection(connStrX))
                            {
                                connX.Open();
                                using (var txX = connX.BeginTransaction())
                                {
                                    try
                                    {
                                        // 状态检查
                                        using (var cmdCheck = new SqlCommand("SELECT TOP 1 Incident_status FROM incident_info WHERE Incident_id=@id", connX, txX))
                                        {
                                            cmdCheck.Parameters.AddWithValue("@id", incidentId);
                                            var obj = cmdCheck.ExecuteScalar();
                                            var st = obj == null ? string.Empty : obj.ToString();
                                            if (!string.Equals(st, "已公开", StringComparison.Ordinal))
                                            {
                                                txX.Rollback();
                                                WriteMessage("当前状态不是已公开，无法下架。", true);
                                                return;
                                            }
                                        }

                                        // 更新状态为“已下架”
                                        using (var cmdUpdate = new SqlCommand("UPDATE incident_info SET Incident_status=N'已下架' WHERE Incident_id=@id", connX, txX))
                                        {
                                            cmdUpdate.Parameters.AddWithValue("@id", incidentId);
                                            cmdUpdate.ExecuteNonQuery();
                                        }

                                        // 写审核日志
                                        using (var cmdLog = new SqlCommand("INSERT INTO incident_audit_log(Incident_id, Auditor_id, Action, Reason) VALUES(@iid, @aid, @act, @rsn)", connX, txX))
                                        {
                                            cmdLog.Parameters.AddWithValue("@iid", incidentId);
                                            cmdLog.Parameters.AddWithValue("@aid", Session["User_id"] == null ? 0 : Convert.ToInt32(Session["User_id"]));
                                            cmdLog.Parameters.AddWithValue("@act", "Unpublish");
                                            cmdLog.Parameters.AddWithValue("@rsn", string.IsNullOrWhiteSpace(reason) ? (object)DBNull.Value : reason);
                                            cmdLog.ExecuteNonQuery();
                                        }

                                        txX.Commit();
                                        WriteMessage("已下架：该事件不再出现在不安全事件查询里。", false);
                                        BindPending();
                                        ClearMessage();
                                        return;
                                    }
                                    catch (Exception ex)
                                    {
                                        txX.Rollback();
                                        WriteMessage("下架失败：" + ex.Message, true);
                                        return;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteMessage("系统错误：" + ex.Message, true);
                        }
                    }
                    break;
                case "Reject":
                    if (string.IsNullOrWhiteSpace(reason))
                    {
                        WriteMessage("请填写驳回理由。", true);
                        return;
                    }
                    newStatus = "已驳回";
                    break;
                default:
                    return;
                case "DeleteAbnormal":
                    {
                        int userType2 = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);
                        if (userType2 != 1 && userType2 != 3)
                        {
                            WriteMessage("无权限：仅管理员或审核人员可删除异常数据。", true);
                            return;
                        }

                        string connStr2 = GetConnString();
                        if (string.IsNullOrEmpty(connStr2))
                        {
                            WriteMessage("无法获取数据库连接字符串。", true);
                            return;
                        }
                        try
                        {
                            using (var conn2 = new SqlConnection(connStr2))
                            {
                                conn2.Open();
                                using (var tx = conn2.BeginTransaction())
                                {
                                    try
                                    {
                                        using (var cmdDelLog = new SqlCommand("DELETE FROM incident_audit_log WHERE Incident_id=@id", conn2, tx))
                                        {
                                            cmdDelLog.Parameters.AddWithValue("@id", incidentId);
                                            cmdDelLog.ExecuteNonQuery();
                                        }
                                        using (var cmdDelInc = new SqlCommand("DELETE FROM incident_info WHERE Incident_id=@id", conn2, tx))
                                        {
                                            cmdDelInc.Parameters.AddWithValue("@id", incidentId);
                                            int rows = cmdDelInc.ExecuteNonQuery();
                                            if (rows <= 0)
                                            {
                                                tx.Rollback();
                                                WriteMessage("删除失败：未找到事件。", true);
                                                return;
                                            }
                                        }
                                        tx.Commit();
                                        WriteMessage("删除成功：已移除异常数据。", false);
                                        if (txtReason != null) { txtReason.Text = string.Empty; }
                                        BindPending();
                                        ClearMessage();
                                    }
                                    catch (Exception ex)
                                    {
                                        tx.Rollback();
                                        WriteMessage("删除失败：" + ex.Message, true);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteMessage("系统错误：" + ex.Message, true);
                        }
                        return;
                    }
                case "DeleteAny":
                    {
                        int userType3 = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);
                        if (userType3 != 1 && userType3 != 3)
                        {
                            WriteMessage("无权限：仅管理员或审核人员可删除。", true);
                            return;
                        }

                        string connStr3 = GetConnString();
                        if (string.IsNullOrEmpty(connStr3))
                        {
                            WriteMessage("无法获取数据库连接字符串。", true);
                            return;
                        }
                        try
                        {
                            using (var conn3 = new SqlConnection(connStr3))
                            {
                                conn3.Open();
                                using (var tx3 = conn3.BeginTransaction())
                                {
                                    try
                                    {
                                        using (var cmdDelLog = new SqlCommand("DELETE FROM incident_audit_log WHERE Incident_id=@id", conn3, tx3))
                                        {
                                            cmdDelLog.Parameters.AddWithValue("@id", incidentId);
                                            cmdDelLog.ExecuteNonQuery();
                                        }
                                        using (var cmdDelInc = new SqlCommand("DELETE FROM incident_info WHERE Incident_id=@id", conn3, tx3))
                                        {
                                            cmdDelInc.Parameters.AddWithValue("@id", incidentId);
                                            int rows = cmdDelInc.ExecuteNonQuery();
                                            if (rows <= 0)
                                            {
                                                tx3.Rollback();
                                                WriteMessage("删除失败：未找到事件。", true);
                                                return;
                                            }
                                        }
                                        tx3.Commit();
                                        WriteMessage("删除成功。", false);
                                        if (txtReason != null) { txtReason.Text = string.Empty; }
                                        BindPending();
                                        ClearMessage();
                                    }
                                    catch (Exception ex)
                                    {
                                        tx3.Rollback();
                                        WriteMessage("删除失败：" + ex.Message, true);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteMessage("系统错误：" + ex.Message, true);
                        }
                        return;
                    }
            }

            int auditorId = Session["User_id"] == null ? 0 : Convert.ToInt32(Session["User_id"]);
            if (auditorId <= 0)
            {
                WriteMessage("请先登录。", true);
                return;
            }

            string connStr = GetConnString();
            if (string.IsNullOrEmpty(connStr))
            {
                WriteMessage("无法获取数据库连接字符串。", true);
                return;
            }

            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string updateSql = "UPDATE incident_info SET Incident_status=@st WHERE Incident_id=@id";
                            using (var cmd = new SqlCommand(updateSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@st", newStatus);
                                cmd.Parameters.AddWithValue("@id", incidentId);
                                cmd.ExecuteNonQuery();
                            }

                            string logSql = @"
                                INSERT INTO incident_audit_log(Incident_id, Auditor_id, Action, Reason) 
                                VALUES(@iid, @aid, @act, @rsn)";
                            using (var logCmd = new SqlCommand(logSql, conn, transaction))
                            {
                                logCmd.Parameters.AddWithValue("@iid", incidentId);
                                logCmd.Parameters.AddWithValue("@aid", auditorId);
                                logCmd.Parameters.AddWithValue("@act", action);
                                logCmd.Parameters.AddWithValue("@rsn", string.IsNullOrWhiteSpace(reason) ? (object)DBNull.Value : reason);
                                logCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            WriteMessage("操作成功，状态已更新为：" + newStatus, false);

                            if (txtReason != null) { txtReason.Text = string.Empty; }

                            BindPending();
                            ClearMessage();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteMessage("操作失败：" + ex.Message, true);
            }
        }

        /// <summary>
        /// ShowDetail（小白讲解）：
        /// 按事件编号读取数据库完整信息，在页面下方的“事件详情”区展示全文。
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
        /// btnCloseDetail_Click（小白讲解）：点击“收起”按钮隐藏详情区。
        /// </summary>
        protected void btnCloseDetail_Click(object sender, EventArgs e)
        {
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var pnl = cph == null ? null : cph.FindControl("pnlDetail") as System.Web.UI.WebControls.Panel;
            if (pnl != null) pnl.Visible = false;
        }
    }
}
