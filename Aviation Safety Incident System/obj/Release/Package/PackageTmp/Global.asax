<%@ Application Language="C#" %>
<%@ Import Namespace="System.Configuration" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.IO" %>
<script runat="server">

    /// <summary>
    /// 获取数据库连接字符串（小白解读）：
    /// 从 Web.config 的 connectionStrings 中读取 key 为 "AviationDb" 的连接串。
    /// </summary>
    private string GetConnString()
    {
        return ConfigurationManager.ConnectionStrings["AviationDb"].ConnectionString;
    }

    /// <summary>
    /// 确保数据库对象存在（视图/存储过程/触发器/日志表）（小白解读）：
    /// 第一次启动站点或数据库为空时，我们自动创建这些对象，避免你手动执行 SQL。
    /// 说明：
    /// - 视图 v_PublishedNews：只展示已发布新闻并带作者名；
    /// - 存储过程 sp_InsertIncident：统一插入不安全事件，状态默认“待审核”；
    /// - 触发器 trg_news_auto_publish_time：新闻状态改为“已发布”时自动填充发布时间；
    /// - 审核日志表 incident_audit_log：记录审核操作。
    /// </summary>
    private void EnsureDatabaseObjects()
    {
        try
        {
            using (var conn = new SqlConnection(GetConnString()))
            {
                conn.Open();
                // 0) 核心业务表：users_info / incident_info / news_info（小白解读：这些是系统基础表）
                using (var cmdTables = new SqlCommand(@"
IF OBJECT_ID('dbo.users_info','U') IS NULL
BEGIN
    CREATE TABLE dbo.users_info(
        User_id INT IDENTITY(1,1) PRIMARY KEY,
        User_name NVARCHAR(50) NOT NULL,
        password NVARCHAR(64) NOT NULL,
        type INT NOT NULL,
        email NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX UX_users_info_User_name ON dbo.users_info(User_name);
END

IF OBJECT_ID('dbo.incident_info','U') IS NULL
BEGIN
    CREATE TABLE dbo.incident_info(
        Incident_id INT IDENTITY(1,1) PRIMARY KEY,
        Incident_type NVARCHAR(50) NOT NULL,
        Occur_time DATETIME NOT NULL,
        Location NVARCHAR(50) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        User_id INT NOT NULL,
        Report_time DATETIME NOT NULL DEFAULT GETDATE(),
        Incident_status NVARCHAR(20) NOT NULL DEFAULT N'待审核'
    );
    ALTER TABLE dbo.incident_info
        ADD CONSTRAINT FK_incident_user
        FOREIGN KEY (User_id) REFERENCES dbo.users_info(User_id);
    CREATE INDEX IX_incident_info_status_time ON dbo.incident_info(Incident_status, Report_time);
END

IF OBJECT_ID('dbo.news_info','U') IS NULL
BEGIN
    CREATE TABLE dbo.news_info(
        News_id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        User_id INT NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT N'审核中',
        Publish_time DATETIME NULL
    );
    ALTER TABLE dbo.news_info
        ADD CONSTRAINT FK_news_user
        FOREIGN KEY (User_id) REFERENCES dbo.users_info(User_id);
    CREATE INDEX IX_news_info_status_time ON dbo.news_info(Status, Publish_time);
END
                ", conn))
                {
                    cmdTables.ExecuteNonQuery();
                }
                // 修复列类型：如果历史表 users_info.type 是 NVARCHAR，则自动迁移为 INT
                // 小白解读：老版本可能把角色类型存成文本，这里检测后改成数字类型，兼容当前代码读取为 int。
                using (var cmdFixType = new SqlCommand(@"
IF OBJECT_ID('dbo.users_info','U') IS NOT NULL
BEGIN
    DECLARE @typeName NVARCHAR(128);
    SELECT @typeName = t.name
    FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('dbo.users_info') AND c.name = 'type';
    IF @typeName = 'nvarchar'
    BEGIN
        ALTER TABLE dbo.users_info ALTER COLUMN type INT NOT NULL;
    END
END
                ", conn))
                {
                    cmdFixType.ExecuteNonQuery();
                }
                // 1) 审核日志表（可直接在 IF 里 CREATE TABLE）
                using (var cmdLog = new SqlCommand(@"
IF OBJECT_ID('dbo.incident_audit_log','U') IS NULL
BEGIN
    CREATE TABLE dbo.incident_audit_log(
        Log_id INT IDENTITY(1,1) PRIMARY KEY,
        Incident_id INT NOT NULL,
        Auditor_id INT NOT NULL,
        Action NVARCHAR(50) NOT NULL,
        Reason NVARCHAR(4000) NULL,
        Action_time DATETIME NOT NULL DEFAULT GETDATE()
    );
END", conn))
                {
                    cmdLog.ExecuteNonQuery();
                }

                // 修复审核日志的 Reason 列类型（小白讲解）：
                // 老库可能把 Reason 建成 VARCHAR，存中文就会显示为“???”。
                // 这里检测列的实际类型，如果不是 nvarchar，就自动改为 NVARCHAR(4000)。
                using (var cmdFixReason = new SqlCommand(@"
IF OBJECT_ID('dbo.incident_audit_log','U') IS NOT NULL
BEGIN
    DECLARE @typeName NVARCHAR(128);
    SELECT @typeName = t.name
    FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('dbo.incident_audit_log') AND c.name = 'Reason';
    IF @typeName IS NOT NULL AND @typeName <> 'nvarchar'
    BEGIN
        ALTER TABLE dbo.incident_audit_log ALTER COLUMN Reason NVARCHAR(4000) NULL;
    END
END", conn))
                {
                    cmdFixReason.ExecuteNonQuery();
                }

                // 修复事件状态列类型（小白讲解）：避免显示中文为“???”，统一用 NVARCHAR(20)
                using (var cmdFixIncidentStatus = new SqlCommand(@"
IF OBJECT_ID('dbo.incident_info','U') IS NOT NULL
BEGIN
    DECLARE @typeName NVARCHAR(128);
    SELECT @typeName = t.name
    FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('dbo.incident_info') AND c.name = 'Incident_status';
    IF @typeName IS NOT NULL AND @typeName <> 'nvarchar'
    BEGIN
        ALTER TABLE dbo.incident_info ALTER COLUMN Incident_status NVARCHAR(20) NOT NULL;
    END
END", conn))
                {
                    cmdFixIncidentStatus.ExecuteNonQuery();
                }

                // 修复新闻状态列类型（小白讲解）：同样统一为 NVARCHAR(20)
                using (var cmdFixNewsStatus = new SqlCommand(@"
IF OBJECT_ID('dbo.news_info','U') IS NOT NULL
BEGIN
    DECLARE @typeName NVARCHAR(128);
    SELECT @typeName = t.name
    FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('dbo.news_info') AND c.name = 'Status';
    IF @typeName IS NOT NULL AND @typeName <> 'nvarchar'
    BEGIN
        ALTER TABLE dbo.news_info ALTER COLUMN Status NVARCHAR(20) NOT NULL;
    END
END", conn))
                {
                    cmdFixNewsStatus.ExecuteNonQuery();
                }

                using (var cmdRepairIncidentStatus = new SqlCommand(@"
UPDATE i
SET Incident_status = CASE last.Action
    WHEN 'ApprovePublic' THEN N'已公开'
    WHEN 'ApproveProcessing' THEN N'处理中'
    WHEN 'Reject' THEN N'已驳回'
    ELSE N'已上报'
END
FROM dbo.incident_info AS i
OUTER APPLY (
    SELECT TOP 1 l.Action FROM dbo.incident_audit_log AS l WHERE l.Incident_id = i.Incident_id ORDER BY l.Action_time DESC
) AS last
WHERE i.Incident_status IS NULL OR i.Incident_status NOT IN (N'待审核', N'已上报', N'已驳回', N'处理中', N'已公开')", conn))
                {
                    cmdRepairIncidentStatus.ExecuteNonQuery();
                }

                // 2) 视图（通过动态 SQL 创建，避免 CREATE VIEW 不能置于 IF 的限制）
                using (var cmdView = new SqlCommand(@"
IF OBJECT_ID('dbo.v_PublishedNews', 'V') IS NULL
BEGIN
    EXEC('CREATE VIEW dbo.v_PublishedNews AS
          SELECT n.News_id, n.Title, n.Content, n.Publish_time, n.Status,
                 u.User_name
          FROM dbo.news_info n
          JOIN dbo.users_info u ON n.User_id = u.User_id
          WHERE n.Status = ''已发布''');
END", conn))
                {
                    cmdView.ExecuteNonQuery();
                }

                // 3) 存储过程（统一事件插入）
                using (var cmdProc = new SqlCommand(@"
IF OBJECT_ID('dbo.sp_InsertIncident', 'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE dbo.sp_InsertIncident
          @Incident_type NVARCHAR(50),
          @Occur_time DATETIME,
          @Location NVARCHAR(50),
          @Description NVARCHAR(MAX),
          @User_id INT
      AS
      BEGIN
          SET NOCOUNT ON;
          INSERT INTO dbo.incident_info(Incident_type, Occur_time, Location, Description, User_id, Report_time, Incident_status)
          VALUES(@Incident_type, @Occur_time, @Location, @Description, @User_id, GETDATE(), N''待审核'');
      END');
END", conn))
                {
                    cmdProc.ExecuteNonQuery();
                }

                // 4) 触发器（状态改为“已发布”时自动补 Publish_time）
                using (var cmdTrg = new SqlCommand(@"
IF OBJECT_ID('dbo.trg_news_auto_publish_time', 'TR') IS NULL
BEGIN
    EXEC('CREATE TRIGGER dbo.trg_news_auto_publish_time
          ON dbo.news_info
          AFTER INSERT, UPDATE
      AS
      BEGIN
          SET NOCOUNT ON;
          IF TRIGGER_NESTLEVEL() > 1 RETURN;
          UPDATE n
          SET Publish_time = ISNULL(n.Publish_time, GETDATE())
          FROM dbo.news_info n
          JOIN inserted i ON n.News_id = i.News_id
          WHERE i.Status = N''已发布'' AND (n.Publish_time IS NULL);
      END');
END
ELSE
BEGIN
    EXEC('ALTER TRIGGER dbo.trg_news_auto_publish_time
          ON dbo.news_info
          AFTER INSERT, UPDATE
      AS
      BEGIN
          SET NOCOUNT ON;
          IF TRIGGER_NESTLEVEL() > 1 RETURN;
          UPDATE n
          SET Publish_time = ISNULL(n.Publish_time, GETDATE())
          FROM dbo.news_info n
          JOIN inserted i ON n.News_id = i.News_id
          WHERE i.Status = N''已发布'' AND (n.Publish_time IS NULL);
      END');
END", conn))
                {
                    cmdTrg.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            // 小白解读：这里不抛异常以免站点启动失败，仅记录到应用日志；
            // 你也可以改为抛出，便于开发阶段立即发现问题。
            System.Diagnostics.Trace.WriteLine("EnsureDatabaseObjects error: " + ex.Message);
        }
    }

    /// <summary>
    /// 获取本机用户目录下的数据库文件夹（小白解读）：
    /// 我们在 `%LOCALAPPDATA%` 下创建一个专属目录 `AviationDb`，避免权限问题。
    /// </summary>
    private string GetLocalDbFolder()
    {
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(baseDir, "AviationDb");
    }

    /// <summary>
    /// 确保本机数据库文件就位（小白解读）：
    /// - 如果本地目录不存在，则创建；
    /// - 如果 `hw.mdf / hw_log.ldf` 不在本地目录，就从项目的 `~/Database` 里复制过去；
    /// - 这样 LocalDB 就能顺利附加这些文件，避免“拒绝访问”。
    /// </summary>
    private void EnsureLocalDbFiles()
    {
        try
        {
            var localFolder = GetLocalDbFolder();
            if (!Directory.Exists(localFolder))
            {
                Directory.CreateDirectory(localFolder);
            }

            var sourceFolder = Server.MapPath("~/Database");
            var sourceMdf = Path.Combine(sourceFolder, "hw.mdf");
            var sourceLdf = Path.Combine(sourceFolder, "hw_log.ldf");
            var targetMdf = Path.Combine(localFolder, "hw.mdf");
            var targetLdf = Path.Combine(localFolder, "hw_log.ldf");

            // 如果目标不存在且源存在，则复制
            if (!File.Exists(targetMdf) && File.Exists(sourceMdf))
            {
                File.Copy(sourceMdf, targetMdf, true);
            }
            if (!File.Exists(targetLdf) && File.Exists(sourceLdf))
            {
                File.Copy(sourceLdf, targetLdf, true);
            }
        }
        catch (Exception ex)
        {
            // 小白解读：复制失败不会阻止网站启动，但请检查文件权限或手动复制。
            System.Diagnostics.Trace.WriteLine("EnsureLocalDbFiles error: " + ex.Message);
        }
    }

    // 应用程序启动事件（网站首次启动时触发）
    // 小白解读：命名库模式——自动确保 AviationDb 存在，并创建表/视图/存储过程/触发器。
    void Application_Start(object sender, EventArgs e)
    {
        // 1) 确保命名数据库存在（LocalDB 或你的实例上）；
        EnsureNamedDatabaseExists();

        // 2) 确保数据库对象齐备（核心表/视图/存储过程/触发器/审核日志表）
        EnsureDatabaseObjects();

        // 3) 种子数据：确保有一个默认管理员账号，便于你登录验证
        EnsureSeedAdminUser();
    }

    // 应用程序结束事件（网站停止时触发）
    // 小白解读：目前不需要做任何清理，这里留空即可。
    void Application_End(object sender, EventArgs e)
    {
    }

    /// <summary>
    /// 全局请求拦截（小白解读）：
    /// 有人访问 /Home.aspx（含虚拟目录的路径，如 /Aviation Safety Incident System/Home.aspx）时，
    /// 我们在请求最早阶段直接把它重定向到真正首页 ~/Default.aspx，防止 404。
    /// 说明：
    /// - 这是“兜底”策略，即使物理页面不存在也能跳转；
    /// - 会保留原请求的 QueryString（?后面的参数），避免信息丢失。
    /// </summary>
    void Application_BeginRequest(object sender, EventArgs e)
    {
        try
        {
            var path = Request.Url.LocalPath ?? string.Empty; // 例如：/Home.aspx 或 /Aviation Safety Incident System/Home.aspx
            // 小白讲解：有人直接访问 /~/IncidentQuery.aspx 这样的地址时，浏览器不会解析 ~，就会报 404。
            // 我们在这里做一个“纠错”——如果路径里包含 /~/，就自动改成正常的 / 路径，然后重定向过去。
            // 举例：/~/IncidentQuery.aspx -> /IncidentQuery.aspx；/Aviation Safety Incident System/~/Regulations.aspx -> /Aviation Safety Incident System/Regulations.aspx
            if (path.Contains("/~/"))
            {
                var fixedPath = path.Replace("/~/", "/");
                var qs = Request.Url.Query ?? string.Empty; // 保留原始查询字符串
                Response.Redirect(fixedPath + qs, true);
                return; // 已重定向，后续逻辑不再执行
            }
            var lower = path.ToLowerInvariant();
            if (lower.EndsWith("/home.aspx"))
            {
                var qs = Request.Url.Query ?? string.Empty; // 包含 ?，例如 ?a=1
                var target = string.IsNullOrEmpty(qs) ? "~/Default.aspx" : ("~/Default.aspx" + qs);
                Response.Redirect(target, true);
            }
        }
        catch
        {
            // 安全兜底：任何异常都不影响其它页面请求。
        }
    }

    /// <summary>
    /// 确保命名数据库存在（小白解读）：
    /// 临时把连接串的 Initial Catalog 改为 master，连到系统库创建 AviationDb。
    /// </summary>
    private void EnsureNamedDatabaseExists()
    {
        try
        {
            var cs = GetConnString();
            var builder = new SqlConnectionStringBuilder(cs);
            var dbName = builder.InitialCatalog;
            if (string.IsNullOrEmpty(dbName)) return;

            builder.InitialCatalog = "master";
            using (var conn = new SqlConnection(builder.ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
DECLARE @db sysname = @dbName;
IF DB_ID(@db) IS NULL
BEGIN
    EXEC('CREATE DATABASE [' + @db + ']');
END
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@dbName", dbName);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine("EnsureNamedDatabaseExists error: " + ex.Message);
        }
    }

    /// <summary>
    /// 种子创建管理员账号（小白解读）：
    /// 如果还没有任何管理员（type=1），自动创建一个用户名为 admin，密码为 Admin@123 的账号。
    /// 密码会做 MD5 存入 password 字段。你可以登录后在“用户管理”里改密码。
    /// </summary>
    private void EnsureSeedAdminUser()
    {
        try
        {
            using (var conn = new SqlConnection(GetConnString()))
            {
                conn.Open();
                // 检查是否已有管理员账号
                using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.users_info WHERE type = 1", conn))
                {
                    int adminCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (adminCount > 0)
                    {
                        return; // 已有管理员则不再创建
                    }
                }

                // 计算 Admin@123 的 MD5（与前端一致的加密方式）
                string md5;
                using (var md5Alg = System.Security.Cryptography.MD5.Create())
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes("Admin@123");
                    var hash = md5Alg.ComputeHash(bytes);
                    var sb = new System.Text.StringBuilder(hash.Length * 2);
                    foreach (var b in hash)
                    {
                        sb.Append(b.ToString("x2"));
                    }
                    md5 = sb.ToString();
                }

                // 插入一个默认管理员账号
                using (var insertCmd = new SqlCommand(
                    "INSERT INTO dbo.users_info(User_name, password, type, email) VALUES(@name, @pwd, @type, @mail)", conn))
                {
                    insertCmd.Parameters.AddWithValue("@name", "admin");
                    insertCmd.Parameters.AddWithValue("@pwd", md5);
                    insertCmd.Parameters.AddWithValue("@type", 1);
                    insertCmd.Parameters.AddWithValue("@mail", DBNull.Value);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            // 小白解读：种子数据失败不会影响站点运行，你可以手动注册管理员或修复后重启。
            System.Diagnostics.Trace.WriteLine("EnsureSeedAdminUser error: " + ex.Message);
        }
    }
    /// <summary>
    /// 全局错误处理（小白解读）：
    /// 当页面发生未捕获异常时，这里拦截并在 IncidentReport.aspx 上直接输出详细错误，
    /// 方便我们定位 500 的根因。其他页面不影响正常行为。
    /// </summary>
    void Application_Error(object sender, EventArgs e)
    {
        Exception ex = Server.GetLastError();
        try
        {
            var url = Request != null && Request.Url != null ? Request.Url.ToString() : string.Empty;
            if (!string.IsNullOrEmpty(url) && url.IndexOf("IncidentReport.aspx", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Response.Clear();
                Response.ContentType = "text/plain; charset=utf-8";
                Response.Write("IncidentReport.aspx 错误：\r\n" + (ex == null ? "未知错误" : ex.ToString()));
                Response.Flush();
                Server.ClearError();
            }
        }
        catch { /* 忽略这里的次要异常，避免影响其他请求 */ }
    }
</script>
