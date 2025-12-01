-- 创建数据库（如果不存在）
-- 小白解读：这段脚本会检查名为 AviationDb 的数据库是否存在；
-- 如果不存在，就创建一个新的数据库。你可以在 SSMS 里执行它。

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'AviationDb')
BEGIN
    PRINT N'数据库 AviationDb 不存在，开始创建...';
    CREATE DATABASE AviationDb;
    PRINT N'数据库 AviationDb 创建完成。';
END
ELSE
BEGIN
    PRINT N'数据库 AviationDb 已存在，跳过创建。';
END

-- 提示：
-- 1) 如果你使用非 LocalDB 的 SQL Server，请确保你有创建数据库权限。
-- 2) 如果需要指定数据文件位置或大小，可以改为：
--    CREATE DATABASE AviationDb ON 
--    ( NAME = N'AviationDb', FILENAME = N'C:\Data\AviationDb.mdf', SIZE = 50MB, FILEGROWTH = 10MB )
--    LOG ON ( NAME = N'AviationDb_log', FILENAME = N'C:\Data\AviationDb_log.ldf', SIZE = 20MB, FILEGROWTH = 10MB );
--    上述路径需要存在且你有写权限。

GO

-- 可选：切换到该数据库，后续可以继续创建表/视图/存储过程
USE AviationDb;
GO

-- 这里可以放置后续对象创建脚本，例如：
-- IF OBJECT_ID(N'dbo.incident_audit_log', N'U') IS NULL
-- BEGIN
--   CREATE TABLE dbo.incident_audit_log(
--       audit_id INT IDENTITY(1,1) PRIMARY KEY,
--       incident_id INT NOT NULL,
--       auditor NVARCHAR(50) NOT NULL,
--       action NVARCHAR(50) NOT NULL,
--       reason NVARCHAR(200) NULL,
--       audit_time DATETIME NOT NULL DEFAULT GETDATE()
--   );
-- END