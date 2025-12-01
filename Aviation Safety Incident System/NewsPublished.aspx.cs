using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    /// <summary>
    /// 小白讲解：这个页面用来展示“已发布新闻”。
    /// - 打开页面时，我们到数据库视图 v_PublishedNews 读取列表并显示。
    /// - 点击表格里的“下架”按钮，会把该新闻的状态改为“已下架”，并清空发布时间，相当于撤下首页展示。
    /// </summary>
    public partial class NewsPublished : Page
    {
        /// <summary>
        /// 小白讲解：从 Web.config 里取数据库连接字符串。你可以理解为“告诉程序到哪里连数据库”。
        /// </summary>
        private string GetConnString()
        {
            return ConfigurationManager.ConnectionStrings["AviationDb"].ConnectionString;
        }

        /// <summary>
        /// 小白讲解：页面首次加载时绑定新闻列表；回发（比如点击按钮）则不重复加载，避免覆盖提示信息。
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var from = Request.QueryString["from"];
                if (string.Equals(from, "publish_success", StringComparison.OrdinalIgnoreCase))
                {
                    lblMessage.Text = "发布成功：已为你加载最新已发布列表。";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                BindPublishedNews();
            }
        }

        /// <summary>
        /// 小白讲解：读取 v_PublishedNews 视图的数据，绑定到 GridView 展示。
        /// 这个视图只包含状态为“已发布”的新闻，并附带作者名称，方便直接显示。
        /// </summary>
        private void BindPublishedNews()
        {
            try
            {
                using (var conn = new SqlConnection(GetConnString()))
                {
                    conn.Open();
                    DataTable table = new DataTable();
                    bool loadedFromView = false;

                    // 优先尝试视图
                    try
                    {
                        using (var cmd = new SqlCommand("SELECT News_id, Title, Content, Publish_time, User_name FROM dbo.v_PublishedNews ORDER BY Publish_time DESC", conn))
                        using (var adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(table);
                            loadedFromView = true;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        if (sqlEx.Message.IndexOf("Invalid object name", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            loadedFromView = false;
                        }
                        else
                        {
                            throw;
                        }
                    }

                    // 如果视图不存在或返回空结果，回退到基础表查询
                    if (!loadedFromView || table.Rows.Count == 0)
                    {
                        table.Clear();
                        using (var cmd = new SqlCommand(@"SELECT n.News_id, n.Title, n.Content, n.Publish_time, u.User_name
                                                          FROM dbo.news_info AS n LEFT JOIN dbo.users_info AS u ON n.User_id = u.User_id
                                                          WHERE n.Status = N'已发布'
                                                          ORDER BY ISNULL(n.Publish_time, GETDATE()) DESC", conn))
                        using (var adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(table);
                        }
                    }

                    gvNews.DataSource = table;
                    gvNews.DataBind();
                    lblMessage.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "加载已发布新闻异常: " + ex.Message;
            }
        }

        /// <summary>
        /// 小白讲解：“下架”功能。点击对应行的按钮后触发。
        /// 处理方式：把该新闻的 Status 改为“已下架”，同时把 Publish_time 置空（清除发布时间），这样它就不再出现在已发布列表。
        /// </summary>
        protected void gvNews_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.RowIndex >= gvNews.Rows.Count)
                {
                    lblMessage.Text = "操作失败：无效的行索引";
                    return;
                }

                // 通过 GridView 的 DataKey 获取 News_id（我们在 .aspx 已设置 DataKeyNames="News_id"）
                int newsId = Convert.ToInt32(gvNews.DataKeys[e.RowIndex].Value);

                using (var conn = new SqlConnection(GetConnString()))
                using (var cmd = new SqlCommand("UPDATE dbo.news_info SET Status = N'已下架', Publish_time = NULL WHERE News_id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", newsId);
                    conn.Open();
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                    {
                        lblMessage.Text = "已下架该新闻";
                        BindPublishedNews();
                    }
                    else
                    {
                        lblMessage.Text = "下架失败：未找到该新闻或状态已变更";
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "下架异常: " + ex.Message;
            }
        }

        /// <summary>
        /// 小白讲解：分页事件。当你点击分页控件的页码，我们把 GridView 的 PageIndex 改为你选择的页码，然后重新绑定数据。
        /// </summary>
        protected void gvNews_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                gvNews.PageIndex = e.NewPageIndex;
                BindPublishedNews();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "分页异常: " + ex.Message;
            }
        }

        /// <summary>
        /// 小白讲解：行数据绑定事件。这里可以对每一行做一些展示优化，比如清理过长内容、格式化时间等。
        /// 目前先不做额外处理，只保证不抛异常。
        /// </summary>
        protected void gvNews_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            try
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    // 仅管理员显示“下架”按钮（小白讲解）：避免普通用户或游客误操作
                    int userType = Session["User_type"] == null ? 0 : Convert.ToInt32(Session["User_type"]);
                    var btnDelete = e.Row.Cells[e.Row.Cells.Count - 1].Controls.Count > 0 ? e.Row.Cells[e.Row.Cells.Count - 1].Controls[0] as LinkButton : null;
                    if (btnDelete != null && btnDelete.CommandName == "Delete")
                    {
                        btnDelete.Visible = (userType == 1);
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "行绑定异常: " + ex.Message;
            }
        }
    }
}
