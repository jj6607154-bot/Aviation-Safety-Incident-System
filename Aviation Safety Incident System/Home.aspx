<%@ Page Title="首页重定向" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Home.aspx.cs" Inherits="WebForms.Home" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <%-- 小白讲解：这个页面只是做兼容。有人访问 /Home.aspx 时，我们立刻把他重定向到真正的首页 Default.aspx。 --%>
  <asp:Label ID="lblRedirect" runat="server" Text="正在跳转到首页，请稍候…" />
</asp:Content>
