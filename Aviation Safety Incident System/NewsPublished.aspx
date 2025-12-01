<%@ Page Title="已发布新闻" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="NewsPublished.aspx.cs" Inherits="WebForms.NewsPublished" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <style>
    .page { max-width:1000px; margin:0 auto; }
    .title { font-size:24px; font-weight:700; color:#0c5aa6; margin:0 0 10px; }
    .msg { display:block; margin-bottom:10px; padding:10px; border-radius:8px; }
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
    <h2 class="title">已发布新闻</h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="msg" />

    <asp:GridView ID="gvNews" runat="server" AutoGenerateColumns="false" EmptyDataText="暂无已发布新闻"
      DataKeyNames="News_id" OnRowDeleting="gvNews_RowDeleting" OnPageIndexChanging="gvNews_PageIndexChanging" OnRowDataBound="gvNews_RowDataBound" CssClass="grid" GridLines="None" AllowPaging="true" PageSize="8"
      PagerSettings-Mode="Numeric" PagerSettings-Position="Bottom" PagerSettings-PageButtonCount="5" PagerStyle-CssClass="pager">
      <Columns>
        <asp:TemplateField HeaderText="标题">
          <ItemTemplate>
            <asp:HyperLink ID="lnkTitle" runat="server" CssClass="link" Text='<%# Eval("Title") %>'
              NavigateUrl='<%# "~/NewsDetail.aspx?NewsId=" + Eval("News_id") %>' />
          </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="User_name" HeaderText="作者" />
        <asp:BoundField DataField="Publish_time" HeaderText="发布时间" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
        <asp:TemplateField HeaderText="正文">
          <ItemTemplate>
            <asp:Literal ID="litContent" runat="server" Mode="PassThrough" Text='<%# (Eval("Content") ?? "<p>无内容</p>") %>' />
          </ItemTemplate>
        </asp:TemplateField>
        <asp:CommandField ShowDeleteButton="true" DeleteText="下架" />
      </Columns>
    </asp:GridView>
  </div>
</asp:Content>
