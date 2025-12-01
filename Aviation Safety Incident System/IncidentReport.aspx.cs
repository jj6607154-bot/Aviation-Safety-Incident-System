using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using WebFormsOptions;

namespace Aviation_Safety_Incident_System
{
    public partial class IncidentReport : System.Web.UI.Page
    {
        /// <summary>
        /// 页面加载（小白讲解）：
        /// - 首次加载统一绑定下拉选项，和“不安全事件查询”页保持一致；
        /// - 若未登录：提示并引导去登录；
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["User_id"] == null)
                {
                    WriteMessage("请先登录后再进行事件上报。", true);
                }

                // 角色限制（小白讲解）：此页仅普通用户(type=2)用于上报，管理员/审核员不参与上报。
                int userType = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);
                if (userType != 2)
                {
                    WriteMessage("此页面仅供普通用户上报事件。请使用审核页或首页导航。", true);
                    if (btnSubmit != null) btnSubmit.Enabled = false;
                    if (ddlType != null) ddlType.Enabled = false;
                    if (ddlLocation != null) ddlLocation.Enabled = false;
                    if (txtOccurTime != null) txtOccurTime.Enabled = false;
                    if (txtDescription != null) txtDescription.Enabled = false;
                }

                // 统一绑定事件类型与地点（与查询页一致）
                BindDropdowns();
            }
        }

        /// <summary>
        /// 绑定统一下拉（小白讲解）：
        /// 事件类型固定为：事故、事故征候、一般事件；
        /// 地点固定为：跑道、滑行道、登机口、候机楼、机库、航站楼；
        /// </summary>
        private void BindDropdowns()
        {
            DropdownOptions.BindIncidentTypes(ddlType, includeAll: false);
            // 在首项插入“请选择”
            ddlType.Items.Insert(0, new ListItem("请选择", ""));

            DropdownOptions.BindLocations(ddlLocation, includeCustom: false);
            ddlLocation.Items.Insert(0, new ListItem("请选择", ""));
        }

        /// <summary>
        /// 提交上报按钮（小白讲解）：
        /// 读取你在表单中填写的事件类型、时间、地点和描述，
        /// 调用数据库存储过程 sp_InsertIncident 插入到 incident_info 表，
        /// 并将状态设为“待审核”、自动记录上报时间。
        /// </summary>
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            // 1) 登录校验（小白讲解）：必须先登录才能上报事件
            int userId = Session["User_id"] == null ? 0 : Convert.ToInt32(Session["User_id"]);
            if (userId <= 0)
            {
                WriteMessage("请先登录后再提交事件。", true);
                return;
            }

            // 2) 读取并整理表单值
            string finalType = (ddlType.SelectedValue ?? string.Empty).Trim();

            string occurText = (txtOccurTime.Text ?? string.Empty).Trim();
            DateTime occurDate;
            if (!DateTime.TryParse(occurText, out occurDate))
            {
                WriteMessage("请正确填写发生日期。", true);
                return;
            }
            DateTime occurTime = occurDate.Date;

            string finalLocation = (ddlLocation.SelectedValue ?? string.Empty).Trim();

            string description = (txtDescription.Text ?? string.Empty).Trim();

            // 3) 表单校验（小白讲解）：保证必填项不为空、描述不少于20字
            if (string.IsNullOrEmpty(finalType))
            {
                WriteMessage("请选择事件类型。", true);
                return;
            }
            if (string.IsNullOrEmpty(finalLocation))
            {
                WriteMessage("请选择事件地点。", true);
                return;
            }
            if (string.IsNullOrEmpty(description) || description.Length < 20)
            {
                WriteMessage("详细描述不少于20字，请补充完整。", true);
                return;
            }

            // 4) 调用存储过程插入（小白讲解）：统一设置状态为“待审核”，自动记录上报时间
            string connStr = ConfigurationManager.ConnectionStrings["AviationDb"].ConnectionString;
            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    try
                    {
                        using (var cmd = new SqlCommand("dbo.sp_InsertIncident", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Incident_type", finalType);
                            cmd.Parameters.AddWithValue("@Occur_time", occurTime);
                            cmd.Parameters.AddWithValue("@Location", finalLocation);
                            cmd.Parameters.AddWithValue("@Description", description);
                            cmd.Parameters.AddWithValue("@User_id", userId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException sx)
                    {
                        // 小白讲解：如果数据库里还没有创建存储过程，就直接用普通 INSERT 方式写入
                        if (sx.Number == 2812 || sx.Message.IndexOf("could not find stored procedure", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            using (var cmd2 = new SqlCommand(@"INSERT INTO dbo.incident_info(Incident_type, Occur_time, Location, Description, User_id, Report_time, Incident_status)
                                                              VALUES(@t, @ot, @loc, @desc, @uid, GETDATE(), N'待审核')", conn))
                            {
                                cmd2.Parameters.AddWithValue("@t", finalType);
                                cmd2.Parameters.AddWithValue("@ot", occurTime);
                                cmd2.Parameters.AddWithValue("@loc", finalLocation);
                                cmd2.Parameters.AddWithValue("@desc", description);
                                cmd2.Parameters.AddWithValue("@uid", userId);
                                cmd2.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                // 5) 成功反馈与留在本页（小白讲解）：不跳转，清空表单，方便继续上报
                WriteMessage("上报成功！已提交给审核员。", false);
                try
                {
                    // 清空文本框与选择（小白讲解）：把你刚填的内容清掉，避免重复提交同一份
                    txtOccurTime.Text = string.Empty;
                    txtDescription.Text = string.Empty;
                    if (ddlType.Items.Count > 0)
                    {
                        ddlType.ClearSelection();
                        // 尝试选中“请选择”占位项
                        ListItem placeholder = ddlType.Items.FindByValue("");
                        if (placeholder != null) placeholder.Selected = true;
                    }
                    if (ddlLocation.Items.Count > 0)
                    {
                        ddlLocation.ClearSelection();
                        ListItem placeholder2 = ddlLocation.Items.FindByValue("");
                        if (placeholder2 != null) placeholder2.Selected = true;
                    }
                }
                catch { /* 保护性处理：即使清空失败也不影响成功提示 */ }
            }
            catch (Exception ex)
            {
                WriteMessage("提交失败：" + ex.Message, true);
            }
        }

        /// <summary>
        /// 输出提示信息（小白讲解）：
        /// 用一个页面上的 Label 显示成功/失败消息，并用背景色做区分。
        /// </summary>
        private void WriteMessage(string message, bool isError)
        {
            if (lblMessage != null)
            {
                lblMessage.Text = message;
                lblMessage.Style["background"] = isError ? "#fff1f0" : "#f6ffed";
                lblMessage.Style["border"] = isError ? "1px solid #ffa39e" : "1px solid #b7eb8f";
                lblMessage.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;
            }
            else
            {
                Response.Write(message);
            }
        }
    }
}
