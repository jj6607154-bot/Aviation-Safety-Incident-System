<%@ Page Title="事件上报" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="IncidentReport.aspx.cs" Inherits="Aviation_Safety_Incident_System.IncidentReport" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
  <style>
    .page { max-width: 1000px; margin: 0 auto; }
    .card { background: #fff; border: 1px solid #e6e6e6; border-radius: 12px; box-shadow: 0 8px 24px rgba(0,0,0,.08); margin-bottom: 18px; }
    .card-header { padding: 12px 16px; border-bottom: 1px solid #eee; display: flex; align-items: center; justify-content: space-between; }
    .card-body { padding: 16px; }
    .row { display: flex; gap: 16px; flex-wrap: wrap; }
    .col { flex: 1 1 300px; }
    .label { display: block; margin-bottom: 6px; color: #555; font-weight: 600; }
    .input, .select, .textarea { width: 100%; padding: 10px 12px; border: 1px solid #d9d9d9; border-radius: 8px; }
    .textarea { min-height: 140px; height: 160px; resize: none; overflow: auto; }
    .tip { font-size: 12px; color: #888; margin-top: 6px; }
    .actions { display: flex; justify-content: flex-end; gap: 10px; padding: 16px; }
    .btn { background: #1890ff; color: #fff; border: none; padding: 10px 16px; border-radius: 8px; cursor: pointer; }
    .btn.secondary { background: #aaa; }
    .msg { display:block; margin-bottom:10px; padding:10px; border-radius:8px; }
  </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
  <div class="page">
    <h2 style="color:#0c5aa6;">事件上报</h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="msg" />

    <div class="card">
      <div class="card-header">
        <strong>基本信息</strong>
      </div>
      <div class="card-body">
        <div class="row">
          <div class="col">
            <label class="label">事件类型*</label>
            <asp:DropDownList ID="ddlType" runat="server" CssClass="select">
              <%-- 选项在后端统一绑定为：事故、事故征候、一般事件 --%>
            </asp:DropDownList>
          </div>
          <div class="col">
            <label class="label">发生日期*</label>
            <asp:TextBox ID="txtOccurTime" runat="server" CssClass="input" TextMode="Date" />
            <div class="tip">请选择事件发生的日期（无需时分）</div>
          </div>
        </div>

        <div class="row" style="margin-top:8px;">
          <div class="col">
            <label class="label">地点*</label>
            <asp:DropDownList ID="ddlLocation" runat="server" CssClass="select">
              <%-- 选项在后端统一绑定为：跑道、滑行道、登机口、候机楼、机库、航站楼 --%>
            </asp:DropDownList>
          </div>
        </div>
      </div>
    </div>

    <div class="card">
      <div class="card-header">
        <strong>详细描述</strong>
      </div>
      <div class="card-body">
        <label class="label">详细描述（不少于20字）*</label>
        <asp:TextBox ID="txtDescription" runat="server" CssClass="textarea" TextMode="MultiLine" />
        <div class="tip">请详细描述事件经过、原因和影响。</div>
      </div>
    </div>

    <div class="actions">
      <asp:Button ID="btnCancel" runat="server" Text="取消" CssClass="btn secondary" PostBackUrl="~/IncidentQuery.aspx" />
      <asp:Button ID="btnSubmit" runat="server" Text="提交上报" CssClass="btn" OnClick="btnSubmit_Click" />
    </div>
  </div>
</asp:Content>
