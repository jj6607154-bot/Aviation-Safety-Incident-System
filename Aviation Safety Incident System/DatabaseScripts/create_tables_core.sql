-- 创建命名库 AviationDb 的核心业务表结构
-- 小白解读：这些是系统必须的三张基础表，以及一个审核日志表。
-- 在 SSMS 里选择数据库 AviationDb，执行本脚本即可。

-- users_info：用户信息
IF OBJECT_ID(N'dbo.users_info', N'U') IS NULL
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
GO

-- incident_info：不安全事件信息
IF OBJECT_ID(N'dbo.incident_info', N'U') IS NULL
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
GO

-- news_info：新闻信息
IF OBJECT_ID(N'dbo.news_info', N'U') IS NULL
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
GO

-- incident_audit_log：事件审核日志
IF OBJECT_ID(N'dbo.incident_audit_log', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.incident_audit_log(
        Log_id INT IDENTITY(1,1) PRIMARY KEY,
        Incident_id INT NOT NULL,
        Auditor_id INT NOT NULL,
        Action NVARCHAR(50) NOT NULL,
        Reason NVARCHAR(4000) NULL,
        Action_time DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 小提示：视图/存储过程/触发器已在 Global.asax 启动时自动创建。
-- 如需单独执行，可用 DatabaseScripts 目录下的对应脚本。