<%@ Page Title="登录" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="WebForms.Login" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <style>
    /* 页面背景与整体布局 - 增强版 */
    body { 
      background: linear-gradient(135deg, #f0f7ff 0%, #e6f2ff 50%, #ffffff 100%);
      min-height: 100vh;
    }
    .login-wrap { 
      display: flex; 
      align-items: center; 
      justify-content: center; 
      min-height: 70vh; 
      padding: 24px; 
    }

    /* 顶部品牌横幅 - 增强版 */
    .login-hero { 
      max-width: 820px; 
      margin: 18px auto 0; 
      background: linear-gradient(120deg, #e6f7ff 0%, #f0fdff 100%); 
      border: 1px solid #bae7ff; 
      box-shadow: 0 12px 40px rgba(24,144,255,0.15); 
      border-radius: 16px; 
      padding: 24px 28px; 
      position: relative;
      overflow: hidden;
    }
    .login-hero:before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      width: 6px;
      height: 100%;
      background: linear-gradient(to bottom, #1890ff, #69c0ff);
    }
    .login-hero .title { 
      font-size: 24px; 
      font-weight: 700; 
      color: #0c5aa6; 
      margin-bottom: 8px;
    }
    .login-hero .desc { 
      color: #4a4a4a; 
      font-size: 15px; 
      line-height: 1.7; 
    }

    /* 登录卡片 - 增强版 */
    .login-card { 
      width: 440px; 
      background: #fff; 
      border: 1px solid #e8f4ff; 
      border-radius: 16px; 
      box-shadow: 0 16px 40px rgba(0,0,0,0.1); 
      padding: 32px 36px;
      position: relative;
      overflow: hidden;
      transition: transform 0.3s ease, box-shadow 0.3s ease;
    }
    .login-card:hover {
      transform: translateY(-5px);
      box-shadow: 0 20px 50px rgba(0,0,0,0.15);
    }
    .login-card:before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 4px;
      background: linear-gradient(to right, #1890ff, #36cfc9);
    }
    .login-title { 
      margin: 0 0 8px; 
      font-size: 28px; 
      font-weight: 700; 
      color: #1890ff;
    }
    .login-sub { 
      margin: 0 0 24px; 
      color: #666; 
      font-size: 15px; 
      line-height: 1.6;
    }

    /* 表单控件 - 增强版 */
    .form-row { 
      margin-bottom: 20px; 
      position: relative;
    }
    .form-label { 
      display: block; 
      margin-bottom: 8px; 
      color: #333; 
      font-weight: 600; 
      font-size: 14px;
    }
    .form-input { 
      width: 100%; 
      padding: 12px 16px; 
      border: 1px solid #d9d9d9; 
      border-radius: 10px; 
      font-size: 15px; 
      transition: all 0.3s ease; 
      background-color: #fafafa;
    }
    .form-input:focus { 
      outline: none; 
      border-color: #40a9ff; 
      box-shadow: 0 0 0 3px rgba(24,144,255,0.18); 
      background-color: #fff;
    }
    .error-tip { 
      color: #ff4d4f; 
      font-size: 13px; 
      margin-top: 6px; 
      display: block; 
      padding-left: 4px;
    }

    /* 操作区与按钮 - 增强版 */
    .login-actions { 
      margin-top: 24px; 
      display: flex; 
      align-items: center; 
      justify-content: space-between; 
    }
    .btn-primary { 
      background: linear-gradient(135deg, #1890ff, #36cfc9); 
      border: none; 
      color: #fff; 
      padding: 12px 28px; 
      border-radius: 10px; 
      cursor: pointer; 
      font-size: 15px; 
      font-weight: 600;
      box-shadow: 0 8px 20px rgba(24,144,255,0.3); 
      transition: all 0.3s ease; 
      letter-spacing: 0.5px;
    }
    .btn-primary:hover { 
      background: linear-gradient(135deg, #1677ff, #13c2c2); 
      transform: translateY(-2px); 
      box-shadow: 0 12px 25px rgba(24,144,255,0.4);
    }
    .btn-primary:active {
      transform: translateY(0);
    }
    .helper { 
      color: #999; 
      font-size: 13px; 
    }

    /* 底部辅助区 - 增强版 */
    .login-foot { 
      margin-top: 20px; 
      display: flex; 
      justify-content: space-between; 
      align-items: center; 
      padding-top: 16px;
      border-top: 1px solid #f0f0f0;
    }
    .link { 
      color: #1890ff; 
      text-decoration: none; 
      font-weight: 500;
      transition: all 0.2s ease;
      position: relative;
    }
    .link:after {
      content: '';
      position: absolute;
      bottom: -2px;
      left: 0;
      width: 0;
      height: 2px;
      background: #1890ff;
      transition: width 0.3s ease;
    }
    .link:hover { 
      color: #096dd9; 
      text-decoration: none;
    }
    .link:hover:after {
      width: 100%;
    }
    
    /* 消息标签样式 */
    #MainContent_lblMessage {
      display: block;
      padding: 12px 16px;
      margin-bottom: 20px;
      border-radius: 8px;
      font-size: 14px;
      text-align: center;
    }
    
    /* 响应式调整 */
    @media (max-width: 480px) {
      .login-card {
        width: 100%;
        padding: 24px 20px;
      }
      .login-actions {
        flex-direction: column;
        gap: 16px;
        align-items: flex-start;
      }
    }
  </style>

  <%-- 保留登录功能：输入用户名/密码进行登录 --%>
  <%-- 小白讲解：把登录板块设为可见（Visible=true），首页直接展示登录表单 --%>
  <asp:Panel ID="panelLogin" runat="server" Visible="true">
    <div class="login-wrap">
      <div class="login-card">
        <h2 class="login-title">登录</h2>
        <p class="login-sub">请输入用户名与密码以进入系统</p>
        <asp:ScriptManager ID="smMain" runat="server" />

      <%-- 系统提示 --%>
      <asp:Label ID="lblMessage" runat="server" />

      <div class="form-row">
        <asp:Label runat="server" AssociatedControlID="txtUserName" CssClass="form-label" Text="用户名" />
        <%-- 小白讲解：关闭浏览器自动填充，防止退出后恢复旧值 --%>
        <asp:TextBox ID="txtUserName" runat="server" CssClass="form-input" autocomplete="off" Placeholder="请输入用户名" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtUserName" CssClass="error-tip" ErrorMessage="用户名不能为空" Display="Dynamic" />
      </div>

      <div class="form-row">
        <asp:Label runat="server" AssociatedControlID="txtPassword" CssClass="form-label" Text="密码" />
        <%-- 小白讲解：密码框用 new-password 进一步禁止自动填充 --%>
        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-input" TextMode="Password" autocomplete="new-password" Placeholder="请输入密码" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword" CssClass="error-tip" ErrorMessage="密码不能为空" Display="Dynamic" />
      </div>

        <div class="login-actions">
          <asp:Button ID="btnLogin" runat="server" Text="登录" OnClick="btnLogin_Click" CssClass="btn-primary" />
          <span class="helper">忘记密码请联系管理员重置</span>
        </div>

        <div class="login-foot">
          <span class="helper">没有账号？</span>
          <asp:HyperLink ID="lnkGoRegister" runat="server" NavigateUrl="~/Register.aspx" Text="去注册" CssClass="link" />
        </div>
      </div>
    </div>
  </asp:Panel>
  
</asp:Content>
