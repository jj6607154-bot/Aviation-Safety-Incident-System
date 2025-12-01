-- 视图：v_PublishedNews（小白解读）
-- 作用：只展示“已发布”的新闻，并带上作者名字，方便直接查询展示。
-- 注意：如果视图已存在，Global.asax 的启动脚本会跳过创建。

CREATE VIEW dbo.v_PublishedNews AS
SELECT
    n.News_id,
    n.Title,
    n.Content,
    n.Publish_time,
    n.Status,
    u.User_name
FROM dbo.news_info AS n
JOIN dbo.users_info AS u ON n.User_id = u.User_id
WHERE n.Status = '已发布';