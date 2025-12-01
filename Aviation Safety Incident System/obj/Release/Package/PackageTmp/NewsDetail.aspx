<%@ Page Title="新闻详情" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="NewsDetail.aspx.cs" Inherits="WebForms.NewsDetail" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <h2>新闻详情</h2>
  <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />

  <h3><asp:Label ID="lblTitle" runat="server" /></h3>
  <p><asp:Label ID="lblMeta" runat="server" /></p>
  <div>
    <asp:Literal ID="litContent" runat="server" Mode="PassThrough" />
  </div>

  <p>
    <asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="~/NewsPublished.aspx" Text="返回已发布新闻" />
  </p>
</asp:Content>
