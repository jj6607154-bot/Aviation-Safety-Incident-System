<%@ Page Title="事件审核" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="IncidentInfoManage.aspx.cs" Inherits="WebForms.IncidentInfoManage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <style>
    /* 页面整体样式 */
    body {
      background: linear-gradient(135deg, #f8fbff 0%, #f0f7ff 100%);
      font-family: "微软雅黑", "Segoe UI", Arial, sans-serif;
    }

    /* 页面容器与标题样式 */
    .page-wrapper { max-width: 1100px; margin: 10px auto 30px; padding: 0 12px; }
    .card { background:#fff; border-radius: 16px; box-shadow: 0 10px 30px rgba(0,0,0,.08); padding: 20px 24px; }
    .toolbar { display:flex; gap:12px; align-items:center; margin-bottom:14px; }
    h2 {
      color: #0c5aa6;
      font-size: 28px;
      font-weight: 700;
      margin: 6px 0 16px 0;
      padding-bottom: 12px;
      border-bottom: 2px solid #e6f7ff;
    }

    h3 {
      color: #0c5aa6;
      font-size: 20px;
      font-weight: 600;
      margin: 30px 0 15px;
      padding-left: 8px;
      border-left: 4px solid #1890ff;
    }

    /* 消息提示样式 */
    #lblMessage {
      display: block;
      padding: 12px 16px;
      margin-bottom: 20px;
      border-radius: 8px;
      font-size: 14px;
      font-weight: 500;
      border-left: 4px solid #ddd;
      background: #f9f9f9;
    }

    /* 审核表格样式 - 美化版 */
    .audit-grid {
      table-layout: fixed;
      width: 100%;
      border-collapse: collapse;
      background: #fff;
      border-radius: 12px;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
      overflow: hidden;
      margin-bottom: 24px;
    }

    .audit-grid td, .audit-grid th {
      padding: 18px 20px;
      vertical-align: top;
      transition: all 0.3s ease;
    }

    .audit-grid .grid-header th {
      background: linear-gradient(135deg, #f8fbff, #e6f7ff);
      color: #0c5aa6;
      font-weight: 700;
      font-size: 16px;
      border-bottom: 2px solid #e6f7ff;
      padding: 20px 22px;
    }

    .audit-grid .row td {
      border-bottom: 1px solid #f0f8ff;
      background: #fff;
    }

    .audit-grid .row-alt td {
      background: #fafcff;
      border-bottom: 1px solid #f0f8ff;
    }

    .audit-grid .row:hover td {
      background: #eef6ff;
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(24, 144, 255, 0.1);
    }

    .audit-grid .grid-pager {
      padding: 16px;
      background: linear-gradient(135deg, #f8fbff, #e6f7ff);
      border-top: 1px solid #e6f7ff;
      text-align: center;
    }

    .audit-grid .grid-pager a, .audit-grid .grid-pager span {
      margin: 0 4px;
      padding: 8px 12px;
      border: 1px solid #e6f7ff;
      border-radius: 6px;
      text-decoration: none;
      color: #1890ff;
      font-weight: 500;
      transition: all 0.3s ease;
    }

    .audit-grid .grid-pager a:hover {
      background: linear-gradient(135deg, #1890ff, #36cfc9);
      color: #fff;
      border-color: #1890ff;
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(24, 144, 255, 0.25);
    }

    .audit-grid .grid-pager span {
      background: linear-gradient(135deg, #1890ff, #36cfc9);
      color: #fff;
      border-color: #1890ff;
    }

    /* 列宽设置 */
    .col-serial { width: 5%; text-align: center; }
    .col-id { width: 7%; text-align: center; }
    .col-type { width: 10%; }
    .col-time { width: 14%; }
    .col-loc { width: 10%; }
    .col-desc { width: 22%; }
    .col-ops { width: 18%; }

    /* 描述文本样式 */
    .desc-wrap {
      white-space: pre-wrap;
      word-break: break-word;
      font-size: 15px;
      line-height: 1.85;
      color: #1f1f1f;
    }
    .desc-wrap.clamp {
      display: -webkit-box;
      -webkit-line-clamp: 5;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    /* 操作按钮样式 */
    .action-btn {
      margin: 2px 4px 2px 0;
      padding: 8px 12px;
      border: none;
      border-radius: 6px;
      font-size: 12px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s ease;
      text-decoration: none;
      display: inline-block;
      text-align: center;
      min-width: 80px;
      box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
    }

    #btnApprovePublic { background: linear-gradient(135deg, #52c41a, #73d13d); color: white; }
    #btnApproveProcessing { background: linear-gradient(135deg, #1890ff, #40a9ff); color: white; }
    #btnReject { background: linear-gradient(135deg, #ff4d4f, #ff7875); color: white; }

    .action-btn:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      filter: brightness(1.1);
    }

    /* 审核备注样式 */
    .audit-note {
      width: 100%;
      height: 120px;
      resize: none;
      padding: 16px;
      border: 1px solid #e6f7ff;
      border-radius: 8px;
      font-size: 14px;
      line-height: 1.6;
      background: #fff;
      transition: all 0.3s ease;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    }

    /* 状态徽章 */
    .status-badge { display:inline-block; padding:6px 10px; border-radius:999px; font-size:12px; font-weight:700; }
    .status-pending { background:#fff7e6; color:#d46b08; border:1px solid #ffd591; }
    .status-reject { background:#fff1f0; color:#cf1322; border:1px solid #ffa39e; }
    .status-processing { background:#e6f7ff; color:#096dd9; border:1px solid #91d5ff; }
    .status-public { background:#f6ffed; color:#389e0d; border:1px solid #b7eb8f; }
    .status-more { background:#fffbe6; color:#ad6800; border:1px solid #ffe58f; }

    .audit-note:focus {
      outline: none;
      border-color: #1890ff;
      box-shadow: 0 0 0 3px rgba(24, 144, 255, 0.1);
      background: #fafcff;
    }

    /* 空数据提示样式 */
    .empty-data {
      text-align: center;
      padding: 40px;
      color: #999;
      font-size: 15px;
      background: #fafbfc;
    }

    /* 响应式调整 */
    @media (max-width: 768px) {
      .audit-grid { font-size: 14px; }
      .audit-grid td, .audit-grid th { padding: 8px 12px; }
      .col-ops { width: 200px; }
      .action-btn { min-width: 60px; padding: 6px 8px; font-size: 11px; margin: 1px 2px 1px 0; }
    }
  </style>

  <div class="page-wrapper">
    <div class="card">
      <h2>事件管理</h2>
      
      <asp:Label ID="lblMessage" runat="server" />

      <asp:Panel ID="pnlAuditContent" runat="server">
        <div class="toolbar">
          <asp:Button ID="btnRefresh" runat="server" CssClass="action-btn" Text="刷新列表" OnClick="btnRefresh_Click" />
        </div>

        <asp:GridView ID="gvPending" runat="server" CssClass="audit-grid" AutoGenerateColumns="false" DataKeyNames="Incident_id" AllowPaging="true" PageSize="5" GridLines="None" OnRowCommand="gvPending_RowCommand"
          PagerSettings-Mode="NumericFirstLast" PagerSettings-Position="TopAndBottom" PagerSettings-PageButtonCount="5"
          PagerSettings-FirstPageText="首页" PagerSettings-LastPageText="尾页" PagerSettings-NextPageText="下一页" PagerSettings-PreviousPageText="上一页"

          OnPageIndexChanging="gvPending_PageIndexChanging" OnRowDataBound="gvPending_RowDataBound" EmptyDataText="<tr><td colspan='8' class='empty-data'>暂无待审核/已上报/已驳回事件</td></tr>">
          <Columns>
            <asp:TemplateField HeaderText="序号" HeaderStyle-CssClass="col-serial" ItemStyle-CssClass="col-serial">
              <ItemTemplate>
                <asp:Label ID="lblSerial" runat="server" />
              </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Incident_id" HeaderText="事件ID" HeaderStyle-CssClass="col-id" ItemStyle-CssClass="col-id" />
            <asp:BoundField DataField="Incident_type" HeaderText="类型" HeaderStyle-CssClass="col-type" ItemStyle-CssClass="col-type" />
            <asp:BoundField DataField="Occur_time" HeaderText="发生时间" DataFormatString="{0:yyyy-MM-dd HH:mm}" HeaderStyle-CssClass="col-time" ItemStyle-CssClass="col-time" />
            <asp:BoundField DataField="Report_time" HeaderText="上报时间" DataFormatString="{0:yyyy-MM-dd HH:mm}" HeaderStyle-CssClass="col-time" ItemStyle-CssClass="col-time" />
            <asp:BoundField DataField="Location" HeaderText="地点" HeaderStyle-CssClass="col-loc" ItemStyle-CssClass="col-loc" />
            <asp:TemplateField HeaderText="描述" HeaderStyle-CssClass="col-desc" ItemStyle-CssClass="col-desc">
              <ItemTemplate>
                <div class="desc-wrap clamp" title='<%# Eval("Description") %>'><%# Eval("Description") %></div>
              </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="管理操作" HeaderStyle-CssClass="col-ops" ItemStyle-CssClass="col-ops">
              <ItemTemplate>
                <asp:Button ID="btnUnpublish" runat="server" CssClass="action-btn" Text="下架-取消公开" CommandName="Unpublish" CommandArgument='<%# Eval("Incident_id") %>' />
              </ItemTemplate>
            </asp:TemplateField>
          </Columns>
        </asp:GridView>

        <h3>审核备注</h3>
        <asp:TextBox ID="txtReason" runat="server" TextMode="MultiLine" CssClass="audit-note" placeholder="请输入审核备注信息（驳回时必填）..." />
      </asp:Panel>
    </div>
  </div>

  <script type="text/javascript"></script>
</asp:Content>
