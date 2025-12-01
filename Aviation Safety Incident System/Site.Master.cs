using System;
using System.Configuration;
using System.Data.SqlClient;

namespace WebForms
{
    public partial class Site : System.Web.UI.MasterPage
    {
        /// <summary>
        /// 页面加载（小白解读）：
        /// 1）判断你是否已登录：已登录就显示“欢迎，用户名（角色）”，未登录显示“未登录”。
        /// 2）同时控制登录/注册链接的显示：已登录就隐藏登录/注册，未登录就显示。
        /// 3）角色来源说明：登录时会把用户类型写入 Session["User_type"]，
        ///    取值约定为：1=管理员、2=普通用户、3=审核人员。我们用它来显示你的角色。
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            var userNameObj = Session["User_name"];
            string userName = userNameObj == null ? null : userNameObj.ToString();
            // 读取登录时写入的用户类型（小白解读）：
            // 如果还没登录，就拿不到这个值，默认为 0（未知）。
            int userType = 0;
            var typeObj = Session["User_type"];
            if (typeObj != null)
            {
                int.TryParse(typeObj.ToString(), out userType);
            }

            // 把数字类型转换成中文角色名，便于展示
            string roleName;
            switch (userType)
            {
                case 1: roleName = "管理员"; break;
                case 2: roleName = "普通用户"; break;
                case 3: roleName = "审核人员"; break;
                default: roleName = "用户"; break; // 未知或未登录时用“用户”占位
            }

            if (!string.IsNullOrEmpty(userName))
            {
                // 已登录：右上角显示“欢迎，用户名（角色）”
                lblUser.Text = "欢迎，" + userName + "（" + roleName + "）";
                // 导航可见性（小白解读）：
                // - 事件上报：仅普通用户(2)可见；管理员(1)与审核人员(3)隐藏
                //   小白提示：管理员与审核人员不参与“上报”，分别负责“用户管理”和“事件审核”。
                lnkIncidentReport.Visible = (userType == 2);
                if (lnkIncidentAudit != null)
                {
                    lnkIncidentAudit.Visible = (userType == 3);
                }
                // 信息发布入口：所有登录用户均可进入“新闻动态”页浏览；发布权限仅管理员在页内控制
                if (lnkNews != null) lnkNews.Visible = (userType == 1);
                if (lnkNewsPublished != null) lnkNewsPublished.Visible = true;
                // - 安全法律法规：所有登录用户与未登录用户都可以查看，保持可见
                if (lnkRegulations != null)
                {
                    lnkRegulations.Visible = true;
                }
                if (lnkIncidentManage != null)
                {
                    lnkIncidentManage.Visible = false;
                }
                if (lnkUserManage != null)
                {
                    lnkUserManage.Visible = (userType == 1);
                }
                // 退出按钮可见性（小白解读）：
                // 改为：只要你已登录（不管是管理员、普通用户还是审核人员），都显示“退出”。
                btnLogout.Visible = !string.IsNullOrEmpty(userName);

                // 顶部“消息提醒”绑定（小白解读）：仅普通用户显示，管理员/审核人员隐藏
                BindTopMessageReminder(userType);
            }
            else
            {
                lblUser.Text = "未登录";
                // 未登录不允许直接上报或审核
                lnkIncidentReport.Visible = false;
                if (lnkIncidentAudit != null) lnkIncidentAudit.Visible = false;
                // 未登录也允许浏览新闻动态，保持可见（发布区在页内按角色隐藏）
                if (lnkNews != null) lnkNews.Visible = false;
                if (lnkNewsPublished != null) lnkNewsPublished.Visible = true;
                // 未登录也允许浏览法律法规，因此保持可见
                if (lnkRegulations != null) lnkRegulations.Visible = true;
                if (lnkIncidentManage != null) lnkIncidentManage.Visible = false;
                if (lnkUserManage != null) lnkUserManage.Visible = false;
                // 未登录时不显示退出按钮
                btnLogout.Visible = false;
                if (lnkMessagesTop != null) lnkMessagesTop.Visible = false;
            }
        }

        /// <summary>
        /// 获取数据库连接字符串（小白讲解）：
        /// 从 Web.config 里读取名为 AviationDb 的连接字符串，用来查询提醒数量。
        /// </summary>
        private string GetConnString()
        {
            var cs = ConfigurationManager.ConnectionStrings["AviationDb"];
            return cs == null ? string.Empty : cs.ConnectionString;
        }

        /// <summary>
        /// 绑定顶部消息提醒（小白讲解）：
        /// - 仅普通用户(2)显示；文本为“消息提醒”或“消息提醒(N)”；
        /// - 统计你名下事件中“已驳回/待补充/待重新提交”的数量；
        /// - 若你已在“我的消息”页查看过（Session["MessagesViewed"] = true），则不显示数字避免一直提示。
        /// </summary>
        private void BindTopMessageReminder(int userType)
        {
            try
            {
                if (lnkMessagesTop == null) return;

                if (Session["User_id"] == null || userType != 2)
                {
                    lnkMessagesTop.Visible = false;
                    return;
                }

                int userId = Convert.ToInt32(Session["User_id"]);
                using (var conn = new SqlConnection(GetConnString()))
                using (var cmd = new SqlCommand(@"SELECT COUNT(*) FROM incident_info WHERE User_id=@uid AND Incident_status IN (N'已驳回', N'待补充', N'待重新提交')", conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    bool viewed = Session["MessagesViewed"] != null && (bool)Session["MessagesViewed"];
                    lnkMessagesTop.Visible = true;
                    lnkMessagesTop.NavigateUrl = "~/MyMessages.aspx";
                    lnkMessagesTop.Text = (!viewed && count > 0) ? ("消息提醒(" + count + ")") : "消息提醒";
                }
            }
            catch
            {
                if (lnkMessagesTop != null) lnkMessagesTop.Visible = false;
            }
        }

        /// <summary>
        /// btnLogout_Click（小白讲解）：
        /// 当你点“退出”按钮时会执行这个方法。
        /// 我们做两件事：
        /// 1）清理 Session 中的登录信息（相当于“登出”）；
        /// 2）跳转回登录页面，让你可以重新登录。
        /// </summary>
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // 清理登录相关的 Session（小白讲解：把你是谁、什么角色都清空）
            Session["User_name"] = null;
            Session["User_type"] = null;
            Session.Abandon();

            // 跳转到登录页面（小白讲解）：带上 clear=1 参数，提示登录页把输入框清空
            Response.Redirect("~/Login.aspx?clear=1");
        }
    }
}
