<%@ Page Title="新闻动态" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="NewsManage.aspx.cs"
  Inherits="WebForms.NewsManage" %>
  <asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
      .page { max-width:1000px; margin:0 auto; }
      .title { font-size:24px; font-weight:700; color:#0c5aa6; margin:0 0 10px; }
      .msg { display:block; margin-bottom:10px; padding:10px; border-radius:8px; }
      .card { background:#fff; border:1px solid #e6e6e6; border-radius:12px; box-shadow:0 8px 24px rgba(0,0,0,.08); padding:18px; margin-bottom:16px; }
      .form-row { margin-bottom:10px; }
      .input { width:600px; padding:10px 12px; border:1px solid #d9d9d9; border-radius:8px; }
      .btn { background:#1890ff; color:#fff; border:none; padding:10px 16px; border-radius:8px; cursor:pointer; }
      .btn:hover { background:#1677ff; }
      .grid { width:100%; border-collapse:collapse; background:#fff; border:1px solid #eee; border-radius:12px; overflow:hidden; box-shadow:0 6px 18px rgba(0,0,0,.06); }
      .grid th { background:#f8f9fa; padding:12px; text-align:left; border-bottom:1px solid #eee; }
      .grid td { padding:12px; border-bottom:1px solid #f5f5f5; }
      .link { color:#1890ff; text-decoration:none; }
      .link:hover { color:#096dd9; text-decoration:underline; }
      .pager { text-align:center; padding:12px; }
      .pager a, .pager span { margin:0 4px; padding:6px 10px; border:1px solid #d6dae3; border-radius:6px; text-decoration:none; color:#333; }
      .pager a:hover { background:#dfe8ff; }
    </style>
    <div class="page">
      <h2 class="title">新闻动态</h2>
      <asp:Label ID="lblMessage" runat="server" CssClass="msg" />
      <asp:Panel ID="panelPublish" runat="server" Visible="false" CssClass="card">
        <h3>发布新闻</h3>
        <div class="form-row">
          <asp:Label runat="server" AssociatedControlID="txtTitle" Text="标题" />
          <asp:TextBox ID="txtTitle" runat="server" CssClass="input" />
        </div>
        <div class="form-row">
          <asp:Label runat="server" AssociatedControlID="txtContent" Text="内容" />
          <asp:TextBox ID="txtContent" runat="server" TextMode="MultiLine" Rows="6" CssClass="input" />
        </div>
        
        <div class="form-row">
          <asp:CheckBox ID="chkAutoPublish" runat="server" Checked="true" Text="创建后自动发布" />
          <asp:Button ID="btnCreate" runat="server" Text="提交" OnClick="btnCreate_Click" CssClass="btn" />
        </div>
      </asp:Panel>

      <asp:HyperLink ID="lnkGoPublished" runat="server" CssClass="link" NavigateUrl="~/NewsPublished.aspx" Text="前往已发布新闻" />
    </div>
  </asp:Content>
