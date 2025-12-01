<%@ Page Title="用户管理" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="UserManage.aspx.cs" Inherits="WebForms.UserManage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <h2>用户管理（删除用户与配置权限）</h2>
  <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />

  <!-- 新增用户模块已移除：仅保留“删除用户”和“配置权限”两种功能 -->

  <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="false" DataKeyNames="User_id"
      OnRowDataBound="gvUsers_RowDataBound" OnRowCommand="gvUsers_RowCommand">
    <Columns>
      <%-- 序号（小白讲解）：这里只显示表格的顺序编号，从 1 开始，方便阅读；真正的数据库主键 User_id 仍在数据中用于操作（比如删除/重置），只是不直接显示。 --%>
      <asp:TemplateField HeaderText="序号">
        <ItemTemplate>
          <%# Container.DataItemIndex + 1 %>
        </ItemTemplate>
      </asp:TemplateField>
      <asp:BoundField DataField="User_name" HeaderText="用户名" />
      <asp:TemplateField HeaderText="角色">
        <ItemTemplate>
          <asp:DropDownList ID="ddlRoleRow" runat="server">
            <asp:ListItem Text="管理员" Value="1" />
            <asp:ListItem Text="普通用户" Value="2" />
            <asp:ListItem Text="审核人员" Value="3" />
          </asp:DropDownList>
        </ItemTemplate>
      </asp:TemplateField>
      <asp:TemplateField HeaderText="操作">
        <ItemTemplate>
          <asp:Button ID="btnSaveRole" runat="server" Text="保存角色" CommandName="SaveRole"
            CommandArgument='<%# Eval("User_id") %>' />
          <%-- 删除用户（小白讲解）：点击后会弹出确认框；服务器端会做安全校验，避免误删最后一个管理员或当前登录账号。 --%>
          <asp:Button ID="btnDeleteUser" runat="server" Text="删除用户" CommandName="DeleteUser"
            CommandArgument='<%# Eval("User_id") %>' OnClientClick="return confirm('确定要删除该用户吗？此操作不可恢复。');" />

          <%-- 直接重置为指定密码（小白讲解）：在此输入新密码并一键重置 --%>
          <asp:TextBox ID="txtNewPwdRow" runat="server" TextMode="Password" Placeholder="输入新密码" />
          <asp:Button ID="btnSetPwd" runat="server" Text="重置为指定密码" CommandName="SetPwd"
            CommandArgument='<%# Eval("User_id") %>' />
        </ItemTemplate>
      </asp:TemplateField>
    </Columns>
  </asp:GridView>
</asp:Content>
