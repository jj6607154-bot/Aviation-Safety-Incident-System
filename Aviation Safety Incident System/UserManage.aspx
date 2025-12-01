<%@ Page Title="用户管理" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="UserManage.aspx.cs" Inherits="WebForms.UserManage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <style>
    .page { max-width: 1100px; margin: 12px auto 28px; padding: 0 12px; }
    .title { color:#0c5aa6; font-size:28px; font-weight:700; margin:6px 0 16px; border-bottom:2px solid #e6f7ff; padding-bottom:10px; }
    #lblMessage { display:block; padding:12px 16px; margin-bottom:16px; border-radius:8px; font-size:14px; font-weight:500; border-left:4px solid #ddd; background:#f9f9f9; }
    .grid { width:100%; border-collapse:collapse; background:#fff; border:1px solid #eee; border-radius:16px; overflow:hidden; box-shadow:0 8px 24px rgba(0,0,0,.08); }
    .grid th { background:#f8f9fa; padding:12px; text-align:left; border-bottom:1px solid #eee; font-weight:600; color:#333; }
    .grid td { padding:12px; border-bottom:1px solid #f5f5f5; vertical-align:middle; }
    .col-serial { width:70px; text-align:center; color:#666; }
    .col-name { width:200px; }
    .col-role { width:180px; }
    .col-ops { width:480px; }
    .ddl { width:160px; padding:8px 10px; border:1px solid #d9d9d9; border-radius:8px; }
    .input { width:180px; padding:8px 10px; border:1px solid #d9d9d9; border-radius:8px; }
    .btn { background:#1890ff; color:#fff; border:none; padding:8px 14px; border-radius:8px; cursor:pointer; margin-right:8px; }
    .btn:hover { background:#1677ff; }
    .btn-danger { background:#ff4d4f; }
    .btn-danger:hover { background:#d9363e; }
    .btn-secondary { background:#595959; }
    .btn-secondary:hover { background:#434343; }
  </style>
  <div class="page">
    <h2 class="title">用户管理（删除用户与配置权限）</h2>
    <asp:Label ID="lblMessage" runat="server" />

    <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="false" DataKeyNames="User_id"
        OnRowDataBound="gvUsers_RowDataBound" OnRowCommand="gvUsers_RowCommand" CssClass="grid" GridLines="None">
      <Columns>
      <asp:TemplateField HeaderText="序号" HeaderStyle-CssClass="col-serial" ItemStyle-CssClass="col-serial">
        <ItemTemplate>
          <%# Container.DataItemIndex + 1 %>
        </ItemTemplate>
      </asp:TemplateField>
      <asp:BoundField DataField="User_name" HeaderText="用户名" HeaderStyle-CssClass="col-name" ItemStyle-CssClass="col-name" />
      <asp:TemplateField HeaderText="角色" HeaderStyle-CssClass="col-role" ItemStyle-CssClass="col-role">
        <ItemTemplate>
          <asp:DropDownList ID="ddlRoleRow" runat="server" CssClass="ddl">
            <asp:ListItem Text="管理员" Value="1" />
            <asp:ListItem Text="普通用户" Value="2" />
            <asp:ListItem Text="审核人员" Value="3" />
          </asp:DropDownList>
        </ItemTemplate>
      </asp:TemplateField>
      <asp:TemplateField HeaderText="操作" HeaderStyle-CssClass="col-ops" ItemStyle-CssClass="col-ops">
        <ItemTemplate>
          <asp:Button ID="btnSaveRole" runat="server" Text="保存角色" CommandName="SaveRole"
            CommandArgument='<%# Eval("User_id") %>' CssClass="btn" />
          <asp:Button ID="btnDeleteUser" runat="server" Text="删除用户" CommandName="DeleteUser"
            CommandArgument='<%# Eval("User_id") %>' OnClientClick="return confirm('确定要删除该用户吗？此操作不可恢复。');" CssClass="btn btn-danger" />
          <asp:TextBox ID="txtNewPwdRow" runat="server" TextMode="Password" Placeholder="输入新密码" CssClass="input" />
          <asp:Button ID="btnSetPwd" runat="server" Text="重置为指定密码" CommandName="SetPwd"
            CommandArgument='<%# Eval("User_id") %>' CssClass="btn btn-secondary" />
        </ItemTemplate>
      </asp:TemplateField>
    </Columns>
    </asp:GridView>
  </div>
</asp:Content>
