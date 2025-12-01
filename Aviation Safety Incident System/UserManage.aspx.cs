using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography; // 计算 MD5 用
using System.Text; // 字符串转字节用

namespace WebForms
{
    public partial class UserManage : System.Web.UI.Page
    {
        /// <summary>
        /// 获取数据库连接字符串（小白解读）：
        /// 就是告诉程序去哪个数据库工作，我们从 Web.config 的 connectionStrings 里
        /// 读取名为 "AviationDb" 的配置。
        /// </summary>
        private string GetConnString()
        {
            return ConfigurationManager.ConnectionStrings["AviationDb"].ConnectionString;
        }

        /// <summary>
        /// 页面加载（小白解读）：
        /// 仅管理员(1)可以访问本页面；首次加载时绑定用户列表到 GridView。
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int userType = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);
                if (userType != 1)
                {
                    WriteMessage("无权限访问：仅管理员可以分配角色", true);
                    return;
                }
                BindUsers();
            }
        }

        /// <summary>
        /// 绑定用户列表（小白解读）：
        /// 从 users_info 表把用户的 编号/用户名/当前角色 读出来，展示到表格。
        /// </summary>
        private void BindUsers()
        {
            try
            {
                using (var conn = new SqlConnection(GetConnString()))
                using (var da = new SqlDataAdapter("SELECT User_id, User_name, type FROM users_info ORDER BY User_id ASC", conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    gvUsers.DataSource = dt;
                    gvUsers.DataBind();
                }
            }
            catch (Exception ex)
            {
                WriteMessage("加载用户失败：" + ex.Message, true);
            }
        }

        /// <summary>
        /// 行数据绑定（小白解读）：
        /// 每一行绑定时，把该用户的当前角色值设置到下拉框里显示出来。
        /// </summary>
        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var ddl = e.Row.FindControl("ddlRoleRow") as DropDownList;
                if (ddl != null)
                {
                    var drv = e.Row.DataItem as DataRowView;
                    if (drv != null)
                    {
                        int type = Convert.ToInt32(drv["type"]);
                        var item = ddl.Items.FindByValue(type.ToString());
                        if (item != null)
                        {
                            ddl.ClearSelection();
                            item.Selected = true;
                        }

                        // 隐藏管理员的“重置密码”按钮（小白解读）：
                        // 如果该行用户的角色是管理员(type=1)，就把按钮隐藏，避免在 UI 层面触发不允许的操作。
                        var resetBtn = e.Row.FindControl("btnResetPwd") as Button;
                        if (resetBtn != null && type == 1)
                        {
                            resetBtn.Visible = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 表格命令（小白解读）：
        /// 当你点击“保存角色”按钮时，我们读取该行的角色下拉框值，
        /// 更新到数据库 users_info.type 字段里，然后提示成功并刷新列表。
        /// </summary>
        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "SaveRole")
            {
                int userId = 0;
                int.TryParse(e.CommandArgument.ToString(), out userId);
                if (userId <= 0) { WriteMessage("参数错误：User_id", true); return; }

                // 找到当前按钮所在行，从中取出下拉框的值
                var btn = e.CommandSource as Control;
                var row = (btn == null) ? null : btn.NamingContainer as GridViewRow;
                var ddl = (row == null) ? null : row.FindControl("ddlRoleRow") as DropDownList;
                int newType = 0;
                if (ddl == null || !int.TryParse(ddl.SelectedValue, out newType) || newType == 0)
                {
                    WriteMessage("请选择有效的角色", true);
                    return;
                }

                try
                {
                    using (var conn = new SqlConnection(GetConnString()))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand("UPDATE users_info SET type=@t WHERE User_id=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@t", newType);
                            cmd.Parameters.AddWithValue("@id", userId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    WriteMessage("角色已更新", false);
                    BindUsers();
                }
                catch (Exception ex)
                {
                    WriteMessage("更新失败：" + ex.Message, true);
                }
            }
            else if (e.CommandName == "ResetPwd")
            {
                // 重置密码（小白解读）：生成一个新的临时密码，更新到数据库，并把明文临时密码提示给管理员。
                int userId = 0;
                int.TryParse(e.CommandArgument.ToString(), out userId);
                if (userId <= 0) { WriteMessage("参数错误：User_id", true); return; }
                try
                {
                    // 在这里声明临时密码变量（小白解读）：
                    // 需要在整个 try 范围内使用它做提示，所以提前声明并在下方赋值。
                    string tempPwd = null;
                    using (var conn = new SqlConnection(GetConnString()))
                    {
                        conn.Open();

                        // 安全校验（小白解读）：如果目标用户是管理员(type=1)，禁止通过此页面重置密码。
                        int targetType = 0;
                        using (var typeCmd = new SqlCommand("SELECT type FROM users_info WHERE User_id=@id", conn))
                        {
                            typeCmd.Parameters.AddWithValue("@id", userId);
                            object typeObj = typeCmd.ExecuteScalar();
                            if (typeObj == null || typeObj == DBNull.Value)
                            {
                                WriteMessage("要重置的用户不存在", true);
                                return;
                            }
                            targetType = Convert.ToInt32(typeObj);
                        }
                        if (targetType == 1)
                        {
                            WriteMessage("为安全起见，禁止通过此功能重置管理员密码。请由管理员本人在“修改密码”流程中自行处理。", true);
                            return;
                        }

                        tempPwd = GenerateTempPassword(10); // 生成 10 位较安全的临时密码
                        using (var cmd = new SqlCommand("UPDATE users_info SET password=@pwd WHERE User_id=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@pwd", ComputeMD5Hex(tempPwd));
                            cmd.Parameters.AddWithValue("@id", userId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    // 在行内显示临时密码，并在顶部再提示一遍（小白友好）
                    var btn = e.CommandSource as Control;
                    var row = (btn == null) ? null : btn.NamingContainer as GridViewRow;
                    // 只有非管理员用户才会走到这里，因此可以正常回显临时密码
                    ShowTempPasswordInRow(row, tempPwd);
                    WriteMessage("已重置密码。临时密码为：" + tempPwd + "。请尽快通知用户登录后自行修改。", false);
                }
                catch (Exception ex)
                {
                    WriteMessage("重置失败：" + ex.Message, true);
                }
            }
            else if (e.CommandName == "SetPwd")
            {
                /// <summary>
                /// 重置为指定密码（小白讲解）：
                /// 在表格该行输入新密码，点击按钮后把该用户密码改为你指定的值（会做 MD5 存库）。
                /// 安全策略：不允许通过本页面重置管理员(type=1)的密码，防止误操作；请管理员在个人修改密码流程中自助处理。
                /// </summary>
                int userId = 0;
                int.TryParse(e.CommandArgument.ToString(), out userId);
                if (userId <= 0) { WriteMessage("参数错误：User_id", true); return; }

                // 取该行的密码输入框
                var btn = e.CommandSource as Control;
                var row = (btn == null) ? null : btn.NamingContainer as GridViewRow;
                var txt = (row == null) ? null : row.FindControl("txtNewPwdRow") as TextBox;
                string newPwd = txt == null ? null : (txt.Text ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(newPwd)) { WriteMessage("请输入新密码", true); return; }
                if (newPwd.Length < 6 || newPwd.Length > 50) { WriteMessage("密码长度需在 6-50 之间", true); return; }

                try
                {
                    using (var conn = new SqlConnection(GetConnString()))
                    {
                        conn.Open();
                        // 查询被重置用户的角色
                        int targetType = 0;
                        using (var typeCmd = new SqlCommand("SELECT type FROM users_info WHERE User_id=@id", conn))
                        {
                            typeCmd.Parameters.AddWithValue("@id", userId);
                            object typeObj = typeCmd.ExecuteScalar();
                            if (typeObj == null || typeObj == DBNull.Value)
                            {
                                WriteMessage("要重置的用户不存在", true);
                                return;
                            }
                            targetType = Convert.ToInt32(typeObj);
                        }
                        if (targetType == 1)
                        {
                            WriteMessage("为安全起见，禁止通过此功能重置管理员密码，请由管理员本人在“修改密码”流程中处理。", true);
                            return;
                        }

                        using (var cmd = new SqlCommand("UPDATE users_info SET password=@pwd WHERE User_id=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@pwd", ComputeMD5Hex(newPwd));
                            cmd.Parameters.AddWithValue("@id", userId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    WriteMessage("密码已重置为指定值", false);
                }
                catch (Exception ex)
                {
                    WriteMessage("重置失败：" + ex.Message, true);
                }
            }
            else if (e.CommandName == "DeleteUser")
            {
                // 删除用户（小白解读）：安全删除指定的用户，包含以下保护逻辑：
                // 1) 禁止删除当前登录账号，避免把自己删掉导致无法继续管理；
                // 2) 禁止删除系统中最后一个管理员，系统必须至少保留一名管理员；
                // 3) 正常删除时执行数据库 DELETE 并刷新列表。
                int userId = 0;
                int.TryParse(e.CommandArgument.ToString(), out userId);
                if (userId <= 0) { WriteMessage("参数错误：User_id", true); return; }

                TryDeleteUser(userId);
            }
            else if (e.CommandName == "TransferNewsToMe")
            {
                /// <summary>
                /// 转移新闻作者到当前管理员（小白讲解）：
                /// 当该用户因为被新闻引用而无法删除时，点击此按钮可把他所有新闻的作者改为“我”（当前登录管理员），
                /// 这样就不会再被外键引用阻塞删除。
                /// 安全限制：仅管理员可执行；若未登录或非管理员会提示无权。
                /// </summary>
                int sourceUserId = 0;
                int.TryParse(e.CommandArgument.ToString(), out sourceUserId);
                int currentUserId = Session["User_id"] == null ? 0 : Convert.ToInt32(Session["User_id"]);
                int currentUserType = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);
                if (currentUserId <= 0 || currentUserType != 1)
                {
                    WriteMessage("无权操作：仅管理员可转移新闻作者", true);
                    return;
                }
                if (sourceUserId <= 0)
                {
                    WriteMessage("参数错误：User_id", true);
                    return;
                }
                try
                {
                    using (var conn = new SqlConnection(GetConnString()))
                    using (var cmd = new SqlCommand("UPDATE dbo.news_info SET User_id=@to WHERE User_id=@from", conn))
                    {
                        cmd.Parameters.AddWithValue("@to", currentUserId);
                        cmd.Parameters.AddWithValue("@from", sourceUserId);
                        conn.Open();
                        int affected = cmd.ExecuteNonQuery();
                        WriteMessage("已转移新闻数量：" + affected + "（作者改为当前管理员）", false);
                    }
                    BindUsers();
                }
                catch (Exception ex)
                {
                    WriteMessage("转移失败：" + ex.Message, true);
                }
            }
            else if (e.CommandName == "DeleteNewsByUser")
            {
                /// <summary>
                /// 删除该用户的所有新闻（小白讲解）：
                /// 当你确认不需要保留这个用户的新闻时，使用此功能批量删除其所有新闻记录。
                /// 注意：此操作不可恢复，且仅管理员可执行。
                /// </summary>
                int targetUserId = 0;
                int.TryParse(e.CommandArgument.ToString(), out targetUserId);
                int currentUserId = Session["User_id"] == null ? 0 : Convert.ToInt32(Session["User_id"]);
                int currentUserType = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);
                if (currentUserId <= 0 || currentUserType != 1)
                {
                    WriteMessage("无权操作：仅管理员可删除新闻", true);
                    return;
                }
                if (targetUserId <= 0)
                {
                    WriteMessage("参数错误：User_id", true);
                    return;
                }
                try
                {
                    using (var conn = new SqlConnection(GetConnString()))
                    using (var cmd = new SqlCommand("DELETE FROM dbo.news_info WHERE User_id=@uid", conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", targetUserId);
                        conn.Open();
                        int affected = cmd.ExecuteNonQuery();
                        WriteMessage("已删除新闻数量：" + affected, false);
                    }
                    BindUsers();
                }
                catch (Exception ex)
                {
                    WriteMessage("删除新闻失败：" + ex.Message, true);
                }
            }
        }

        /// <summary>
        /// 统一提示输出（小白解读）：
        /// 在页面顶部的 lblMessage 显示绿色成功或红色错误信息。
        /// </summary>
        private void WriteMessage(string message, bool isError)
        {
            if (lblMessage != null)
            {
                lblMessage.Text = message;
                lblMessage.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;
            }
            else
            {
                Response.Write(isError ? ("<div style='color:red'>" + message + "</div>") : ("<div style='color:green'>" + message + "</div>"));
            }
        }

        /// <summary>
        /// 计算输入密码的 MD5 32位十六进制字符串（小白解读）：
        /// 管理员创建用户时，我们把初始密码做 MD5 存到数据库，避免明文密码。
        /// 你只要传入明文字符串，这个函数会返回 32 位的小写十六进制字符串。
        /// </summary>
        private string ComputeMD5Hex(string input)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input ?? string.Empty);
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
        /// 生成临时密码（小白解读）：
        /// 使用系统的随机数生成器，按给定长度生成由大小写字母和数字组成的字符串。
        /// 这个明文只用于提示管理员，写入数据库时依然会做 MD5 哈希。
        /// </summary>
        private string GenerateTempPassword(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789"; // 去掉易混淆字符
            var data = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
            }
            var sb = new StringBuilder(length);
            foreach (var b in data)
            {
                sb.Append(chars[b % chars.Length]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 在表格行内显示临时密码（小白解读）：
        /// 找到该行的 lblTempPwd 控件，把生成的临时密码放进去显示给管理员。
        /// 注意：不刷新列表，避免刚写入的文本被覆盖；这只是一次性显示，不会存数据库明文。
        /// </summary>
        private void ShowTempPasswordInRow(GridViewRow row, string tempPwd)
        {
            try
            {
                if (row == null) return;
                var lbl = row.FindControl("lblTempPwd") as Label;
                if (lbl != null)
                {
                    lbl.Text = tempPwd;
                }
            }
            catch
            {
                // 静默处理行内显示失败的情况，顶部的成功提示仍然保留
            }
        }

        /// <summary>
        /// 尝试删除用户（小白解读）：
        /// 根据传入的 userId 做安全检查并删除用户。
        /// - 如果要删除的是当前登录用户（Session 中的 User_name），则拒绝，避免“自我删除”。
        /// - 如果系统只剩最后一个管理员（type=1），则拒绝，系统必须至少保留一个管理员。
        /// - 其他情况执行 DELETE，并刷新列表。
        /// </summary>
        private void TryDeleteUser(int userId)
        {
            try
            {
                using (var conn = new SqlConnection(GetConnString()))
                {
                    conn.Open();

                    // 读取准备删除的用户基本信息
                    string targetName = null;
                    int targetType = 0;
                    using (var infoCmd = new SqlCommand("SELECT User_name, type FROM users_info WHERE User_id=@id", conn))
                    {
                        infoCmd.Parameters.AddWithValue("@id", userId);
                        using (var reader = infoCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                targetName = reader["User_name"] as string;
                                targetType = Convert.ToInt32(reader["type"]);
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(targetName))
                    {
                        WriteMessage("要删除的用户不存在", true);
                        return;
                    }

                    // 保护 1：禁止删除当前登录用户
                    var currentNameObj = Session["User_name"];
                    string currentName = currentNameObj == null ? null : currentNameObj.ToString();
                    if (!string.IsNullOrEmpty(currentName) && string.Equals(currentName, targetName, StringComparison.OrdinalIgnoreCase))
                    {
                        WriteMessage("不允许删除当前登录账号，请使用其他管理员账号操作", true);
                        return;
                    }

                    // 保护 2：禁止删除系统中的最后一个管理员
                    if (targetType == 1)
                    {
                        int adminCount = 0;
                        using (var countCmd = new SqlCommand("SELECT COUNT(*) FROM users_info WHERE type=1", conn))
                        {
                            adminCount = Convert.ToInt32(countCmd.ExecuteScalar());
                        }
                        if (adminCount <= 1)
                        {
                            WriteMessage("系统必须保留至少一个管理员，请先创建新的管理员或更改该用户角色后再删除", true);
                            return;
                        }
                    }

                    // 保护 3：外键引用检查（小白讲解）：
                    // 如果该用户在事件表或新闻表中被引用，直接删除会触发数据库外键错误（如 FK_incident_user）。
                    // 我们先统计被引用次数，提示你先处理业务数据或改为“停用/更改角色”。
                    int incidentRef = 0;
                    using (var cntIncident = new SqlCommand("SELECT COUNT(*) FROM incident_info WHERE User_id=@id", conn))
                    {
                        cntIncident.Parameters.AddWithValue("@id", userId);
                        incidentRef = Convert.ToInt32(cntIncident.ExecuteScalar());
                    }
                    int newsRef = 0;
                    using (var cntNews = new SqlCommand("SELECT COUNT(*) FROM news_info WHERE User_id=@id", conn))
                    {
                        cntNews.Parameters.AddWithValue("@id", userId);
                        newsRef = Convert.ToInt32(cntNews.ExecuteScalar());
                    }
                    if (incidentRef > 0 || newsRef > 0)
                    {
                        WriteMessage(string.Format(
                            "删除失败：该用户在 {0} 条事件、{1} 条新闻中被引用。请先转移或删除这些数据，或改为停用/更改角色。",
                            incidentRef, newsRef), true);
                        return;
                    }

                    // 执行删除
                    using (var delCmd = new SqlCommand("DELETE FROM users_info WHERE User_id=@id", conn))
                    {
                        delCmd.Parameters.AddWithValue("@id", userId);
                        delCmd.ExecuteNonQuery();
                    }
                }

                WriteMessage("用户已删除", false);
                BindUsers();
            }
            catch (Exception ex)
            {
                // 常见错误（小白提示）：
                // - 如果数据库有外键约束（例如其他表引用了该用户），删除可能失败；
                //   这时需要先处理业务数据或改为“停用”逻辑，这里先把异常提示出来。
                WriteMessage("删除失败：" + ex.Message, true);
            }
        }

        /// <summary>
        /// 创建新用户（小白解读）：
        /// 管理员在顶部表单填入“用户名/初始密码/邮箱/角色”，点击“创建用户”即可。
        /// 我们会检查用户名是否已存在；不存在则插入到 users_info 表，并刷新下面的列表。
        /// </summary>
        // 已移除“新增用户”功能，页面仅保留删除用户与配置权限。
    }
}
