-- 存储过程：sp_InsertIncident（小白解读）
-- 作用：统一插入不安全事件，默认状态设置为“待审核”，并记录上报时间。
-- 参数：
--   @Incident_type   事件类型（如 设备故障/操作违规）
--   @Occur_time      发生时间（精确到分钟）
--   @Location        发生地点（机场/航站楼等）
--   @Description     详细描述（不少于20字，应用端已校验）
--   @User_id         上报人编号

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