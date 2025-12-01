<%@ Page Title="我的消息" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="MyMessages.aspx.cs" Inherits="WebForms.MyMessages" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <h2>我的消息</h2>
  <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />

  <div style="margin:8px 0;">
    <asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="javascript:history.back()" Text="返回上一页" />
  </div>

  <fieldset style="margin:8px 0;">
    <legend>筛选</legend>
    状态：
    <asp:DropDownList ID="ddlStatusFilter" runat="server">
      <asp:ListItem Text="全部" Value="" />
      <asp:ListItem Text="已驳回" Value="已驳回" />
      <asp:ListItem Text="待补充" Value="待补充" />
    </asp:DropDownList>
    &nbsp;
    <asp:Button ID="btnFilter" runat="server" Text="筛选" OnClick="btnFilter_Click" />
  </fieldset>

  <%-- 小白讲解：这里展示“被驳回/待补充”的事件，以及审核人员填写的理由 --%>
  <asp:GridView ID="gvMessages" runat="server" AutoGenerateColumns="false" EmptyDataText="暂无需要处理的消息" OnRowCommand="gvMessages_RowCommand">
    <Columns>
      <asp:TemplateField HeaderText="编号">
        <ItemTemplate>
          <%# Container.DataItemIndex + 1 %>
        </ItemTemplate>
      </asp:TemplateField>
      <asp:BoundField DataField="Incident_id" HeaderText="事件ID" />
      <asp:BoundField DataField="Incident_type" HeaderText="类型" />
      <asp:BoundField DataField="Occur_time" HeaderText="发生时间" DataFormatString="{0:yyyy-MM-dd}" />
      <asp:BoundField DataField="Incident_status" HeaderText="当前状态" />
      <asp:BoundField DataField="LastAction" HeaderText="最近动作" />
      <asp:TemplateField HeaderText="审核理由">
        <ItemTemplate>
          <div style="white-space:normal;word-break:break-word;"> <%# Eval("Reason") %> </div>
        </ItemTemplate>
      </asp:TemplateField>
      <asp:HyperLinkField HeaderText="详情" Text="查看详情" DataNavigateUrlFields="Incident_id" DataNavigateUrlFormatString="~/MyMessageDetail.aspx?id={0}" />
      <asp:TemplateField HeaderText="操作">
        <ItemTemplate>
          <asp:LinkButton ID="btnDeleteMsg" runat="server" Text="删除" CommandName="DeleteMsg" CommandArgument='<%# Eval("Incident_id") %>' OnClientClick="return confirm('确定删除该事件及其审核记录？');" />
        </ItemTemplate>
      </asp:TemplateField>
    </Columns>
  </asp:GridView>

  
</asp:Content>
