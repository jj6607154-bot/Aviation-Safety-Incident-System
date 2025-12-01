<%@ Page Title="注册" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Register.aspx.cs" Inherits="WebForms.Register" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <style>
    /* 美化注册页：居中卡片、简洁配色、提升可用性 */
    .auth-wrapper { min-height: 60vh; display: grid; place-items: center; padding: 24px; }
    .auth-card { width: 100%; max-width: 520px; background: #fff; border: 1px solid #e6e8eb; border-radius: 12px; box-shadow: 0 8px 24px rgba(0,0,0,0.08); padding: 28px; }
    .auth-title { margin: 0 0 8px; font-size: 22px; font-weight: 600; color: #111; }
    .auth-subtitle { margin: 0 0 16px; font-size: 13px; color: #666; }
    .message { margin-bottom: 12px; font-size: 13px; color: #d23; }
    .form-group { margin-bottom: 14px; }
    .form-label { display: block; margin-bottom: 6px; font-size: 13px; color: #333; }
    .form-input, .form-select { width: 100%; padding: 10px 12px; font-size: 14px; border: 1px solid #cfd6de; border-radius: 8px; background: #fff; transition: border-color .15s, box-shadow .15s; }
    .form-input:focus, .form-select:focus { outline: none; border-color: #2684ff; box-shadow: 0 0 0 3px rgba(38,132,255,0.18); }
    .helper { display: block; margin-top: 6px; font-size: 12px; color: #6b7280; }
    .btn-primary { display: inline-block; width: 100%; padding: 10px 14px; font-size: 15px; font-weight: 600; color: #fff; background: #1d4ed8; border: none; border-radius: 10px; cursor: pointer; transition: background .15s, transform .05s; }
    .btn-primary:hover { background: #1e40af; }
    .btn-primary:active { transform: translateY(1px); }
    .validator { display: block; margin-top: 6px; font-size: 12px; color: #d23; }
  </style>

  <div class="auth-wrapper">
    <div class="auth-card" role="form" aria-labelledby="registerTitle">
      <h2 id="registerTitle" class="auth-title">注册</h2>
      <p class="auth-subtitle">创建账号以使用系统功能</p>
      <asp:Label ID="lblMessage" runat="server" CssClass="message" />

      <asp:ValidationSummary ID="valSummary" runat="server" CssClass="validator" HeaderText="请修正以下问题：" ValidationGroup="Register" />

      <div class="form-group">
        <asp:Label runat="server" CssClass="form-label" AssociatedControlID="txtUserName" Text="用户名" />
        <asp:TextBox ID="txtUserName" runat="server" CssClass="form-input" autocomplete="username" />
        <asp:RequiredFieldValidator ID="rfvUserName" runat="server" CssClass="validator" ControlToValidate="txtUserName" ErrorMessage="用户名不能为空" ValidationGroup="Register" Display="Dynamic" />
      </div>

      <div class="form-group">
        <asp:Label runat="server" CssClass="form-label" AssociatedControlID="txtPassword" Text="密码" />
        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-input" TextMode="Password" autocomplete="new-password" />
        <asp:RequiredFieldValidator ID="rfvPassword" runat="server" CssClass="validator" ControlToValidate="txtPassword" ErrorMessage="密码不能为空" ValidationGroup="Register" Display="Dynamic" />
      </div>

      <div class="form-group">
        <%-- 已移除邮箱字段（小白讲解）：按你的需求，注册不再要求填写邮箱 --%>
      </div>

      <div class="form-group">
        <asp:Label runat="server" CssClass="form-label" AssociatedControlID="ddlRole" Text="角色" />
        <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select">
          <asp:ListItem Text="普通用户" Value="2" Selected="True" />
          <asp:ListItem Text="审核人员" Value="3" />
          <asp:ListItem Text="管理员" Value="1" />
        </asp:DropDownList>
      </div>

      <div class="form-group">
        <asp:Label runat="server" CssClass="form-label" AssociatedControlID="txtRoleKey" Text="角色口令（仅管理员/审核需要）" />
        <asp:TextBox ID="txtRoleKey" runat="server" CssClass="form-input" TextMode="Password" autocomplete="new-password" />
        <span class="helper">提示：普通用户无需填写口令。</span>
        <!-- 小白讲解：下面这个 Literal 会显示系统配置的“真实口令提示”，避免把“键名”误当作口令。 -->
        <asp:Literal ID="litRoleKeyHint" runat="server" />
      </div>

      <div class="form-group">
        <asp:Button ID="btnRegister" runat="server" CssClass="btn-primary" Text="注册" OnClick="btnRegister_Click" ValidationGroup="Register" />
      </div>
    </div>
  </div>
</asp:Content>
