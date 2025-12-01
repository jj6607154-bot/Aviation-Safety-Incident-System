using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class IncidentInfoManage : System.Web.UI.Page
    {
        // ========================================================================
        // 1. 工具方法与辅助类
        // ========================================================================

        /// <summary>
        /// 获取数据库连接字符串（小白讲解）：从配置读取 AviationDb。
        /// </summary>
        private string GetConnString()
        {
            var connStr = ConfigurationManager.ConnectionStrings["AviationDb"];
            return connStr != null ? connStr.ConnectionString : string.Empty;
        }

        /// <summary>
        /// WriteMessage（小白讲解）：在页面顶部显示提示信息。
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
        }

        /// <summary>
        /// ClearMessage（小白讲解）：清空提示并恢复默认样式。
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
        /// 状态徽章样式（小白讲解）：根据状态返回对应的样式类名。
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

        /// <summary>
        /// 下拉框选项类（小白讲解）。
        /// </summary>
        public class DropdownOption
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        /// <summary>
        /// 获取事件类型列表（小白讲解）。
        /// </summary>
        private List<DropdownOption> GetIncidentTypes()
        {
            return new List<DropdownOption>
            {
                new DropdownOption { Text = "全部类型", Value = "" },
                new DropdownOption { Text = "事故", Value = "事故" },
                new DropdownOption { Text = "事故征候", Value = "事故征候" },
                new DropdownOption { Text = "一般事件", Value = "一般事件" }
            };
        }

        /// <summary>
        /// 获取事件状态列表（小白讲解）。
        /// </summary>
        private List<DropdownOption> GetIncidentStatuses()
        {
            return new List<DropdownOption>
            {
                new DropdownOption { Text = "已上报", Value = "已上报" },
                new DropdownOption { Text = "待审核", Value = "待审核" },
                new DropdownOption { Text = "处理中", Value = "处理中" },
                new DropdownOption { Text = "已驳回", Value = "已驳回" },
                new DropdownOption { Text = "待重新提交", Value = "待重新提交" }
            };
        }

        // ========================================================================
        // 2. 页面初始化与数据加载
        // ========================================================================

        /// <summary>
        /// Page_Load（小白讲解）：权限检查 + 首次绑定下拉和数据。
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            int userType = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);
            // 仅审核员(3)可访问事件管理页
            if (userType != 3)
            {
                WriteMessage("无权限访问：仅审核人员可访问事件管理。", true);
                var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
                var panel = cph == null ? null : cph.FindControl("pnlManageContent") as System.Web.UI.WebControls.Panel;
                if (panel != null) { panel.Visible = false; }
                return;
            }

            var navAudit = Master.FindControl("MainContent").FindControl("navAudit") as HyperLink;
            var navUserManage = Master.FindControl("MainContent").FindControl("navUserManage") as HyperLink;
            if (navAudit != null) navAudit.Visible = true;
            if (navUserManage != null) navUserManage.Visible = (userType == 1);

            if (!IsPostBack)
            {
                ClearMessage();
                WriteMessage("提醒：审核操作请前往<a href='IncidentAudit.aspx'>审核页</a>进行处理。", false);
                BindSearchTypeDropdown();
                ClearAllTextBoxes();
                BindGrid();
            }
        }

        /// <summary>
        /// 清空页面上的所有文本框（小白讲解）：
        /// 进入事件管理页时，把输入框内容统一清空，避免遗留上次的查询或备注。
        /// 仅清空 TextBox，不影响下拉框与表格。
        /// </summary>
        private void ClearAllTextBoxes()
        {
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            if (cph == null) return;

            var txts = new[]
            {
                cph.FindControl("txtReason") as TextBox,
                cph.FindControl("txtSearchLocation") as TextBox,
                cph.FindControl("txtSearchStart") as TextBox,
                cph.FindControl("txtSearchEnd") as TextBox
            };

            foreach (var t in txts)
            {
                if (t != null) t.Text = string.Empty;
            }
        }

        /// <summary>
        /// 绑定搜索类型下拉框（小白讲解）。
        /// </summary>
        private void BindSearchTypeDropdown()
        {
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var ddl = cph == null ? null : cph.FindControl("ddlSearchType") as System.Web.UI.WebControls.DropDownList;
            if (ddl != null)
            {
                ddl.DataSource = GetIncidentTypes();
                ddl.DataTextField = "Text";
                ddl.DataValueField = "Value";
                ddl.DataBind();
            }
        }

        /// <summary>
        /// BindGrid（小白讲解）：根据搜索条件查询并绑定管理列表。
        /// </summary>
        private void BindGrid()
        {
            string connStr = GetConnString();
            if (string.IsNullOrEmpty(connStr))
            {
                WriteMessage("无法获取数据库连接字符串。", true);
                return;
            }

            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var ddlType = cph.FindControl("ddlSearchType") as DropDownList;
            var txtLoc = cph.FindControl("txtSearchLocation") as TextBox;
            var txtStart = cph.FindControl("txtSearchStart") as TextBox;
            var txtEnd = cph.FindControl("txtSearchEnd") as TextBox;

            string type = ddlType != null ? ddlType.SelectedValue : string.Empty;
            string loc = txtLoc != null ? (txtLoc.Text ?? string.Empty).Trim() : string.Empty;
            string start = txtStart != null ? (txtStart.Text ?? string.Empty).Trim() : string.Empty;
            string end = txtEnd != null ? (txtEnd.Text ?? string.Empty).Trim() : string.Empty;

            string sql = @"SELECT Incident_id, Incident_type, Occur_time, Report_time, Location, Description, Incident_status 
                           FROM incident_info 
                           WHERE 1=1 ";

            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(type))
            {
                sql += " AND Incident_type = @type";
                parameters.Add(new SqlParameter("@type", type));
            }
            if (!string.IsNullOrEmpty(loc))
            {
                sql += " AND Location LIKE @loc";
                parameters.Add(new SqlParameter("@loc", "%" + loc + "%"));
            }
            if (!string.IsNullOrEmpty(start))
            {
                sql += " AND Occur_time >= @start";
                parameters.Add(new SqlParameter("@start", start));
            }
            if (!string.IsNullOrEmpty(end))
            {
                DateTime endDate;
                if (DateTime.TryParse(end, out endDate))
                {
                    sql += " AND Occur_time < @end";
                    parameters.Add(new SqlParameter("@end", endDate.AddDays(1).ToString("yyyy-MM-dd")));
                }
            }

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

                        var gridIncident = cph.FindControl("gvIncident") as GridView;
                        var gridPending = cph.FindControl("gvPending") as GridView;
                        if (gridIncident != null)
                        {
                            gridIncident.DataSource = dt;
                            gridIncident.DataBind();
                        }
                        else if (gridPending != null)
                        {
                            gridPending.DataSource = dt;
                            gridPending.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteMessage("查询数据失败：" + ex.Message, true);
            }
        }

        /// <summary>
        /// btnSearch_Click（小白讲解）：点击“查询”时重置页码并刷新。
        /// </summary>
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            ClearMessage();
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var grid = cph.FindControl("gvIncident") as GridView;
            if (grid != null) grid.PageIndex = 0;
            BindGrid();
        }

        /// <summary>
        /// btnRefresh_Click（小白讲解）：当你点击“刷新列表”按钮时执行。
        /// 我们不改变当前页码，只是重新去数据库读取一次数据，并刷新表格显示。
        /// 这样可以快速看到刚刚的新增/修改是否已经生效。
        /// </summary>
        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearMessage();
            BindGrid();
        }


        /// <summary>
        /// gvIncident_PageIndexChanging（小白讲解）：翻页并刷新。
        /// </summary>
        protected void gvIncident_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ClearMessage();
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var grid = cph.FindControl("gvIncident") as GridView;
            if (grid != null)
            {
                grid.PageIndex = e.NewPageIndex;
                BindGrid();
            }
        }

        /// <summary>
        /// gvPending_PageIndexChanging（小白讲解）：
        /// 这是为了兼容当前页面使用了 ID 为 gvPending 的 GridView。
        /// 翻页时设置新页并刷新数据。
        /// </summary>
        protected void gvPending_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ClearMessage();
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var grid = cph.FindControl("gvPending") as GridView;
            if (grid != null)
            {
                grid.PageIndex = e.NewPageIndex;
                BindGrid();
            }
        }

        /// <summary>
        /// gvIncident_RowDataBound（小白讲解）：行绑定时计算序号并绑定编辑下拉。
        /// </summary>
        protected void gvIncident_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridView grid = (GridView)sender;
                int serial = e.Row.RowIndex + 1 + (grid.PageIndex * grid.PageSize);
                var lbl = e.Row.FindControl("lblSerial") as Label;
                if (lbl != null) lbl.Text = serial.ToString();

                if ((e.Row.RowState & DataControlRowState.Edit) > 0)
                {
                    var ddlType = e.Row.FindControl("ddlEditType") as DropDownList;
                    var hfType = e.Row.FindControl("hfOriginType") as HiddenField;
                    if (ddlType != null && hfType != null)
                    {
                        var types = GetIncidentTypes();
                        types.RemoveAt(0);
                        ddlType.DataSource = types;
                        ddlType.DataTextField = "Text";
                        ddlType.DataValueField = "Value";
                        ddlType.DataBind();
                        ddlType.SelectedValue = hfType.Value;
                    }

                    var ddlStatus = e.Row.FindControl("ddlEditStatus") as DropDownList;
                    var hfStatus = e.Row.FindControl("hfOriginStatus") as HiddenField;
                    if (ddlStatus != null && hfStatus != null)
                    {
                        ddlStatus.DataSource = GetIncidentStatuses();
                        ddlStatus.DataTextField = "Text";
                        ddlStatus.DataValueField = "Value";
                        ddlStatus.DataBind();
                        ddlStatus.SelectedValue = hfStatus.Value;
                    }
                }
            }
        }

        /// <summary>
        /// gvPending_RowDataBound（小白讲解）：
        /// 兼容 gvPending 的行绑定事件，计算序号并显示在 lblSerial；
        /// 同时根据状态与角色控制“下架-取消公开”按钮可见性（仅审核员，且状态为已公开）。
        /// </summary>
        protected void gvPending_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridView grid = (GridView)sender;
                int serial = e.Row.RowIndex + 1 + (grid.PageIndex * grid.PageSize);
                var lbl = e.Row.FindControl("lblSerial") as Label;
                if (lbl != null) lbl.Text = serial.ToString();

                var btnUnpublish = e.Row.FindControl("btnUnpublish") as Button;
                var drv = e.Row.DataItem as System.Data.DataRowView;
                string status = drv == null ? string.Empty : (drv["Incident_status"].ToString());
                int userType = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);
                if (btnUnpublish != null)
                {
                    btnUnpublish.Visible = (string.Equals(status, "已公开", StringComparison.Ordinal) && userType == 3);
                }
            }
        }

        /// <summary>
        /// gvPending_RowCommand（小白讲解）：处理“下架-取消公开”操作。
        /// 审核员点击后将已公开事件的状态改回“已上报”，相当于取消公开。
        /// </summary>
        protected void gvPending_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            ClearMessage();
            if (!string.Equals(e.CommandName, "Unpublish", StringComparison.Ordinal))
            {
                return;
            }

            int userType = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);
            if (userType != 3)
            {
                WriteMessage("无权限：仅审核员可以下架公开的事件。", true);
                return;
            }

            int incidentId = 0;
            int.TryParse(e.CommandArgument == null ? "0" : e.CommandArgument.ToString(), out incidentId);
            if (incidentId <= 0)
            {
                WriteMessage("无效的事件 ID。", true);
                return;
            }

            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var txtReason = cph == null ? null : cph.FindControl("txtReason") as System.Web.UI.WebControls.TextBox;
            string reason = txtReason == null ? string.Empty : (txtReason.Text ?? string.Empty).Trim();

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

                    // 检查当前状态是否为“已公开”
                    string checkSql = "SELECT TOP 1 Incident_status FROM incident_info WHERE Incident_id=@id";
                    using (var cmdCheck = new SqlCommand(checkSql, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@id", incidentId);
                        var obj = cmdCheck.ExecuteScalar();
                        var st = obj == null ? string.Empty : obj.ToString();
                        if (!string.Equals(st, "已公开", StringComparison.Ordinal))
                        {
                            WriteMessage("当前状态不是已公开，无法下架。", true);
                            return;
                        }
                    }

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 下架：把状态改为“已下架”，保留记录
                            string updateSql = "UPDATE incident_info SET Incident_status=N'已下架' WHERE Incident_id=@id";
                            using (var cmdUpdate = new SqlCommand(updateSql, conn, transaction))
                            {
                                cmdUpdate.Parameters.AddWithValue("@id", incidentId);
                                cmdUpdate.ExecuteNonQuery();
                            }

                            // 写入审核日志
                            string logSql = @"INSERT INTO incident_audit_log(Incident_id, Auditor_id, Action, Reason) VALUES(@iid, @aid, @act, @rsn)";
                            using (var logCmd = new SqlCommand(logSql, conn, transaction))
                            {
                                int auditorId = Session["User_id"] == null ? 0 : Convert.ToInt32(Session["User_id"]);
                                logCmd.Parameters.AddWithValue("@iid", incidentId);
                                logCmd.Parameters.AddWithValue("@aid", auditorId);
                                logCmd.Parameters.AddWithValue("@act", "Unpublish");
                                logCmd.Parameters.AddWithValue("@rsn", string.IsNullOrWhiteSpace(reason) ? (object)DBNull.Value : reason);
                                logCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            WriteMessage("操作成功：已下架。", false);
                            if (txtReason != null) txtReason.Text = string.Empty;
                            BindGrid();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            WriteMessage("下架失败：" + ex.Message, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteMessage("操作异常：" + ex.Message, true);
            }
        }

        /// <summary>
        /// gvIncident_RowEditing（小白讲解）：进入编辑模式。
        /// </summary>
        protected void gvIncident_RowEditing(object sender, GridViewEditEventArgs e)
        {
            ClearMessage();
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var grid = cph.FindControl("gvIncident") as GridView;
            if (grid != null)
            {
                grid.EditIndex = e.NewEditIndex;
                BindGrid();
            }
        }

        /// <summary>
        /// gvIncident_RowCancelingEdit（小白讲解）：取消编辑。
        /// </summary>
        protected void gvIncident_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            ClearMessage();
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var grid = cph.FindControl("gvIncident") as GridView;
            if (grid != null)
            {
                grid.EditIndex = -1;
                BindGrid();
            }
        }

        /// <summary>
        /// gvIncident_RowUpdating（小白讲解）：保存编辑更改。
        /// </summary>
        protected void gvIncident_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ClearMessage();
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var grid = cph.FindControl("gvIncident") as GridView;
            if (grid == null) return;

            int incidentId = Convert.ToInt32(grid.DataKeys[e.RowIndex].Value);

            GridViewRow row = grid.Rows[e.RowIndex];
            var ddlType = row.FindControl("ddlEditType") as DropDownList;
            var ddlStatus = row.FindControl("ddlEditStatus") as DropDownList;
            var txtDesc = row.FindControl("txtEditDesc") as TextBox;

            string newType = ddlType.SelectedValue;
            string newStatus = ddlStatus.SelectedValue;
            string newDesc = (txtDesc.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(newDesc))
            {
                WriteMessage("事件描述不能为空。", true);
                return;
            }

            string connStr = GetConnString();
            string sql = @"UPDATE incident_info 
                           SET Incident_type = @type, 
                               Incident_status = @status, 
                               Description = @desc 
                           WHERE Incident_id = @id";

            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@type", newType);
                    cmd.Parameters.AddWithValue("@status", newStatus);
                    cmd.Parameters.AddWithValue("@desc", newDesc);
                    cmd.Parameters.AddWithValue("@id", incidentId);

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        WriteMessage("事件信息已更新成功。", false);
                        grid.EditIndex = -1;
                        BindGrid();
                    }
                    else
                    {
                        WriteMessage("更新失败，未找到指定记录。", true);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteMessage("更新数据库失败：" + ex.Message, true);
            }
        }

        /// <summary>
        /// gvIncident_RowDeleting（小白讲解）：删除事件（演示用）。
        /// </summary>
        protected void gvIncident_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            ClearMessage();
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            var grid = cph.FindControl("gvIncident") as GridView;
            if (grid == null) return;

            int incidentId = Convert.ToInt32(grid.DataKeys[e.RowIndex].Value);

            string connStr = GetConnString();
            string sql = "DELETE FROM incident_info WHERE Incident_id = @id";

            try
            {
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", incidentId);
                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        WriteMessage("事件记录已成功删除。", false);
                        if (grid.Rows.Count == 1 && grid.PageIndex > 0)
                        {
                            grid.PageIndex--;
                        }
                        BindGrid();
                    }
                    else
                    {
                        WriteMessage("删除失败，未找到指定记录。", true);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 547)
                {
                    WriteMessage("删除失败：该事件已被其他记录引用（如审计日志），无法直接删除。", true);
                }
                else
                {
                    WriteMessage("数据库删除错误：" + sqlEx.Message, true);
                }
            }
            catch (Exception ex)
            {
                WriteMessage("删除操作失败：" + ex.Message, true);
            }
        }
    }
}
