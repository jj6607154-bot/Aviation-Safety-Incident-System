<%@ Page Title="消息详情" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="MyMessageDetail.aspx.cs" Inherits="WebForms.MyMessageDetail" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        /* 样式保持不变 */
        :root {
            --primary-color: #007bff;
            --secondary-color: #6c757d;
            --success-color: #28a745;
            --danger-color: #dc3545;
            --warning-color: #ffc107;
            --info-color: #17a2b8;
            --light-bg: #f8f9fa;
            --border-color: #dee2e6;
            --text-color: #343a40;
            --shadow: 0 2px 4px rgba(0,0,0,0.05);
        }

        .container { padding: 20px; max-width: 1000px; margin: 0 auto; }
        .card { background-color: #fff; border: 1px solid var(--border-color); border-radius: 8px; box-shadow: var(--shadow); margin-bottom: 25px; overflow: hidden; }
        .card-header { padding: 15px 20px; background-color: #fff; border-bottom: 1px solid var(--border-color); display: flex; justify-content: space-between; align-items: center; }
        .card-header h3 { margin: 0; font-size: 1.1rem; color: var(--text-color); font-weight: 600; }
        .card-body { padding: 25px; }
        .alert { padding: 12px 15px; margin-bottom: 15px; border: 1px solid transparent; border-radius: 4px; font-size: 0.9rem; }
        .alert-danger { color: #721c24; background-color: #f8d7da; border-color: #f5c6cb; }
        .alert-success { color: #155724; background-color: #d4edda; border-color: #c3e6cb; }
        #MainContent_lblMessage:empty { display: none; }
        .btn { display: inline-block; font-weight: 400; text-align: center; vertical-align: middle; border: 1px solid transparent; padding: 6px 12px; font-size: 0.9rem; line-height: 1.5; border-radius: 4px; transition: all 0.15s; cursor: pointer; text-decoration: none; }
        .btn-primary { color: #fff; background-color: var(--primary-color); border-color: var(--primary-color); }
        .btn-primary:hover { background-color: #0069d9; }
        .btn-secondary { color: #fff; background-color: var(--secondary-color); border-color: var(--secondary-color); }
        .btn-secondary:hover { background-color: #5a6268; border-color: #545b62; }
        .btn-sm { padding: 4px 8px; font-size: 0.8rem; }
        .badge { display: inline-block; padding: 4px 8px; font-size: 0.8rem; font-weight: 600; border-radius: 10px; color: #fff; }
        .badge-danger { background-color: var(--danger-color); }
        .badge-warning { background-color: var(--warning-color); color: #212529; }
        .badge-success { background-color: var(--success-color); }
        .badge-secondary { background-color: var(--secondary-color); }
        .badge-primary { background-color: var(--primary-color); }

        /* --- 详情页特有样式 --- */
        .info-list { list-style: none; padding: 0; margin: 0; }
        .info-list li { display: flex; padding: 12px 0; border-bottom: 1px solid var(--light-bg); align-items: center; }
        .info-list li:last-child { border-bottom: none; }
        .info-label { width: 120px; font-weight: 600; color: var(--secondary-color); flex-shrink: 0; }
        .info-value { flex-grow: 1; color: var(--text-color); }
        .description-box { background-color: var(--light-bg); padding: 15px; border-radius: 4px; border: 1px solid var(--border-color); white-space: pre-wrap; }
        
        /* 编辑模式样式 */
        .form-control { width: 100%; padding: 8px; border: 1px solid var(--border-color); border-radius: 4px; }
        .edit-actions { margin-top: 20px; text-align: right; }

        /* 时间轴样式的日志 */
        .timeline { position: relative; padding-left: 30px; }
        .timeline::before { content: ''; position: absolute; left: 10px; top: 5px; bottom: 5px; width: 2px; background-color: var(--border-color); }
        .timeline-item { position: relative; margin-bottom: 25px; }
        .timeline-item::before { content: ''; position: absolute; left: -26px; top: 5px; width: 14px; height: 14px; border-radius: 50%; background-color: #fff; border: 3px solid var(--primary-color); }
        .timeline-header { display: flex; justify-content: space-between; margin-bottom: 8px; font-size: 0.9rem; }
        .timeline-time { color: var(--secondary-color); }
        .timeline-action { font-weight: 600; }
        .timeline-body { background-color: var(--light-bg); padding: 12px; border-radius: 4px; font-size: 0.95rem; }
        .timeline-auditor { font-size: 0.85rem; color: var(--secondary-color); margin-top: 8px; text-align: right; }
    </style>

    <div class="container">
        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-danger" />

        <div class="card">
            <div class="card-header">
                <h2 style="margin:0; font-size:1.25rem;">消息详情</h2>
                <asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="~/MyMessages.aspx" Text="← 返回消息列表" CssClass="btn btn-secondary btn-sm" />
            </div>
        </div>

        <div class="card">
            <div class="card-header">
                <h3><i class="fas fa-info-circle"></i> 事件基础信息</h3>
                <asp:Label ID="lblStatusBadge" runat="server" CssClass="badge" />
            </div>
            <div class="card-body">
                
                <%-- Panel View：只读显示模式 (默认显示) --%>
                <asp:Panel ID="pnlView" runat="server">
                    <ul class="info-list">
                        <li>
                            <span class="info-label">事件编号：</span>
                            <span class="info-value"><asp:Label ID="lblIncidentId" runat="server" /></span>
                        </li>
                        <li>
                            <span class="info-label">事件类型：</span>
                            <span class="info-value"><asp:Label ID="lblType" runat="server" /></span>
                        </li>
                        <li>
                            <span class="info-label">发生时间：</span>
                            <span class="info-value"><asp:Label ID="lblOccurTime" runat="server" /></span>
                        </li>
                        <li>
                            <span class="info-label">发生地点：</span>
                            <span class="info-value"><asp:Label ID="lblLocation" runat="server" /></span>
                        </li>
                        <li>
                            <span class="info-label">当前状态：</span>
                            <span class="info-value" style="font-weight:600;"><asp:Label ID="lblStatus" runat="server" /></span>
                        </li>
                        <li style="flex-direction:column; align-items:flex-start;">
                            <span class="info-label" style="margin-bottom:8px;">详细描述：</span>
                            <div class="info-value description-box">
                                <asp:Label ID="lblDescription" runat="server" />
                            </div>
                        </li>
                    </ul>
                </asp:Panel>

                <%-- Panel Edit：编辑模式 (仅当状态为"待补充"时显示) --%>
                <asp:Panel ID="pnlEdit" runat="server" Visible="false">
                    <div class="alert alert-warning">
                        <i class="fas fa-exclamation-triangle"></i> 当前事件需要补充信息，请修改后重新提交给审核员。
                    </div>
                    <ul class="info-list">
                        <li>
                            <span class="info-label">事件编号：</span>
                            <span class="info-value">
                                <asp:Label ID="lblEditIncidentIdDisplay" runat="server" />
                                <asp:HiddenField ID="hidIncidentId" runat="server" />
                            </span>
                        </li>
                        <li>
                            <span class="info-label required">事件类型：</span>
                            <div class="info-value">
                                <%-- 修复：统一使用 ddlEditType --%>
                                <asp:DropDownList ID="ddlEditType" runat="server" CssClass="form-control">
                                    <%-- 选项由后端统一绑定：事故、事故征候、一般事件 --%>
                                </asp:DropDownList>
                            </div>
                        </li>
                        <li>
                            <span class="info-label required">发生时间：</span>
                            <div class="info-value">
                                <asp:TextBox ID="txtEditOccurTime" runat="server" CssClass="form-control" TextMode="DateTimeLocal" />
                            </div>
                        </li>
                        <li>
                            <span class="info-label required">发生地点：</span>
                            <div class="info-value">
                                <asp:DropDownList ID="ddlEditLocation" runat="server" CssClass="form-control">
                                    <%-- 选项由后端统一绑定：跑道、滑行道、登机口、候机楼、机库、航站楼 --%>
                                </asp:DropDownList>
                            </div>
                        </li>
                        <li style="flex-direction:column; align-items:flex-start;">
                            <span class="info-label required" style="margin-bottom:8px;">详细描述：</span>
                            <div class="info-value" style="width:100%">
                                <asp:TextBox ID="txtEditDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="6" />
                            </div>
                        </li>
                    </ul>
                    <div class="edit-actions">
                        <%-- 修复：确保按钮有正确的ID和事件 --%>
                        <asp:Button ID="btnResubmit" runat="server" Text="修改并重新提交" OnClick="btnResubmit_Click" CssClass="btn btn-primary" OnClientClick="return confirm('确定要提交修改后的信息吗？提交后状态将变为待审核。');" />
                    </div>
                </asp:Panel>

            </div>
        </div>

        <asp:Panel ID="pnlAuditLogs" runat="server" CssClass="card" Visible="false">
            <div class="card-header">
                <h3><i class="fas fa-history"></i> 审核处理记录</h3>
            </div>
            <div class="card-body">
                <div class="timeline">
                    <asp:Repeater ID="rptLogs" runat="server">
                        <ItemTemplate>
                            <div class="timeline-item">
                                <div class="timeline-header">
                                    <span class="timeline-action"><%# Eval("Action") %></span>
                                    <span class="timeline-time"><%# Eval("Action_time", "{0:yyyy-MM-dd HH:mm}") %></span>
                                </div>
                                <div class="timeline-body">
                                    <%# Eval("Reason") %>
                                    <div class="timeline-auditor">
                                        处理人：<%# Eval("Auditor_name") %>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Label ID="lblNoLogs" runat="server" Text="暂无审核记录" Visible="false" CssClass="text-muted" style="padding-left:10px;" />
                </div>
            </div>
        </asp:Panel>

    </div>
</asp:Content>
