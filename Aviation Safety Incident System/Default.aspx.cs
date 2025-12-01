using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

// 命名空间必须是 WebForms（与前端Inherits="WebForms.Default"一致）
namespace WebForms
{
    // 类名必须是 Default（与前端Inherits="WebForms.Default"一致），且是 partial class
    public partial class Default : System.Web.UI.Page
    {
        /// <summary>
        /// 获取数据库连接字符串（从Web.config读取）
        /// </summary>
        private string GetConnString()
        {
            ConnectionStringSettings connectionItem = ConfigurationManager.ConnectionStrings["AviationDb"];
            return connectionItem == null ? string.Empty : connectionItem.ConnectionString;
        }

        /// <summary>
        /// 获取ApiBase配置（预留接口调用）
        /// </summary>
        private string GetApiBase()
        {
            return ConfigurationManager.AppSettings["ApiBase"] ?? "http://localhost:3000/api";
        }

        /// <summary>
        /// 页面加载事件
        /// </summary>
        /// <summary>
        /// 页面加载事件（小白讲解）：
        /// - 未登录：允许浏览首页，不再强制跳转；仅显示公开内容与登录入口
        /// - 已登录：显示欢迎信息、新闻列表、消息提醒等完整内容
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            // 小白讲解：支持通过地址触发退出（例如 Default.aspx?logout=1）
            // 这样在不使用母版页的页面也能使用统一的退出逻辑。
            if (string.Equals(Request.QueryString["logout"], "1", StringComparison.OrdinalIgnoreCase))
            {
                // 清理会话并跳到登录页
                Session.Clear();
                Session.Abandon();
                Response.Redirect("~/Login.aspx?clear=1");
                return;
            }

            if (!IsPostBack)
            {
                bool isLoggedIn = (Session["User_id"] != null && Session["User_type"] != null);
                if (!isLoggedIn)
                {
                    SetupAnonymousHome();
                    return;
                }

                BindWelcome();
                AdjustLinksByRole();
                BindMessageReminders();
            }
        }

        /// <summary>
        /// 绑定欢迎信息
        /// </summary>
        /// <summary>
        /// 绑定欢迎信息（小白讲解）：
        /// - 未登录：显示“欢迎访问（未登录）”
        /// - 已登录：显示“欢迎回来，用户名（角色）”
        /// </summary>
        private void BindWelcome()
        {
            if (lblWelcome != null)
            {
                if (Session["User_type"] == null)
                {
                    lblWelcome.Text = "欢迎访问（未登录）";
                    return;
                }

                string userName = Session["User_name"] != null ? Session["User_name"].ToString() : "未知用户";
                int userType = Convert.ToInt32(Session["User_type"]);
                string roleName = "默认用户";

                switch (userType)
                {
                    case 1:
                        roleName = "管理员";
                        break;
                    case 2:
                        roleName = "普通用户";
                        break;
                    case 3:
                        roleName = "审核人员";
                        break;
                    default:
                        roleName = "默认用户";
                        break;
                }

                lblWelcome.Text = string.Format("欢迎回来，{0}（{1}）", userName, roleName);
            }
        }

        /// <summary>
        /// 根据角色调整导航链接（小白解读）：
        /// - 普通用户：进入只读查询页 IncidentQuery.aspx（查看事件，不可编辑）。
        /// - 管理员 / 审核人员：进入管理页 IncidentInfoManage.aspx（查询+上报+编辑）。
        /// 这样不同角色进入合适的功能页面，避免误操作或权限不足。
        /// </summary>
        /// <summary>
        /// 根据角色调整首页导航（小白讲解）：
        /// - 未登录：显示“登录/注册”，隐藏“事件管理/发布/用户管理/消息提醒”，保留公开入口
        /// - 登录后：按角色显示对应的功能入口
        /// </summary>
        private void AdjustLinksByRole()
        {
            if (Session["User_type"] == null)
            {
                if (lnkToLogin != null) lnkToLogin.Visible = true;
                if (btnLogout != null) btnLogout.Visible = false;
                if (lnkToIncidents != null) lnkToIncidents.Visible = false;
                if (lnkToNewsManage != null) lnkToNewsManage.Visible = false;
                if (lnkToUserManage != null) lnkToUserManage.Visible = false;
                if (lnkMessages != null) lnkMessages.Visible = false;
                return;
            }

            int userType = Convert.ToInt32(Session["User_type"]);

            if (lnkToLogin != null) lnkToLogin.Visible = false;
            if (btnLogout != null) btnLogout.Visible = true;

            if (lnkToIncidents != null)
            {
                if (userType == 2) // 普通用户
                {
                    lnkToIncidents.NavigateUrl = "~/IncidentQuery.aspx";
                    lnkToIncidents.Text = "事件查询";
                    lnkToIncidents.Visible = true;
                }
                else if (userType == 3) // 审核员
                {
                    lnkToIncidents.NavigateUrl = "~/IncidentAudit.aspx";
                    lnkToIncidents.Text = "事件审核";
                    lnkToIncidents.Visible = true;
                }
                else // 管理员等不显示此组件
                {
                    lnkToIncidents.Visible = false;
                }
            }

            if (userType == 1)
            {
                if (lnkToNewsManage != null) lnkToNewsManage.Visible = true;
                if (lnkToUserManage != null) lnkToUserManage.Visible = true;
            }
        }

        /// <summary>
        /// 绑定消息提醒（小白解读）：
        /// 普通用户会看到“消息提醒(N)”链接，N 为该用户的事件中处于“已驳回/待补充”的数量，
        /// 点击进入“我的消息”页查看具体是哪个事件、审核人员的驳回/补充理由。
        /// </summary>
        /// <summary>
        /// 绑定消息提醒（小白讲解）：
        /// - 未登录：不显示消息提醒
        /// - 普通用户：显示其待补充/已驳回数量
        /// </summary>
        private void BindMessageReminders()
        {
            try
            {
                if (Session["User_type"] == null || Session["User_id"] == null)
                {
                    if (lnkMessages != null) lnkMessages.Visible = false;
                    return;
                }

                int userType = Convert.ToInt32(Session["User_type"]);
                int userId = Convert.ToInt32(Session["User_id"]);
                if (userType != 2)
                {
                    if (lnkMessages != null) lnkMessages.Visible = false;
                    return;
                }

                using (var conn = new SqlConnection(GetConnString()))
                using (var cmd = new SqlCommand(@"SELECT COUNT(*) FROM incident_info WHERE User_id=@uid AND (Incident_status = N'已驳回' OR Incident_status = N'待补充')", conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (lnkMessages != null)
                    {
                        lnkMessages.Visible = true;
                        lnkMessages.NavigateUrl = "~/MyMessages.aspx";
                        // 小白讲解：如果你已经看过“我的消息”，我们就把右上角数字隐藏掉，避免一直显示(1)
                        bool viewed = Session["MessagesViewed"] != null && (bool)Session["MessagesViewed"];
                        lnkMessages.Text = (!viewed && count > 0) ? ("消息提醒(" + count + ")") : "消息提醒";
                    }
                }
            }
            catch
            {
                if (lnkMessages != null)
                {
                    lnkMessages.Visible = false;
                }
            }
        }

        /// <summary>
        /// 未登录首页初始化（小白讲解）：
        /// - 只显示公开内容（例如已发布新闻）
        /// - 导航只保留登录入口，隐藏需要权限的链接
        /// </summary>
        private void SetupAnonymousHome()
        {
            BindWelcome();
            AdjustLinksByRole();
        }

        // 首页已移除“最新已发布新闻”板块，相关绑定与分页逻辑一并删除。

        // 小白说明：应用户要求，首页的“最新发布内容”板块已删除；
        // 原方法 BindNewsContent() 已被移除，避免找不到控件 repNews 的编译错误。

        // ==============================================
        // 关键修正：确保以下3点完全正确！
        // 1. 访问修饰符：必须是 protected（不能是 private）
        // 2. 方法名：必须是 btnLogout_Click（与前端OnClick完全一致）
        // 3. 参数：必须是 (object sender, EventArgs e)
        // ==============================================
        /// <summary>
        /// 退出登录事件（前端OnClick绑定的核心方法）
        /// </summary>
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // 清除Session
            Session.Clear();
            Session.Abandon();
            // 跳转登录页（小白讲解）：带上 clear=1 参数，让登录页清空用户名/密码
            Response.Redirect("~/Login.aspx?clear=1");
        }

        /// <summary>
        /// 输出提示信息
        /// </summary>
        private void WriteMessage(string message, bool isError = false)
        {
            if (lblMessage != null)
            {
                lblMessage.Text = message;
                lblMessage.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;
                lblMessage.Style["background-color"] = isError ? "#fff2f0" : "#f0fff4";
                lblMessage.Style["border"] = isError ? "1px solid #ffccc7" : "1px solid #b7eb8f";
            }
            else
            {
                Response.Write(string.Format("<div style='padding:10px;border-radius:4px;margin-bottom:15px;color:{0}'>{1}</div>", isError ? "red" : "green", message));
            }
        }
    }
}
