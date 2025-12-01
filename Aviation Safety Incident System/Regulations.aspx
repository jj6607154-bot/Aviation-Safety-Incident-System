<%@ Page Title="安全法律法规" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Regulations.aspx.cs" Inherits="WebForms.Regulations" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <style>
    .regs-page { max-width: 1000px; margin: 0 auto; }
    .regs-title { font-size: 24px; color: #0c5aa6; margin: 0 0 12px; font-weight: 700; }
    .regs-sub { color: #666; margin-bottom: 14px; }
    .regs-search { display: flex; gap: 10px; margin-bottom: 14px; }
    .regs-input { flex: 1; padding: 10px 12px; border: 1px solid #d9d9d9; border-radius: 8px; font-size: 14px; }
    .regs-input:focus { outline: none; border-color: #40a9ff; box-shadow: 0 0 0 3px rgba(24,144,255,0.18); }
    .regs-btn { background: #1890ff; color: #fff; border: none; padding: 10px 16px; border-radius: 8px; cursor: pointer; font-size: 14px; }
    .regs-btn:hover { background: #1677ff; }
    .msg { display:block; margin-bottom:10px; padding:10px; border-radius:6px; }
    .grid-view { border-collapse: collapse; width: 100%; background:#fff; box-shadow: 0 6px 18px rgba(0,0,0,0.06); }
    .grid-view th { background:#f8f9fa; border:1px solid #eee; padding:12px; text-align:left; color:#333; font-weight:600; }
    .grid-view td { border:1px solid #eee; padding:12px; color:#555; }
    .grid-view tr:hover { background:#f5fafe; }
    .open-link { color:#1890ff; text-decoration:none; }
    .open-link:hover { color:#096dd9; text-decoration:underline; }
  </style>

  <div class="regs-page">
    <h2 class="regs-title">安全法律法规在线查阅</h2>
    <div class="regs-sub">
      <asp:Label ID="lblHint" runat="server" Text="输入关键词（例如：安全生产法、民航）然后点击查询" />
    </div>

    <div class="regs-search">
      <asp:TextBox ID="txtQuery" runat="server" CssClass="regs-input" />
      <asp:Button ID="btnSearch" runat="server" Text="查询" OnClick="btnSearch_Click" CssClass="regs-btn" />
    </div>

    <asp:Label ID="lblMessage" runat="server" CssClass="msg" />

    <asp:GridView ID="gvRegs" runat="server" CssClass="grid-view" AutoGenerateColumns="false" EmptyDataText="未找到匹配的法规，请更换关键词试试">
      <Columns>
        <asp:BoundField DataField="Title" HeaderText="标题" />
        <asp:BoundField DataField="Source" HeaderText="来源" />
        <asp:TemplateField HeaderText="在线阅读">
          <ItemTemplate>
            <asp:HyperLink ID="lnkOpen" runat="server" Text="打开原文" Target="_blank" CssClass="open-link"
              NavigateUrl='<%# Eval("Url") %>' />
          </ItemTemplate>
        </asp:TemplateField>
      </Columns>
    </asp:GridView>
  </div>
</asp:Content>
