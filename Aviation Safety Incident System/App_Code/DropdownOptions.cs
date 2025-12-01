using System.Web.UI.WebControls;

namespace WebFormsOptions
{
    /// <summary>
    /// 下拉选项统一助手（小白讲解）：
    /// 把“事件类型”和“地点”的可选项集中在这里管理，
    /// 这样项目里所有页面都能用同一套选项，避免出现不一致。
    /// 使用方法：在页面的 Page_Load 里调用 BindIncidentTypes/BindLocations。
    /// </summary>
    public static class DropdownOptions
    {
        /// <summary>
        /// 统一的“事件类型”选项（小白讲解）：
        /// 这里固定为三个类型：事故、事故征候、一般事件。
        /// 页面上的下拉列表会直接使用这三项，不允许自定义。
        /// </summary>
        public static readonly string[] IncidentTypes = new string[]
        {
            "事故",
            "事故征候",
            "一般事件"
        };

        /// <summary>
        /// 统一的“地点”选项（小白讲解）：
        /// 如果某个页面需要“自定义...”，我们在绑定时额外加上即可。
        /// </summary>
        public static readonly string[] Locations = new string[]
        {
            "跑道",
            "滑行道",
            "登机口",
            "候机楼",
            "机库",
            "航站楼"
        };

        /// <summary>
        /// 绑定“事件类型”到下拉框（小白讲解）：
        /// - includeAll=true 会在最前面加一个“全部”选项（值为空字符串），用于查询筛选。
        /// </summary>
        public static void BindIncidentTypes(DropDownList ddl, bool includeAll)
        {
            if (ddl == null) return;
            ddl.Items.Clear();
            if (includeAll)
            {
                ddl.Items.Add(new ListItem("全部", ""));
            }
            for (int i = 0; i < IncidentTypes.Length; i++)
            {
                string t = IncidentTypes[i];
                ddl.Items.Add(new ListItem(t, t));
            }
        }

        /// <summary>
        /// 绑定“地点”到下拉框（小白讲解）：
        /// 只绑定固定地点选项，不再提供“自定义...”选项。
        /// includeCustom 参数将被忽略，是为了兼容旧代码的调用。
        /// </summary>
        public static void BindLocations(DropDownList ddl, bool includeCustom)
        {
            if (ddl == null) return;
            ddl.Items.Clear();
            for (int i = 0; i < Locations.Length; i++)
            {
                string l = Locations[i];
                ddl.Items.Add(new ListItem(l, l));
            }
        }
    }
}
