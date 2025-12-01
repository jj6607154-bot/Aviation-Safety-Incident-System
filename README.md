# Aviation-Safety-Incident-System
面向航空安全事件与新闻的管理与发布，支持事件上报、审核、公开/下架、日志留存，以及新闻的撰写、发布与下架
**项目定位**
- 面向航空安全事件与新闻的管理与发布，支持事件上报、审核、公开/下架、日志留存，以及新闻的撰写、发布与下架。
- 采用 ASP.NET WebForms（C#）+ SQL Server（LocalDB）的典型信息管理系统，适配 Windows 环境与 IIS Express。

**技术栈**
- 后端框架：`ASP.NET WebForms`（C#，代码后置 .aspx.cs）
- 数据库：`SQL Server LocalDB`，连接字符串名：`AviationDb`（在 `Web.config`）
- 运行环境：`Windows + IIS Express`，当前监听 `http://localhost:5082/`

**核心页面**
- 事件查询：`IncidentQuery.aspx`  
  - 列表查询，支持“查看全文”弹窗详情（不跳转）；详情取自 `incident_info`，文本做 `HtmlEncode` 防 XSS  
  - 代码参考：`IncidentQuery.aspx.cs:433` 的下架逻辑、`IncidentQuery.aspx.cs:ShowDetail(...)` 加载详情
- 事件审核：`IncidentAudit.aspx`  
  - 审核动作：`ApprovePublic`（已公开）、`ApproveProcessing`（处理中）、`Unpublish`（已下架）  
  - 审核日志写入 `incident_audit_log`  
  - 代码参考：`IncidentAudit.aspx.cs:277-333` 行处理状态为“已下架”
- 事件信息管理：`IncidentInfoManage.aspx`  
  - 审核员“下架-取消公开”已统一写入“已下架”状态  
  - 代码参考：`IncidentInfoManage.aspx.cs:462-468` 更新 `Incident_status=N'已下架'`
- 已发布新闻：`NewsPublished.aspx`  
  - 下架按钮将新闻状态改为“已下架”，并清空 `Publish_time`  
  - 代码参考：`NewsPublished.aspx.cs:120-125`
- 新闻发布：`NewsManage.aspx`  
  - 管理员发布新闻，状态置为“已发布”，首发时写入 `Publish_time`  
  - 代码参考：`NewsManage.aspx.cs:168-176`
- 新闻详情：`NewsDetail.aspx`  
  - 读取标题、正文、作者、发布时间、状态；对异常状态做规范化显示  
  - 代码参考：`NewsDetail.aspx.cs:21-46` 的状态规范函数、`NewsDetail.aspx.cs:67-92` 加载详情
- 我的消息详情：`MyMessageDetail.aspx`  
  - 审核处理记录组件已按你要求隐藏，且后台不再加载日志
- 首页：`Default.aspx`  
  - 功能入口与简介，“留痕可追溯。”文案已移除

**数据模型**
- `dbo.incident_info`：事件主表，含 `Incident_status`（常见：`已上报`、`处理中`、`已公开`、`已下架`）、`Description` 等
- `dbo.incident_audit_log`：事件审核日志，含 `Incident_id`、`Action`（如 `ApprovePublic`、`Unpublish`）、`Reason`、`Auditor_id`、`Action_time`
- `dbo.news_info`：新闻主表，含 `Title`、`Content`、`Status`（`已发布`、`审核中`、`已下架`）、`Publish_time`、`User_id`
- `dbo.users_info`：用户信息（`User_name`、`User_type` 等）
- 视图：`dbo.v_PublishedNews`（聚合已发布新闻与作者名，供列表展示）

**角色与权限**
- 管理员：`User_type == 1`，可发布新闻、下架新闻、执行部分事件操作
- 审核员：`User_type == 3`，可对事件执行审核动作与“下架（已下架）”
- 普通用户/游客：只读为主；个别操作受限于角色判断  
  - 例：`NewsPublished.aspx.cs:169-175` 仅管理员显示“下架”按钮

**状态口径（统一）**
- 事件：`已上报` → 审核 → `已公开` 或 `处理中` → 可“下架”至 `已下架`
- 新闻：`已发布` → 可“下架”至 `已下架`（不再回到`审核中`）
- 历史兼容：个别地方存在“审核中”旧逻辑，已按你的要求统一为“已下架”

**日志与安全**
- 所有数据库写入均使用参数化 SQL，防止 SQL 注入（如：`NewsPublished.aspx.cs:123`）
- 详情展示对正文 `HtmlEncode`，避免 XSS（如：`IncidentQuery.aspx.cs:ShowDetail(...)`）
- 审核日志完整记录动作与备注（如：`IncidentInfoManage.aspx.cs:470-479`）

**运行方式（Windows）**
- 已启动 IIS Express：`http://localhost:5082/`  
- 如需手动启动，可在终端运行：  
  ```
  C:\Program Files\IIS Express\iisexpress.exe /path:"d:\shujuku\Aviation Safety Incident System" /port:5082
  ```
- 访问入口：  
  - 首页：`http://localhost:5082/`  
  - 事件审核：`http://localhost:5082/IncidentAudit.aspx`  
  - 已发布新闻：`http://localhost:5082/NewsPublished.aspx`

