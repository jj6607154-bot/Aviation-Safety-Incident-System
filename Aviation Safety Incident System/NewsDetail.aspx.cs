using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI; // 使用 Page 和控件类型
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class NewsDetail : System.Web.UI.Page
    {
        /// <summary>
        /// 获取数据库连接字符串（小白解读）：
        /// 就是从 Web.config 里拿到连接到数据库的地址。
        /// </summary>
        private string GetConnString()
        {
            return ConfigurationManager.ConnectionStrings["AviationDb"].ConnectionString;
        }

        /// <summary>
        /// 规范化状态显示（小白讲解）：
        /// 数据库里的“状态”可能是数字、中文，甚至被错误写成网址。
        /// 我们在这里做统一处理：
        /// - 为空时显示“未设置”
        /// - 0/1/2 映射为 草稿/已发布/下架（示例映射，够用）
        /// - 如果看起来像网址（http/https 或 .aspx），认为是错误数据，显示“未知”
        /// - 其他情况原样显示
        /// </summary>
        private string NormalizeStatus(object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value) return "未设置";
            var s = Convert.ToString(dbValue).Trim();
            if (string.IsNullOrEmpty(s)) return "未设置";

            var lower = s.ToLowerInvariant();
            if (lower.StartsWith("http://") || lower.StartsWith("https://")) return "未知";
            if (s.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase)) return "未知";

            // 简单数值映射，满足常见场景
            if (s == "0") return "草稿";
            if (s == "1") return "已发布";
            if (s == "2") return "下架";

            return s;
        }

        /// <summary>
        /// 页面加载（小白解读）：
        /// 打开页面时，我们从地址栏中读取 NewsId，然后去数据库把这条新闻的详细内容查出来显示。
        /// 如果没有传 NewsId，或者对应的新闻不存在，就显示提示信息。
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int newsId;
                if (!int.TryParse(Request.QueryString["NewsId"], out newsId) || newsId <= 0)
                {
                    lblMessage.Text = "参数错误：未提供有效的新闻编号";
                    return;
                }
                LoadNews(newsId);
            }
        }

        /// <summary>
        /// 加载新闻详情（小白解读）：
        /// 通过 NewsId 去数据库查询标题、内容、作者、发布时间、状态，将它们显示到页面上。
        /// </summary>
        private void LoadNews(int newsId)
        {
            try
            {
                using (var conn = new SqlConnection(GetConnString()))
                using (var cmd = new SqlCommand(@"SELECT n.Title, n.Content, n.Publish_time, n.Status, u.User_name
                                                  FROM news_info n LEFT JOIN users_info u ON n.User_id = u.User_id
                                                  WHERE n.News_id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", newsId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            lblMessage.Text = "未找到该新闻，可能已被删除或尚未创建";
                            return;
                        }

                        string title = reader["Title"] == DBNull.Value ? string.Empty : reader["Title"].ToString();
                        string content = reader["Content"] == DBNull.Value ? string.Empty : reader["Content"].ToString();
                        string author = reader["User_name"] == DBNull.Value ? "未知作者" : reader["User_name"].ToString();
                        string publishTime = reader["Publish_time"] == DBNull.Value ? "未发布" : Convert.ToDateTime(reader["Publish_time"]).ToString("yyyy-MM-dd HH:mm");

                        lblTitle.Text = title;
                        lblMeta.Text = string.Format("作者：{0}，发布时间：{1}", author, publishTime);
                        litContent.Text = string.IsNullOrWhiteSpace(content) ? "<p>无内容</p>" : content;
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "加载新闻异常：" + ex.Message;
            }
        }
    }
}
