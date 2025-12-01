<%@ Page Title="事件管理" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="IncidentInfoManage.aspx.cs" Inherits="WebForms.IncidentInfoManage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <style>
    /* 页面整体样式 */
    body { 
      background: linear-gradient(135deg, #f8fbff 0%, #f0f7ff 100%);
      font-family: "微软雅黑", "Segoe UI", Arial, sans-serif;
    }

    /* 主容器样式 */
    .inc-page { 
      background: #fff; 
      border: 1px solid #e8f4ff; 
      border-radius: 16px; 
      box-shadow: 0 12px 40px rgba(0,0,0,0.08); 
      padding: 24px 28px; 
      margin: 0 auto;
      position: relative;
      overflow: hidden;
    }

    .inc-page:before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 4px;
      background: linear-gradient(to right, #1890ff, #36cfc9);
    }

    /* 页面标题样式 */
    .page-title { 
      margin: 0 0 20px; 
      font-size: 28px; 
      color: #0c5aa6; 
      font-weight: 800;
      padding-bottom: 12px;
      border-bottom: 2px solid #e6f7ff;
    }

    /* 消息提示样式 */
    #lblMessage {
      display: block;
      padding: 12px 16px;
      margin-bottom: 20px;
      border-radius: 8px;
      font-size: 14px;
      font-weight: 500;
      border-left: 4px solid #1890ff;
      background: linear-gradient(135deg, #e6f7ff, #f0fdff);
      box-shadow: 0 2px 8px rgba(24, 144, 255, 0.1);
    }

    /* 筛选栏样式 */
    .filter-bar { 
      display: flex; 
      flex-wrap: wrap; 
      gap: 16px; 
      align-items: center; 
      margin: 16px 0 24px; 
      padding: 20px;
      background: linear-gradient(135deg, #f8fbff, #e6f7ff);
      border-radius: 12px;
      border: 1px solid #e6f7ff;
    }

    .filter-bar label { 
      color: #0c5aa6; 
      font-weight: 600;
      font-size: 14px;
      min-width: 60px;
    }

    .filter-bar input, .filter-bar select { 
      padding: 10px 12px; 
      border: 1px solid #e6f7ff; 
      border-radius: 8px; 
      font-size: 14px;
      background: #fff;
      transition: all 0.3s ease;
      box-shadow: 0 2px 6px rgba(0,0,0,0.04);
    }

    .filter-bar input:focus, .filter-bar select:focus { 
      outline: none; 
      border-color: #1890ff; 
      box-shadow: 0 0 0 3px rgba(24,144,255,0.15); 
      transform: translateY(-1px);
    }

    .btn-search { 
      background: linear-gradient(135deg, #1890ff, #36cfc9);
      color: #fff; 
      border: none; 
      padding: 10px 20px; 
      border-radius: 8px; 
      cursor: pointer; 
      font-weight: 600;
      font-size: 14px;
      transition: all 0.3s ease;
      box-shadow: 0 6px 16px rgba(24, 144, 255, 0.25);
    }

    .btn-search:hover { 
      background: linear-gradient(135deg, #1677ff, #13c2c2);
      transform: translateY(-2px);
      box-shadow: 0 8px 20px rgba(24, 144, 255, 0.35);
    }

    /* 表格样式 */
    .manage-grid { 
      table-layout: fixed; 
      width: 100%; 
      border-collapse: collapse; 
      background: #fff; 
      border: 1px solid #e8f4ff; 
      border-radius: 12px; 
      overflow: hidden; 
      box-shadow: 0 8px 24px rgba(0,0,0,0.08);
      margin-bottom: 20px;
    }

    .manage-grid td, .manage-grid th { 
      padding: 14px 16px; 
      line-height: 1.6; 
      vertical-align: middle; 
      transition: all 0.3s ease;
    }

    .manage-grid .grid-header th { 
      position: sticky; 
      top: 0; 
      background: linear-gradient(135deg, #f8fbff, #e6f7ff); 
      color: #0c5aa6; 
      font-weight: 700; 
      font-size: 15px;
      border-bottom: 2px solid #e6f7ff; 
      z-index: 2; 
      padding: 16px;
    }

    .manage-grid .row td { 
      border-bottom: 1px solid #f0f8ff; 
    }

    .manage-grid .row-alt td { 
      background: #fafcff; 
      border-bottom: 1px solid #f0f8ff; 
    }

    .manage-grid .row:hover td { 
      background: #eef6ff; 
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(24, 144, 255, 0.1);
    }

    /* 分页器样式 */
    .manage-grid .grid-pager { 
      padding: 16px; 
      text-align: center; 
      background: linear-gradient(135deg, #f8fbff, #e6f7ff);
      border-top: 1px solid #e6f7ff;
    }

    .manage-grid .grid-pager a, .manage-grid .grid-pager span { 
      margin: 0 4px; 
      padding: 8px 12px; 
      border: 1px solid #e6f7ff; 
      border-radius: 6px; 
      text-decoration: none; 
      color: #1890ff; 
      font-weight: 500;
      transition: all 0.3s ease;
    }

    .manage-grid .grid-pager a:hover { 
      background: linear-gradient(135deg, #1890ff, #36cfc9);
      color: #fff; 
      border-color: #1890ff;
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(24, 144, 255, 0.25);
    }

    .manage-grid .grid-pager span { 
      background: linear-gradient(135deg, #1890ff, #36cfc9);
      color: #fff; 
      border-color: #1890ff;
    }

    /* 列宽设置 */
    .col-serial { width: 80px; text-align: center; }
    .col-id { width: 100px; text-align: center; }
    .col-type { width: 130px; }
    .col-time { width: 150px; }
    .col-loc { width: 140px; }
    .col-status { width: 110px; }
    .col-desc { width: auto; }

    /* 描述文本样式 */
    .desc-wrap { 
      white-space: normal; 
      word-break: break-word; 
      line-height: 1.6;
      color: #555;
    }

    .desc-wrap.clamp { 
      display: -webkit-box; 
      -webkit-line-clamp: 3; 
      -webkit-box-orient: vertical; 
      overflow: hidden; 
    }

    .toggle-desc { 
      display: inline-block; 
      margin-top: 8px; 
      color: #1890ff; 
      text-decoration: none; 
      font-size: 12px;
      font-weight: 500;
      padding: 4px 8px;
      border-radius: 4px;
      background: #f0f8ff;
      transition: all 0.3s ease;
    }

    .toggle-desc:hover { 
      color: #096dd9; 
      background: #e6f7ff;
      text-decoration: none;
      transform: translateY(-1px);
    }

    /* 操作按钮样式 */
    .manage-grid a {
      color: #1890ff;
      text-decoration: none;
      padding: 6px 10px;
      border-radius: 4px;
      margin: 0 2px;
      transition: all 0.3s ease;
      font-weight: 500;
    }

    .manage-grid a:hover { 
      background: linear-gradient(135deg, #1890ff, #36cfc9);
      color: #fff; 
      text-decoration: none; 
      transform: translateY(-1px);
    }

    /* 操作列堆叠布局 */
    .ops-stack { display:flex; flex-direction:column; gap:8px; align-items:flex-start; }
    .col-ops { width: 110px; }

    /* 顶部导航 */
    .top-nav { display:flex; gap:10px; align-items:center; margin:10px 0 18px; }
    .top-nav a { display:inline-block; padding:8px 14px; border-radius:999px; border:1px solid #e6f7ff; background:#f8fbff; color:#0c5aa6; text-decoration:none; font-weight:600; }
    .top-nav a:hover { background:#e6f7ff; color:#096dd9; }

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
      .inc-page {
        padding: 16px 20px;
      }
      
      .filter-bar {
        flex-direction: column;
        align-items: stretch;
        gap: 12px;
      }
      
      .filter-bar label {
        min-width: auto;
      }
      
      .manage-grid {
        font-size: 14px;
      }
      
      .manage-grid td, .manage-grid th {
        padding: 10px 12px;
      }
    }
  </style>

  <div class="inc-page">
    <h2 class="page-title">事件管理</h2>
    <div class="top-nav">
      <asp:HyperLink ID="navHome" runat="server" NavigateUrl="~/Default.aspx" Text="首页" />
      <asp:HyperLink ID="navQuery" runat="server" NavigateUrl="~/IncidentQuery.aspx" Text="事件查询" />
      <asp:HyperLink ID="navAudit" runat="server" NavigateUrl="~/IncidentAudit.aspx" Text="事件审核" />
      <asp:HyperLink ID="navUserManage" runat="server" NavigateUrl="~/UserManage.aspx" Text="用户管理" />
    </div>
    <asp:Label ID="lblMessage" runat="server" />
    
    <%-- 小白讲解：这里是查询条件。为了"把空格删掉"，我给这个区域加上 filter-bar 类，统一收紧边距 --%>
    <div class="filter-bar">
      <asp:Label runat="server" AssociatedControlID="ddlSearchType" Text="按类型" />
      <asp:DropDownList ID="ddlSearchType" runat="server">
        <%-- 小白讲解：选项由后台统一绑定（DropdownOptions），包含"全部" --%>
      </asp:DropDownList>
      
      <%-- 地点包含（小白讲解）：支持输入关键字进行模糊查询，例如"虹桥"或"航站楼" --%>
      <asp:Label runat="server" AssociatedControlID="txtSearchLocation" Text="地点包含" />
      <asp:TextBox ID="txtSearchLocation" runat="server" Width="200" placeholder="输入地点关键词..." />
      
      <asp:Label runat="server" AssociatedControlID="txtSearchStart" Text="开始日期" />
      <asp:TextBox ID="txtSearchStart" runat="server" TextMode="Date" />
      
      <asp:Label runat="server" AssociatedControlID="txtSearchEnd" Text="结束日期" />
      <asp:TextBox ID="txtSearchEnd" runat="server" TextMode="Date" />
      
      <asp:Button ID="btnSearch" runat="server" Text="查询" OnClick="btnSearch_Click" CssClass="btn-search" />
    </div>

    <asp:GridView ID="gvIncident" runat="server" AutoGenerateColumns="false" EmptyDataText="<tr><td colspan='8' class='empty-data'>暂无事件数据</td></tr>"
        DataKeyNames="Incident_id,User_id" CssClass="manage-grid" AllowPaging="true" PageSize="5" GridLines="None"
        PagerSettings-Mode="NumericFirstLast" PagerSettings-Position="TopAndBottom" PagerSettings-PageButtonCount="5"
        PagerSettings-FirstPageText="首页" PagerSettings-LastPageText="尾页" PagerSettings-NextPageText="下一页" PagerSettings-PreviousPageText="上一页"
        HeaderStyle-CssClass="grid-header" RowStyle-CssClass="row" AlternatingRowStyle-CssClass="row-alt" PagerStyle-CssClass="grid-pager"
        OnPageIndexChanging="gvIncident_PageIndexChanging"
        OnRowEditing="gvIncident_RowEditing" OnRowCancelingEdit="gvIncident_RowCancelingEdit" OnRowUpdating="gvIncident_RowUpdating" OnRowDeleting="gvIncident_RowDeleting"
        OnRowDataBound="gvIncident_RowDataBound">
      <Columns>
        <%-- 小白讲解：新增"编号(序号)"列，分页时也能连续编号 --%>
        <asp:TemplateField HeaderText="序号" HeaderStyle-CssClass="col-serial" ItemStyle-CssClass="col-serial">
          <ItemTemplate>
            <asp:Label ID="lblSerial" runat="server" />
          </ItemTemplate>
        </asp:TemplateField>
        
        <%-- 小白讲解：保留真实事件ID，便于定位具体记录 --%>
        <asp:BoundField DataField="Incident_id" HeaderText="事件ID" HeaderStyle-CssClass="col-id" ItemStyle-CssClass="col-id" />
        
        <asp:TemplateField HeaderText="类型" HeaderStyle-CssClass="col-type" ItemStyle-CssClass="col-type">
          <ItemTemplate>
            <%# Eval("Incident_type") %>
          </ItemTemplate>
          <EditItemTemplate>
            <asp:DropDownList ID="ddlEditType" runat="server" CssClass="form-input"></asp:DropDownList>
          </EditItemTemplate>
        </asp:TemplateField>
        
        <asp:BoundField DataField="Occur_time" HeaderText="发生时间" DataFormatString="{0:yyyy-MM-dd}" ReadOnly="true" HeaderStyle-CssClass="col-time" ItemStyle-CssClass="col-time" />
        
        <asp:BoundField DataField="Location" HeaderText="地点" ReadOnly="true" HeaderStyle-CssClass="col-loc" ItemStyle-CssClass="col-loc" />
        
        <asp:TemplateField HeaderText="状态" HeaderStyle-CssClass="col-status" ItemStyle-CssClass="col-status">
          <ItemTemplate>
            <%# Eval("Incident_status") %>
          </ItemTemplate>
          <EditItemTemplate>
            <asp:DropDownList ID="ddlEditStatus" runat="server" CssClass="form-input"></asp:DropDownList>
          </EditItemTemplate>
        </asp:TemplateField>
        
        <asp:TemplateField HeaderText="事件描述" HeaderStyle-CssClass="col-desc" ItemStyle-CssClass="col-desc">
          <ItemTemplate>
            <div class="desc-wrap clamp" data-expanded="0"><%# Eval("Description") %></div>
            <asp:HyperLink ID="lnkFull" runat="server" CssClass="toggle-desc" Text="展开全部"
              NavigateUrl='<%# "~/MyMessageDetail.aspx?id=" + Eval("Incident_id") %>' />
          </ItemTemplate>
          <EditItemTemplate>
            <asp:TextBox ID="txtEditDesc" runat="server" Text='<%# Bind("Description") %>' TextMode="MultiLine" Rows="4" CssClass="form-input" />
          </EditItemTemplate>
        </asp:TemplateField>
        
        <asp:TemplateField HeaderText="操作" HeaderStyle-CssClass="col-ops" ItemStyle-CssClass="col-ops">
          <ItemTemplate>
            <div class="ops-stack">
              <asp:LinkButton ID="lnkEdit" runat="server" CommandName="Edit" Text="编辑" CausesValidation="false" />
              <asp:LinkButton ID="lnkDelete" runat="server" CommandName="Delete" Text="删除" CausesValidation="false" />
            </div>
          </ItemTemplate>
          <EditItemTemplate>
            <div class="ops-stack">
              <asp:LinkButton ID="lnkUpdate" runat="server" CommandName="Update" Text="更新" />
              <asp:LinkButton ID="lnkCancel" runat="server" CommandName="Cancel" Text="取消" />
            </div>
          </EditItemTemplate>
        </asp:TemplateField>
      </Columns>
    </asp:GridView>
    
    
  </div>
</asp:Content>
