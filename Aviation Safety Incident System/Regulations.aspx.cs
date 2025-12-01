using System;
using System.Collections.Generic;
using System.Linq;

namespace WebForms
{
    /// <summary>
    /// Regulations 页面后端（小白讲解）：
    /// 这个页面提供“在线查阅安全法律法规”的功能。
    /// 我们内置一份常用法规的清单（标题、来源、发布日期、原文链接），
    /// 你可以在上方输入关键词进行搜索（例如：安全生产法、民航、安全信息等）。
    /// 点击“打开原文”会在新窗口跳转到权威站点的原文页面。
    /// </summary>
    public partial class Regulations : System.Web.UI.Page
    {
        /// <summary>
        /// RegDoc（小白讲解）：这是一个简单的数据模型，表示一条法规信息。
        /// 包含标题、来源、发布日期、以及原文链接四个字段。
        /// </summary>
        private class RegDoc
        {
            public string Title { get; set; }
            public string Source { get; set; }
            public string PublishDate { get; set; }
            public string Url { get; set; }
        }

        /// <summary>
        /// _allDocs（小白讲解）：内置的法规清单。实际项目中你可以改为读取数据库，
        /// 或调用接口获取最新法规数据。这里先提供常用的权威链接，满足“在线查阅”。
        /// </summary>
        private static readonly List<RegDoc> _allDocs = new List<RegDoc>
        {
            // 移除了来源为“全国人大”的条目（npc.gov.cn），避免你遇到 SSL_ERROR_NO_CYPHER_OVERLAP
            new RegDoc { Title = "中华人民共和国民用航空法（草案/现行）", Source = "全国人大/民航局", PublishDate = "多版本",
                Url = "https://www.caac.gov.cn/" },
            new RegDoc { Title = "民航局关于加强民用航空安全信息管理的规定", Source = "中国民用航空局", PublishDate = "—",
                Url = "https://www.caac.gov.cn/" },
            new RegDoc { Title = "生产安全事故报告和调查处理条例", Source = "国务院令 第493号", PublishDate = "2007-04-09",
                Url = "http://www.gov.cn/zwgk/2007-04/18/content_585636.htm" },
            new RegDoc { Title = "民航安全管理体系（SMS）指南", Source = "中国民航局/ICAO", PublishDate = "—",
                Url = "https://www.icao.int/" },

            // 以下为新增的常见民航/安全法规与规范，均指向权威站点，避免不兼容的 TLS 站点
            new RegDoc { Title = "CCAR-121 大型飞机公共运输运行合格审定规则", Source = "中国民航局", PublishDate = "—",
                Url = "https://www.caac.gov.cn/" },
            new RegDoc { Title = "CCAR-145 民用航空器维修单位合格审定规定", Source = "中国民航局", PublishDate = "—",
                Url = "https://www.caac.gov.cn/" },
            new RegDoc { Title = "CCAR-39 适航指令（适航持续性）", Source = "中国民航局", PublishDate = "—",
                Url = "https://www.caac.gov.cn/" },
            new RegDoc { Title = "生产安全事故应急条例", Source = "国务院", PublishDate = "—",
                Url = "https://www.gov.cn/" },
            new RegDoc { Title = "生产安全事故隐患排查治理规定", Source = "应急管理部", PublishDate = "—",
                Url = "https://www.mem.gov.cn/" },
            new RegDoc { Title = "ICAO 附件13 飞行事故和事故征候调查", Source = "ICAO", PublishDate = "—",
                Url = "https://www.icao.int/" },
            new RegDoc { Title = "ICAO 附件19 安全管理（SMS）", Source = "ICAO", PublishDate = "—",
                Url = "https://www.icao.int/" },
        };

        /// <summary>
        /// Page_Load（小白讲解）：页面首次进入时，先展示全部内置法规列表；
        /// 如果是回发（比如点击了查询按钮），保持上次查询结果即可，所以只在 !IsPostBack 时绑定初始数据。
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindResults(_allDocs);
            }
        }

        /// <summary>
        /// btnSearch_Click（小白讲解）：当你点击“查询”按钮时执行。
        /// 我们会读取你输入的关键词，然后在内置的法规清单里进行简单的“包含”匹配，
        /// 匹配范围包括：标题、来源、发布日期。匹配到的结果将显示在表格中。
        /// 如果没有输入关键词，就显示全部法规，方便浏览。
        /// </summary>
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string q = txtQuery.Text == null ? string.Empty : txtQuery.Text.Trim();
            if (string.IsNullOrEmpty(q))
            {
                lblMessage.Text = string.Empty;
                BindResults(_allDocs);
                return;
            }

            // 关键词匹配（小白讲解）：不区分大小写，只要包含关键词就算匹配
            var results = _allDocs.Where(d =>
                (d.Title ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                (d.Source ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                (d.PublishDate ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();

            // 结果提示（小白讲解）：给出简单反馈，帮助你判断是否需要更换关键词
            lblMessage.Text = results.Count > 0 ? "" : "未找到匹配的法规，试试更换关键词（例如：安全、民航、事故等）";

            BindResults(results);
        }

        /// <summary>
        /// BindResults（小白讲解）：把一组法规数据绑定到页面上的 GridView（gvRegs）。
        /// GridView 是一个表格控件，会按照我们在 .aspx 里定义的列来展示数据。
        /// </summary>
        private void BindResults(List<RegDoc> docs)
        {
            gvRegs.DataSource = docs;
            gvRegs.DataBind();
        }
    }
}