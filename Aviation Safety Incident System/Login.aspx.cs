using System;
using System.Configuration; // 读取 Web.config 的连接字符串
using System.Data;          // ADO.NET 通用类型
using System.Data.SqlClient; // 连接 SQL Server
using System.Security.Cryptography; // 做 MD5 哈希
using System.Text; // 字符串转字节

namespace WebForms
{
    public partial class Login : System.Web.UI.Page
    {
        /// <summary>
        /// 页面加载（小白讲解）：
        /// 当你从“退出”回来时（地址带 clear=1），我会清空用户名和密码输入框；
        /// 同时关闭浏览器的自动填充，避免退出后又自动填回旧值。
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            // 始终关闭自动填充（浏览器行为，不是服务器存储）
            txtUserName.Attributes["autocomplete"] = "off";
            txtPassword.Attributes["autocomplete"] = "new-password";

            // 仅当明确带了 clear=1 参数时清空（避免登录失败时保留你已输入的内容）
            bool needClear = string.Equals(Request.QueryString["clear"], "1", StringComparison.OrdinalIgnoreCase);
            if (!IsPostBack && needClear)
            {
                txtUserName.Text = string.Empty;
                txtPassword.Text = string.Empty;
                lblMessage.Text = string.Empty;
            }
        }
        /// <summary>
        /// 获取数据库连接字符串（小白解读）：
        /// 从 Web.config 里的 connectionStrings 读取名为 "AviationDb" 的连接串。
        /// </summary>
        private string GetConnString()
        {
            return ConfigurationManager.ConnectionStrings["AviationDb"].ConnectionString;
        }

        /// <summary>
        /// 计算输入密码的 MD5 32位十六进制字符串（小白解读）：
        /// 因为数据库的 password 字段是 Char(32)，通常用来存 MD5 值。
        /// 这个函数会把你输入的明文密码转成 MD5 的 32 位小写字符串，用来和库里比对。
        /// </summary>
        private string ComputeMD5Hex(string input)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = md5.ComputeHash(bytes);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2")); // 小写十六进制
                }
                return sb.ToString();
            }
        }

        // 说明（小白讲解）：
        // 登录页已按你的需求移除“邮箱显示”功能，不再查询或显示邮箱字段。

        /// <summary>
        /// 登录按钮点击（小白解读）：
        /// 直接查询数据库的 users_info 表，验证用户名和密码；
        /// 成功后把用户信息放到 Session 并跳到主页；失败则提示错误。
        /// </summary>
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            lblMessage.Text = string.Empty;
            var username = (txtUserName.Text ?? string.Empty).Trim();
            var password = (txtPassword.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "请输入用户名和密码";
                return;
            }

            try
            {
                using (var conn = new SqlConnection(GetConnString()))
                {
                    conn.Open();
                    // 用参数化查询避免 SQL 注入
                    using (var cmd = new SqlCommand("SELECT TOP 1 User_id, User_name, type, password FROM users_info WHERE User_name=@name", conn))
                    {
                        cmd.Parameters.AddWithValue("@name", username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                lblMessage.Text = "用户不存在或密码错误";
                                return;
                            }
                            int userId = reader.GetInt32(0);
                            string userName = reader.GetString(1);
                            int userType = reader.GetInt32(2);
                            string dbPassword = reader.GetString(3);

                            // 把你输入的明文密码做 MD5，然后和数据库里存的 32位值比较
                            string inputMd5 = ComputeMD5Hex(password);
                            if (!string.Equals(dbPassword, inputMd5, StringComparison.OrdinalIgnoreCase))
                            {
                                lblMessage.Text = "用户不存在或密码错误";
                                return;
                            }

                            // 写入 Session，后续页面使用
                            Session["User_id"] = userId;
                            Session["User_name"] = userName;
                            Session["User_type"] = userType;
                            // 基于角色跳转（小白讲解）：
                            // - 普通用户(2)：跳到事件查询页（只读）
                            // - 审核人员(3)：跳到事件审核页（处理待审核事件）
                            // - 管理员(1)：跳到首页（不显示事件管理组件）
                            if (userType == 2)
                            {
                                Response.Redirect("~/IncidentQuery.aspx");
                            }
                            else if (userType == 3)
                            {
                                Response.Redirect("~/IncidentAudit.aspx");
                            }
                            else
                            {
                                Response.Redirect("~/Default.aspx");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "登录异常: " + ex.Message;
            }
        }
    }
}
