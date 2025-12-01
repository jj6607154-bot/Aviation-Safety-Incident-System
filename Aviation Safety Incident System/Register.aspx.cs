using System;
using System.Configuration; // 读取 Web.config 的连接字符串
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography; // 做 MD5 哈希
using System.Text; // 字符串转字节

namespace WebForms
{
    public partial class Register : System.Web.UI.Page
    {
        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        private string GetConnString()
        {
            return ConfigurationManager.ConnectionStrings["AviationDb"].ConnectionString;
        }

        /// <summary>
        /// 获取管理员/审核人员注册口令（小白解读）：
        /// 我们把“允许注册成管理员/审核人员的口令”放在 Web.config 的 appSettings，
        /// 这样你可以自己修改，不用改代码。普通用户不需要口令。
        /// 为了兼容旧版编译器，这里使用 System.Tuple<string,string> 而不是 C# 7 的元组语法。
        /// </summary>
        private Tuple<string, string> GetRoleRegisterKeys()
        {
            // 小白解读：角色口令功能已取消，此方法保留但不再被调用。
            var admin = "";
            var auditor = "";
            return Tuple.Create(admin, auditor);
        }

        /// <summary>
        /// 规范化口令字符串（小白解读）：
        /// 把常见的中文全角破折号/连接号（例如“－”、“—”、“–”）统一替换为半角“-”，
        /// 同时把中文全角空格替换为普通空格并去掉首尾空格，避免输入法导致的匹配失败。
        /// </summary>
        private string NormalizeKey(string s)
        {
            if (s == null) return string.Empty;
            string x = s.Replace('\uFF0D', '-')   // 全角减号 －
                        .Replace('\u2014', '-')   // em dash —
                        .Replace('\u2013', '-')   // en dash –
                        .Replace('\u3000', ' ');  // 全角空格
            return x.Trim();
        }

        /// <summary>
        /// 宽松校验角色口令（小白讲解）：
        /// 当你选择要注册“管理员(1)”或“审核人员(3)”时，需要输入对应的角色口令。
        /// 我们把你输入的口令和配置里的口令都做 Trim（去掉前后空格），并忽略大小写，
        /// 这样可以减少因为大小写或不小心多打了空格导致的失败。
        /// 返回 true 表示口令匹配，false 表示不匹配。
        /// </summary>
        private bool CheckRoleKey(int selectedType, string inputKey)
        {
            var keys = GetRoleRegisterKeys();
            string adminKey = NormalizeKey(keys.Item1 ?? string.Empty);
            string auditorKey = NormalizeKey(keys.Item2 ?? string.Empty);
            string normalizedInput = NormalizeKey(inputKey ?? string.Empty);

            if (selectedType == 1) // 管理员
            {
                return string.Equals(normalizedInput, adminKey, StringComparison.OrdinalIgnoreCase);
            }
            if (selectedType == 3) // 审核人员
            {
                return string.Equals(normalizedInput, auditorKey, StringComparison.OrdinalIgnoreCase);
            }
            return true; // 普通用户(2)不需要口令
        }

        /// <summary>
        /// 计算输入密码的 MD5 32位十六进制字符串（小白解读）：
        /// 注册时我们不存明文密码，而是把你输入的密码做 MD5，
        /// 存入数据库的 password(Char(32)) 字段，更安全也符合字段长度。
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
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 页面加载（小白解读）：
        /// 在注册页上实时显示当前系统配置的“管理员/审核员口令”示例，
        /// 帮助你理解“要输入的是口令的值（例如 AUDITOR-REGISTER-KEY）”，不是配置里的键名。
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // 小白讲解：首次进入注册页时，清空用户名和密码，并把角色默认设为“普通用户”。
                if (!IsPostBack)
                {
                    if (txtUserName != null) txtUserName.Text = string.Empty;
                    if (txtPassword != null) txtPassword.Text = string.Empty;
                    if (ddlRole != null) ddlRole.SelectedValue = "2"; // 默认普通用户
                }
            }
            catch
            {
                // 不抛错：即使界面初始化失败也不影响注册流程
            }
        }

        /// <summary>
        /// 注册按钮点击（小白解读）：
        /// 直接往数据库 users_info 表插入新用户；
        /// 先检查用户名是否已存在，存在则提示；否则插入并提示成功。
        /// </summary>
        protected void btnRegister_Click(object sender, EventArgs e)
        {
            lblMessage.Text = string.Empty;
            var username = (txtUserName.Text ?? string.Empty).Trim();
            var password = (txtPassword.Text ?? string.Empty).Trim();
            // 小白讲解：按你的需求，注册不再收集邮箱；数据库里即使有 email 字段，也不再使用。
            int selectedType = 2; // 默认普通用户
            if (ddlRole != null && !string.IsNullOrWhiteSpace(ddlRole.SelectedValue))
            {
                int.TryParse(ddlRole.SelectedValue, out selectedType);
                if (selectedType == 0) selectedType = 2;
            }

            // 小白讲解：按你的要求，角色直接由下拉框决定，不再需要“角色口令”。

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "请输入用户名和密码";
                return;
            }

            // 用户名长度建议不超过 50（对应表结构 Varchar(50)）
            if (username.Length > 50)
            {
                lblMessage.Text = "用户名长度不能超过 50";
                return;
            }

            try
            {
                using (var conn = new SqlConnection(GetConnString()))
                {
                    conn.Open();
                    // 1) 检查用户名是否已存在
                    using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM users_info WHERE User_name=@name", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@name", username);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (count > 0)
                        {
                            lblMessage.Text = "该用户名已存在，请更换";
                            return;
                        }
                    }

                    // 2) 已去除“角色口令”校验：选择了管理员或审核人员，将按选择直接注册。

                    // 3) 插入新用户（type 取你的选择；普通用户=2 不需要口令）
                    using (var insertCmd = new SqlCommand(
                        "INSERT INTO users_info(User_name, password, type) VALUES(@name, @pwd, @type)", conn))
                    {
                        insertCmd.Parameters.AddWithValue("@name", username);
                        // 把明文密码转成 MD5 32位字符串再存入数据库
                        insertCmd.Parameters.AddWithValue("@pwd", ComputeMD5Hex(password));
                        insertCmd.Parameters.AddWithValue("@type", selectedType);
                        insertCmd.ExecuteNonQuery();
                    }

                    // 小白讲解：注册成功后立刻跳转到登录页
                    // 为什么不用 Response.Redirect(url) 默认的重载？
                    // 默认重载会抛出一个 ThreadAbortException 来终止当前请求，
                    // 在 try..catch 里会被我们误当成“异常”，从而显示成“注册异常”。
                    // 用 Response.Redirect(url, false) + CompleteRequest() 可以避免这个异常。
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                    lblMessage.Text = "注册成功，正在前往登录页…";
                    Response.Redirect("~/Login.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "注册异常: " + ex.Message;
            }
        }
    }
}
