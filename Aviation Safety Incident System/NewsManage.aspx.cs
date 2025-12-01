using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls; // 访问 ContentPlaceHolder、Panel、GridView 等控件类型
using System.Web.UI; // 引用 Control 类型，解决编译错误 CS0246
// 不再上传图片

namespace WebForms
{
    public partial class NewsManage : System.Web.UI.Page
    {
        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        private string GetConnString()
        {
            return ConfigurationManager.ConnectionStrings["AviationDb"].ConnectionString;
        }

        /// <summary>
        /// 页面加载（小白解读）：
        /// 首次打开页面：根据你的用户角色决定是否显示“发布新闻”的区域（仅管理员可见），
        /// 然后从后端拉取“已发布”新闻列表显示出来（任何人都可以查看）。
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 角色控制（小白讲解）：只有管理员(1)可以发布新闻；普通用户(2)和审核人员(3)不显示发布区域。
                var typeObj = Session["User_type"];
                int userType = typeObj == null ? 0 : Convert.ToInt32(typeObj);
                // 为避免设计器文件缺失导致找不到字段，这里通过 Master 的 MainContent 精准查找控件（小白解读）：
                // WebForms 使用母版页时，页面控件都嵌在 ContentPlaceHolder(MainContent) 里，
                // 所以要先拿到它，再在里面找具体控件。
                var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
                if (cph == null)
                {
                    lblMessage.Text = "页面结构异常：找不到 MainContent";
                }
                else
                {
                    var publishPanel = cph.FindControl("panelPublish") as Panel;
                    if (publishPanel != null)
                    {
                        publishPanel.Visible = (userType == 1);
                    }
                    else
                    {
                        lblMessage.Text = "页面结构异常：找不到发布面板 panelPublish";
                    }
                    // 已移除“已发布新闻（最近）”组件，取消绑定调用
                }
                // 发布页仅负责发布，不再重复展示已发布新闻列表
            }
        }

        /// <summary>
        /// 绑定内嵌的“已发布新闻（最近）”列表（小白讲解）：
        /// 优先读取视图 v_PublishedNews；如果视图不存在则回退查询基础表。
        /// </summary>
        private void BindPublishedInline()
        {
            try
            {
                var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
                var grid = cph == null ? null : cph.FindControl("gvPublishedInline") as GridView;
                if (grid == null) return;

                using (var conn = new SqlConnection(GetConnString()))
                {
                    conn.Open();
                    var table = new DataTable();
                    try
                    {
                        using (var cmd = new SqlCommand("SELECT TOP 10 News_id, Title, Content, Publish_time, User_name FROM dbo.v_PublishedNews ORDER BY Publish_time DESC", conn))
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(table);
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        if (sqlEx.Message.IndexOf("Invalid object name", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            using (var cmd = new SqlCommand(@"SELECT TOP 10 n.News_id, n.Title, n.Content, n.Publish_time, u.User_name
                                                              FROM dbo.news_info AS n LEFT JOIN dbo.users_info AS u ON n.User_id = u.User_id
                                                              WHERE n.Status = N'已发布'
                                                              ORDER BY n.Publish_time DESC", conn))
                            using (var da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(table);
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }

                    grid.DataSource = table;
                    grid.DataBind();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "加载已发布新闻失败：" + ex.Message;
            }
        }

        /// <summary>
        /// 创建新闻（小白解读）：
        /// 读取你输入的标题和内容，直接插入到 news_info 表，默认状态为“审核中”；
        /// 如果勾选了“自动发布”，立即更新该新闻为“已发布”并记录发布时间。
        /// </summary>
        protected void btnCreate_Click(object sender, EventArgs e)
        {
            lblMessage.Text = string.Empty;
            int userId = Session["User_id"] == null ? 0 : Convert.ToInt32(Session["User_id"]);
            int userType = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);
            if (userId <= 0)
            {
                lblMessage.Text = "请先登录";
                return;
            }
            // 权限校验（小白讲解）：只有管理员才能发布新闻；普通用户和审核人员不能发布。
            if (userType != 1)
            {
                lblMessage.Text = "无权限发布：仅管理员可发布新闻";
                return;
            }

            try
            {
                // 标题长度校验（小白讲解）：数据库 Title 列是 NVARCHAR(200)，所以这里允许最多 200 字符。
                var title = (txtTitle.Text ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(title))
                {
                    lblMessage.Text = "请填写标题";
                    return;
                }
                if (title.Length > 200)
                {
                    lblMessage.Text = "标题长度不能超过 200 个字符";
                    return;
                }
                // 类别需求取消（小白讲解）：直接使用原始标题，不再添加类别前缀。
                var finalTitle = title;
                int newId = 0;
                using (var conn = new SqlConnection(GetConnString()))
                {
                    conn.Open();
                    // 不再支持图片上传（小白讲解）：发布的内容只保存纯文本
                    string finalContent = txtContent.Text == null ? string.Empty : txtContent.Text;
                    // 1) 创建草稿
                    using (var cmd = new SqlCommand(@"INSERT INTO news_info(Title, Content, User_id, Status)
                                                     VALUES(@title, @content, @uid, N'审核中');
                                                     SELECT SCOPE_IDENTITY();", conn))
                    {
                        cmd.Parameters.AddWithValue("@title", finalTitle);
                        var contentParam = string.IsNullOrWhiteSpace(finalContent) ? (object)DBNull.Value : (object)finalContent;
                        cmd.Parameters.AddWithValue("@content", contentParam);
                        cmd.Parameters.AddWithValue("@uid", userId);
                        var objId = cmd.ExecuteScalar();
                        newId = Convert.ToInt32(objId);
                    }

                    // 2) 直接发布（按你的需求：管理员提交后即发布）
                    using (var pubCmd = new SqlCommand("UPDATE news_info SET Status=N'已发布', Publish_time = ISNULL(Publish_time, GETDATE()) WHERE News_id=@id", conn))
                    {
                        pubCmd.Parameters.AddWithValue("@id", newId);
                        pubCmd.ExecuteNonQuery();
                    }
                }
                // 发布成功后，用“诊断提示”告诉你已写入数据库的编号与总数（小白讲解）
                lblMessage.ForeColor = System.Drawing.Color.Green;
                try
                {
                    int total = 0;
                    using (var conn2 = new SqlConnection(GetConnString()))
                    using (var cmd2 = new SqlCommand("SELECT COUNT(1) FROM dbo.news_info WHERE Status = N'已发布'", conn2))
                    {
                        conn2.Open();
                        var obj = cmd2.ExecuteScalar();
                        total = obj == null ? 0 : Convert.ToInt32(obj);
                    }
                    lblMessage.Text = string.Format("新闻已发布（编号：{0}）。当前已发布 {1} 条。", newId, total);
                }
                catch
                {
                    lblMessage.Text = "新闻已发布";
                }
                // 刷新内嵌列表并清空表单，留在当前面板
                BindPublishedInline();
                ResetPublishForm();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "提交异常: " + ex.Message;
            }
        }

        /// <summary>
        /// 内嵌列表分页（小白讲解）：点击页码时切换页并重新绑定。
        /// </summary>
        protected void gvPublishedInline_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
                var grid = cph == null ? null : cph.FindControl("gvPublishedInline") as GridView;
                if (grid == null) return;
                grid.PageIndex = e.NewPageIndex;
                BindPublishedInline();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "分页异常：" + ex.Message;
            }
        }

        /// <summary>
        /// 清空发布表单（小白讲解）：
        /// 当你点击“提交”成功后，我们把标题、内容重置为空，
        /// 图片选择框在页面回发后浏览器会自动清空（出于安全原因），
        /// 复选框恢复到默认勾选状态，方便你继续发布下一条。
        /// </summary>
        private void ResetPublishForm()
        {
            try
            {
                if (txtTitle != null) txtTitle.Text = string.Empty;
                if (txtContent != null) txtContent.Text = string.Empty;
                // FileUpload 无法通过服务器端代码直接保留或清空文件选择，
                // 页面回发后浏览器会自动清空，因此这里无需额外处理。
                if (chkAutoPublish != null) chkAutoPublish.Checked = true;
            }
            catch
            {
                // 清空失败不影响发布流程
            }
        }
    }
}
