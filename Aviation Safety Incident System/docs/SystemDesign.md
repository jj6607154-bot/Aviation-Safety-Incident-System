安全科学与工程学院 2025-2026 学年第一学期
《互联网应用服务开发与数据库实践》课程设计文档

题目：民航不安全事件管理系统
班级：信安三班
学号：231240080
姓名：蒋世玲 / 汪宇伶
电子签：____________________
教师：邢艳
成绩：____________________
日期：2025-10-12

一 系统需求分析
- 系统开发背景：民航不安全事件（设备故障、操作违规、异常延误等）传统人工记录导致信息同步滞后、检索效率低。系统目标是实现事件从上报、审核、查询到归档的数字化闭环，提升安全事件处置效率并提供法规/资讯聚合能力。
- 系统目标：
  - 实现不安全事件的规范化录入与精准查询。
  - 搭建安全信息发布平台，集中展示已公开事件和行业资讯。
  - 建立严格的角色与权限控制（管理员/普通用户/审核人员）。
- 功能需求概述：
  - 用户登录与权限管理：账号密码登录，按角色开放功能。管理员可管理用户和发布信息；审核人员审批事件；普通用户上报和查询公开事件。
  - 安全信息发布：管理员录入并发布新闻，支持长文本和外链图片；已发布内容对所有用户可见。
  - 不安全事件管理：上报事件（类型、时间、地点、描述、上报人），审核流转（待审核→已归档/驳回），按编号、时间范围、类型检索。
- 非功能需求：
  - 性能：常规操作响应≤6秒；分页加载 GridView，减少一次性渲染量。
  - 安全性：
    - 密码使用 MD5(32位) 哈希存储（字段 Char(32)）。
    - 参数化查询、防 SQL 注入；基于 Session 的访问控制。
  - 可用性：简洁 UI；运行稳定性≥99%。
  - 兼容性：支持 Chrome/Firefox/Safari/Edge；桌面与移动适配基础样式。

二 系统总体设计
- 架构：ASP.NET WebForms + SQL Server。采用三层分工的轻量实现（页面/代码后置 + ADO.NET 数据访问）。
- 核心模块：
  - 用户管理：登录、注册、用户列表、角色分配、密码重置。
  - 信息发布：新闻录入/发布/查询，"已发布"视图直接供页面展示。
  - 不安全事件管理：事件上报、查询、审核、归档；分页与连续序号显示。
- 模块关系：
  - 用户管理为其他模块提供权限校验（Session: `User_id`,`User_type`）。
  - 信息发布对所有用户开放已发布内容；发布行为仅限管理员。
  - 事件管理依赖权限：管理员/审核可查看全部并审核，普通用户仅查看自己与公开事件。

三 数据库设计
1) 概念结构（实体/属性）
- 用户（users_info）：`User_id, User_name, password(MD5), type(1/2/3), telephone, address, email`
- 安全新闻（news_info）：`News_id, Title, Content, Publish_time, User_id(FK), Status`
- 不安全事件（incident_info）：`Incident_id, Incident_type, Occur_time, Location, Description, User_id(FK), Report_time, Incident_status`
- 法律法规（law_info）：`Law_id, Law_name, Category, Issue_unit, Effective_time, Content`
- 审核日志（incident_audit_log）：`Log_id, Incident_id, Auditor_id, Action, Reason, Action_time`

2) 逻辑结构（关系与约束）
- `users_info(User_id)` 主键；与 `news_info(User_id)`、`incident_info(User_id)` 外键关联。
- `news_info` 的 `Status` 枚举语义：“已发布/审核中/待审核”。
- `incident_info` 的 `Incident_status` 枚举语义：“待审核/已归档/驳回”等。
- 审核日志表记录每次审核动作，便于追踪。

3) 物理结构（表定义与索引）
- 表结构与脚本位置：`DatabaseScripts/create_tables_core.sql`
- 索引设计：
  - `users_info(User_id)` 主键索引；登录检索加速。
  - `incident_info(User_id, Occur_time, Incident_type)` 组合索引，加速多条件查询（上报人/时间范围/类型）。
  - `news_info(Status, Publish_time)` 组合索引，加速状态/时间排序展示。
  - `law_info(Law_name, Category)` 普通索引，加速关键字与分类检索。

4) 视图、存储过程、触发器（满足课程要求）
- 视图：`dbo.v_PublishedNews`
  - 作用：仅展示“已发布”新闻并带作者名，供信息查询页直接使用。
  - 定义（文件：`DatabaseScripts/create_view_v_PublishedNews.sql`）：
    ```sql
    CREATE VIEW dbo.v_PublishedNews AS
    SELECT n.News_id, n.Title, n.Content, n.Publish_time, n.Status, u.User_name
    FROM dbo.news_info AS n
    JOIN dbo.users_info AS u ON n.User_id = u.User_id
    WHERE n.Status = N'已发布';
    ```
- 存储过程：`dbo.sp_InsertIncident`
  - 作用：统一插入不安全事件，设置默认状态“待审核”，记录上报时间。
  - 定义（文件：`DatabaseScripts/create_proc_sp_InsertIncident.sql`）：
    ```sql
    CREATE PROCEDURE dbo.sp_InsertIncident
        @Incident_type NVARCHAR(50),
        @Occur_time    DATETIME,
        @Location      NVARCHAR(50),
        @Description   NVARCHAR(MAX),
        @User_id       INT
    AS
    BEGIN
        SET NOCOUNT ON;
        INSERT INTO dbo.incident_info(
            Incident_type, Occur_time, Location, Description, User_id, Report_time, Incident_status
        ) VALUES (
            @Incident_type, @Occur_time, @Location, @Description, @User_id, GETDATE(), N'待审核'
        );
    END;
    ```
- 触发器：`dbo.trg_news_auto_publish_time`
  - 作用：当新闻状态改为“已发布”时，如未设置发布时间则自动填充当前时间。
  - 创建设备：在 `Global.asax` 启动中自动创建，避免手工执行；定义摘要：
    ```sql
    CREATE TRIGGER dbo.trg_news_auto_publish_time
    ON dbo.news_info
    AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE n
        SET Publish_time = ISNULL(n.Publish_time, GETDATE())
        FROM dbo.news_info n
        JOIN inserted i ON n.News_id = i.News_id
        WHERE i.Status = N'已发布' AND (n.Publish_time IS NULL);
    END;
    ```

四 系统详细设计与实现
1) 数据库服务器端
- 视图/存储过程/触发器的定义与作用：见第三章 4) 小节；系统首次启动时由 `Global.asax` 自动创建，降低环境搭建门槛。

2) 应用服务器端（页面与示例代码）
- 说明：以下示例为项目中已实现的典型功能，每段代码均含“函数级注释（小白解读）”。

- 数据录入示例：事件上报（文件：`IncidentReport.aspx.cs`）
  - 入口：按钮点击事件 `btnSubmit_Click`（示例逻辑）
  - 关键思路：从表单控件读取类型/时间/地点/描述，取当前用户 `User_id`，调用 `sp_InsertIncident` 插入数据；成功后提示并清空表单。
  - 示例片段：
    ```csharp
    /// <summary>
    /// 提交事件（小白解读）：
    /// 1) 读取页面输入；2) 基本校验；3) 调用存储过程统一插入；
    /// 4) 提示成功并清空表单，等待下一条录入。
    /// </summary>
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        var type = ddlType.SelectedValue;
        var occurTime = DateTime.Parse(txtOccurTime.Text);
        var location = txtLocation.Text.Trim();
        var desc = txtDescription.Text.Trim();
        var userId = Convert.ToInt32(Session["User_id"]);
        using (var conn = new SqlConnection(GetConnString()))
        {
            conn.Open();
            using (var cmd = new SqlCommand("dbo.sp_InsertIncident", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Incident_type", type);
                cmd.Parameters.AddWithValue("@Occur_time", occurTime);
                cmd.Parameters.AddWithValue("@Location", location);
                cmd.Parameters.AddWithValue("@Description", desc);
                cmd.Parameters.AddWithValue("@User_id", userId);
                cmd.ExecuteNonQuery();
            }
        }
        // 清空并提示...
    }
    ```

- 数据删除示例：管理页删除事件（文件：`IncidentInfoManage.aspx.cs`）
  - 入口：`GridView` 的 `RowCommand`，`CommandName="DeleteIncident"`。
  - 关键思路：读取行内的 `Incident_id`，执行 `DELETE FROM incident_info WHERE Incident_id=@id`，成功后 `Rebind()` 刷新列表。
  - 示例片段：
    ```csharp
    /// <summary>
    /// 删除事件（小白解读）：
    /// 从表格按钮拿到事件ID，执行数据库删除，并刷新当前分页数据。
    /// </summary>
    protected void gvIncident_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "DeleteIncident")
        {
            int id = Convert.ToInt32(e.CommandArgument);
            using (var conn = new SqlConnection(GetConnString()))
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM incident_info WHERE Incident_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            Rebind();
        }
    }
    ```

- 数据检索示例：事件查询（文件：`IncidentQuery.aspx.cs`）
  - 入口：查询按钮 `btnQuery_Click`。
  - 关键思路：拼接条件（编号/类型/起止时间）使用参数化 SQL，返回到 `GridView`，支持分页与描述换行展示。
  - 示例片段：
    ```csharp
    /// <summary>
    /// 查询事件（小白解读）：
    /// 安全地把用户输入作为参数，避免 SQL 注入，并把结果绑定到 GridView。
    /// </summary>
    protected void btnQuery_Click(object sender, EventArgs e)
    {
        using (var conn = new SqlConnection(GetConnString()))
        {
            conn.Open();
            var sql = "SELECT Incident_id, Incident_type, Occur_time, Location, Description, Incident_status FROM incident_info WHERE 1=1";
            // 根据输入追加条件...
            using (var cmd = new SqlCommand(sql, conn))
            {
                // cmd.Parameters.AddWithValue("@type", ddlType.SelectedValue);
                // cmd.Parameters.AddWithValue("@begin", beginTime);
                // cmd.Parameters.AddWithValue("@end", endTime);
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    gvResult.DataSource = dt;
                    gvResult.DataBind();
                }
            }
        }
    }
    ```

- 数据修改示例：管理页更新状态/类型（文件：`IncidentInfoManage.aspx.cs`）
  - 入口：`RowCommand` 或行内编辑事件。
  - 关键思路：校验新值合法性，参数化 `UPDATE`，成功后 `Rebind()` 刷新。
  - 示例片段：
    ```csharp
    /// <summary>
    /// 更新事件状态（小白解读）：
    /// 把行内下拉框选择的新状态写回数据库，刷新当前页，保证分页连续序号正确。
    /// </summary>
    protected void gvIncident_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "UpdateStatus")
        {
            int id = Convert.ToInt32(e.CommandArgument);
            string newStatus = GetStatusFromRow(e); // 从控件读取
            using (var conn = new SqlConnection(GetConnString()))
            {
                conn.Open();
                using (var cmd = new SqlCommand("UPDATE incident_info SET Incident_status=@s WHERE Incident_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@s", newStatus);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            Rebind();
        }
    }
    ```

- 安全性实现示例：登录与密码哈希（文件：`Login.aspx.cs`）
  - 要点：
    - 采用 MD5 小写 32位字符串存储密码，与字段 `Char(32)` 对齐。
    - 使用参数化查询，避免 SQL 注入；失败信息不暴露细节（“用户不存在或密码错误”）。
    - 登录后将 `User_id, User_name, User_type` 写入 `Session`，后续页面据此判断权限并跳转。
  - 片段：
    ```csharp
    /// <summary>
    /// 登录按钮点击（小白解读）：
    /// 验证用户名和密码（MD5），登录成功后按角色跳转到查询或管理页。
    /// </summary>
    protected void btnLogin_Click(object sender, EventArgs e)
    {
        string inputMd5 = ComputeMD5Hex(txtPassword.Text);
        using (var conn = new SqlConnection(GetConnString()))
        {
            conn.Open();
            using (var cmd = new SqlCommand("SELECT TOP 1 User_id, User_name, type, password FROM users_info WHERE User_name=@name", conn))
            {
                cmd.Parameters.AddWithValue("@name", txtUserName.Text);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) { lblMessage.Text = "用户不存在或密码错误"; return; }
                    if (!string.Equals(reader.GetString(3), inputMd5, StringComparison.OrdinalIgnoreCase))
                    { lblMessage.Text = "用户不存在或密码错误"; return; }
                    Session["User_id"] = reader.GetInt32(0);
                    Session["User_name"] = reader.GetString(1);
                    Session["User_type"] = reader.GetInt32(2);
                    Response.Redirect(reader.GetInt32(2) == 2 ? "~/IncidentQuery.aspx" : "~/IncidentInfoManage.aspx");
                }
            }
        }
    }
    ```

3) 关键 HTML/CSS/JavaScript 说明
- 列表页 `IncidentInfoManage.aspx` 的 `GridView`：
  - 启用分页：`AllowPaging="true" PageSize="10" OnPageIndexChanging="gvIncident_PageIndexChanging"`
  - 连续序号：新增 `TemplateField` + `RowDataBound` 计算 `序号 = PageIndex * PageSize + RowIndex + 1`。
  - 样式优化：描述列自动换行；列宽按中文类型统一；下拉框类型中文选项（设备故障/操作违规/航班异常/...）。
- 查询页 `IncidentQuery.aspx`：
  - 输入控件包括类型下拉、时间范围、编号输入；结果 `GridView` 绑定、分页、描述换行。
- 注册页 `Register.aspx`：
  - 支持“角色口令”识别管理员/审核人员角色，并在页面显示当前配置的口令提示（便于教学环境演示）。

五 系统说明
1) 系统开发环境
- 操作系统：Windows
- 数据库：SQL Server (本机或远程实例)
- 开发框架：.NET Framework 4.7.2，ASP.NET WebForms
- 依赖：`System.Data.SqlClient`，Roslyn 编译支持包

2) 安装、配置与发布步骤
- 数据库准备：
  - 建议使用 SQL Server 或 SQL Server Express；确保有创建数据库/对象权限。
  - 如需手工执行脚本，按顺序运行：
    - `DatabaseScripts/create_database_if_not_exists.sql`
    - `DatabaseScripts/create_tables_core.sql`
    - `DatabaseScripts/create_view_v_PublishedNews.sql`
    - `DatabaseScripts/create_proc_sp_InsertIncident.sql`
  - 也可直接启动站点，`Global.asax` 会自动创建缺失对象（视图/存储过程/触发器/日志表）。
- 应用配置：
  - 在 `Web.config` 或 `bin/*.config` 中设置 `connectionStrings:AviationDb`，示例：
    `Data Source=localhost\\SQLEXPRESS;Initial Catalog=AviationDb;User ID=sa;Password=你的密码;Encrypt=False`
  - 可选：在 `appSettings` 设置 `AdminRegisterKey`、`AuditorRegisterKey` 用于角色口令注册（教学演示）。
- 发布与运行：
  - 使用 IIS 或 Visual Studio 内置服务器（F5）启动；首次启动会执行 `Global.asax` 种子逻辑，创建默认管理员账号 `admin / Admin@123`（密码以 MD5 存储）。
  - 打开 `Login.aspx` 登录；管理员进入 `IncidentInfoManage.aspx`；普通用户进入 `IncidentQuery.aspx`。

六 课程设计总结（≥300字）
- 本系统实现了用户注册/登录与角色权限控制、新闻信息发布与展示、不安全事件的上报/查询/审核/删除/修改等核心功能。数据库层面满足课程要求，包含视图、存储过程与触发器，并通过索引提升查询性能。应用层采用 WebForms 与 ADO.NET 参数化查询，结合 Session 做访问控制，整体结构清晰、易于教学演示。难点主要在于：
  - 在管理页实现分页与“连续序号”计算，需正确处理 `PageIndex` 和当前行索引，避免跨页断号；
  - 描述列长文本的展示与换行，结合 CSS 控制列宽与 `word-wrap`；
  - 角色注册口令的交互设计，既要保证安全性又要兼容教学便利；
  - 数据库对象的自动化创建（视图/存储过程/触发器），需考虑 SQL 语法在 IF 包裹下的限制，采用动态 SQL 解决。
- 解决方案上，通过 `RowDataBound` 计算序号、`TemplateField` 控件化列渲染、参数化查询与异常兜底提示提升了可用性与安全性。仍存在的改进方向包括：
  - 密码哈希从 MD5 升级到更安全的算法（如 `PBKDF2/Bcrypt/Argon2`），并增加盐；
  - 事件状态设计更细化（处理中/已驳回/已归档），并完善审核日志页面；
  - 引入前端框架改善交互与适配性（如 Bootstrap）；
  - 增加导出功能（CSV/Excel）与更丰富的查询条件（地点模糊匹配等）。
- 通过本课程设计，对数据库对象设计与应用层数据访问、安全性实践有了系统理解，掌握了从需求到实现再到部署的端到端流程。

七 规范性要求
- 文档组织与内容对齐《系统设计文档参考模板》，字型与版式在 Word/PDF 中按要求排版（宋体 + Times New Roman，正文小四号，单倍行距）。
- 交付内容包含：
  - 系统设计报告（Word/PDF，含电子签）
  - 程序源代码（本项目全部文件）
  - 数据库脚本或备份文件（`DatabaseScripts` 文件夹或 `.bak`）

附：页面导航速览
- `Login.aspx` 登录；`Register.aspx` 注册（角色口令可选）
- `UserManage.aspx` 用户管理（管理员）
- `NewsManage.aspx` 信息发布（管理员），展示视图 `v_PublishedNews`
- `IncidentReport.aspx` 事件上报（所有已登录用户）
- `IncidentQuery.aspx` 事件查询（所有已登录用户）
- `IncidentInfoManage.aspx` 事件管理（管理员/审核）