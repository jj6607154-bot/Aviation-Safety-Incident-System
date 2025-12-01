using System;

namespace WebForms
{
    public partial class Home : System.Web.UI.Page
    {
        /// <summary>
        /// 小白讲解：这个页面是“占位/兼容页”。
        /// 有人访问 /Home.aspx 时，我们在页面加载阶段直接跳转到真正的首页 Default.aspx，避免 404 错误。
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            // 小白讲解：采用非阻塞重定向 + CompleteRequest，避免控制台日志中的 ERR_ABORTED 干扰
            Response.Redirect("~/Default.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
            return;
        }
    }
}
