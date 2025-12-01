-- 触发器：trg_news_auto_publish_time（小白解读）
-- 作用：当新闻状态被设置为“已发布”时，如果发布时间为空，则自动填充为当前时间。
-- 场景：避免忘记写 Publish_time；支持 INSERT/UPDATE 两种情况。

CREATE TRIGGER dbo.trg_news_auto_publish_time
ON dbo.news_info
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE n
    SET Publish_time = ISNULL(n.Publish_time, GETDATE())
    FROM dbo.news_info AS n
    JOIN inserted AS i ON n.News_id = i.News_id
    WHERE i.Status = N'已发布' AND n.Publish_time IS NULL;
END;