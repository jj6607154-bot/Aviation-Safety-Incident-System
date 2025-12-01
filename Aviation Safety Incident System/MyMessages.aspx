<%@ Page Title="我的消息" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="MyMessages.aspx.cs" Inherits="WebForms.MyMessages" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        /* --- 全局变量与基础样式 --- */
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

        /* 页面容器 */
        .container {
            padding: 20px;
            max-width: 1200px;
            margin: 0 auto;
        }

        /* 卡片式布局 */
        .card {
            background-color: #fff;
            border: 1px solid var(--border-color);
            border-radius: 8px;
            box-shadow: var(--shadow);
            margin-bottom: 20px;
            overflow: hidden; /* 确保圆角也能包裹住内部元素 */
        }

        .card-header {
            padding: 15px 20px;
            background-color: #fff;
            border-bottom: 1px solid var(--border-color);
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .card-header h2 {
            margin: 0;
            font-size: 1.25rem;
            color: var(--text-color);
            font-weight: 600;
        }

        .card-body {
            padding: 20px;
        }

        /* 消息提示框 */
        .alert {
            padding: 12px 15px;
            margin-bottom: 15px;
            border: 1px solid transparent;
            border-radius: 4px;
            font-size: 0.9rem;
        }
        .alert-danger {
            color: #721c24;
            background-color: #f8d7da;
            border-color: #f5c6cb;
        }
        /* 默认隐藏 */
        #MainContent_lblMessage:empty { display: none; }


        /* --- 工具栏与筛选 --- */
        .toolbar {
            display: flex;
            gap: 15px;
            align-items: center;
            padding: 10px 15px;
            background-color: var(--light-bg);
            border-bottom: 1px solid var(--border-color);
            flex-wrap: wrap; /* 小屏幕自动换行 */
        }

        .filter-group {
            display: flex;
            align-items: center;
            gap: 8px;
        }

        /* 表单控件样式 */
        .form-control {
            padding: 6px 10px;
            border: 1px solid var(--border-color);
            border-radius: 4px;
            font-size: 0.9rem;
        }

        /* 按钮通用样式 */
        .btn {
            display: inline-block;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            user-select: none;
            border: 1px solid transparent;
            padding: 6px 12px;
            font-size: 0.9rem;
            line-height: 1.5;
            border-radius: 4px;
            transition: color 0.15s, background-color 0.15s, border-color 0.15s, box-shadow 0.15s;
            cursor: pointer;
            text-decoration: none;
        }

        .btn-primary { color: #fff; background-color: var(--primary-color); border-color: var(--primary-color); }
        .btn-primary:hover { background-color: #0069d9; border-color: #0062cc; }
        .btn-secondary { color: #fff; background-color: var(--secondary-color); border-color: var(--secondary-color); }
        .btn-secondary:hover { background-color: #5a6268; border-color: #545b62; }
        .btn-danger { color: #fff; background-color: var(--danger-color); border-color: var(--danger-color); }
        .btn-danger:hover { background-color: #c82333; border-color: #bd2130; }
        .btn-sm { padding: 4px 8px; font-size: 0.8rem; }


        /* --- 表格样式 --- */
        .table-responsive {
            overflow-x: auto;
        }
        .table {
            width: 100%;
            margin-bottom: 1rem;
            background-color: transparent;
            border-collapse: collapse;
        }
        .table th, .table td {
            padding: 12px;
            vertical-align: top;
            border-top: 1px solid var(--border-color);
            text-align: left;
        }
        .table thead th {
            vertical-align: bottom;
            border-bottom: 2px solid var(--border-color);
            background-color: var(--light-bg);
            font-weight: 600;
            color: var(--text-color);
        }
        .table-hover tbody tr:hover {
            background-color: rgba(0,0,0,.03);
        }

        /* 状态标签 */
        .badge {
            display: inline-block;
            padding: 4px 8px;
            font-size: 0.8rem;
            font-weight: 600;
            line-height: 1;
            text-align: center;
            white-space: nowrap;
            vertical-align: baseline;
            border-radius: 10px;
            color: #fff;
        }
        .badge-danger { background-color: var(--danger-color); }
        .badge-warning { background-color: var(--warning-color); color: #212529; }
        .badge-success { background-color: var(--success-color); }
        .badge-primary { background-color: var(--primary-color); }

        /* 审核理由文本 */
        .reason-text {
            white-space: normal;
            word-break: break-word;
            max-width: 300px; /* 限制最大宽度，避免过长撑开表格 */
            color: var(--secondary-color);
        }
        
        /* 操作链接 */
        .action-link {
            margin-right: 8px;
            font-weight: 500;
        }
        .text-danger { color: var(--danger-color) !important; }
    </style>

    <div class="container">
        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-danger" />

        <div class="card">
            <div class="card-header">
                <h2>我的消息</h2>
                <asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="javascript:history.back()" Text="← 返回上一页" CssClass="btn btn-secondary btn-sm" />
            </div>

            <div class="toolbar">
                <div class="filter-group">
                    <label for="<%= ddlStatusFilter.ClientID %>">状态筛选：</label>
                    <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-control">
                        <asp:ListItem Text="全部" Value="" />
                        <asp:ListItem Text="待重新提交" Value="待重新提交" />
                        <asp:ListItem Text="待补充" Value="待补充" />
                        <asp:ListItem Text="已驳回" Value="已驳回" />
                        <asp:ListItem Text="处理中" Value="处理中" />
                    </asp:DropDownList>
                </div>
                <asp:Button ID="btnFilter" runat="server" Text="应用筛选" OnClick="btnFilter_Click" CssClass="btn btn-primary btn-sm" />
            </div>

            <div class="card-body">
                <div class="table-responsive">
                    <%-- 小白讲解：这里展示“被驳回/待补充”的事件，以及审核人员填写的理由 --%>
                    <%-- 增加 DataKeyNames="Incident_id" 方便在 RowCommand 中获取主键 --%>
                    <asp:GridView ID="gvMessages" runat="server" AutoGenerateColumns="false" EmptyDataText="暂无需要处理的消息"
                        OnRowCommand="gvMessages_RowCommand" CssClass="table table-hover" GridLines="None" DataKeyNames="Incident_id">
                        <Columns>
                            <asp:TemplateField HeaderText="编号" ItemStyle-Width="60px">
                                <ItemTemplate>
                                    <%# Container.DataItemIndex + 1 %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Incident_type" HeaderText="事件类型" />
                            <asp:BoundField DataField="Occur_time" HeaderText="发生时间" DataFormatString="{0:yyyy-MM-dd}" ItemStyle-Width="110px" />
                            <asp:TemplateField HeaderText="当前状态" ItemStyle-Width="100px">
                                <ItemTemplate>
                                    <%-- 根据状态显示不同颜色的标签 --%>
                                    <span class='badge <%# 
                                        Eval("Incident_status").ToString() == "已驳回" ? "badge-danger" : 
                                        (Eval("Incident_status").ToString() == "待补充" ? "badge-warning" : 
                                        (Eval("Incident_status").ToString() == "已公开" ? "badge-success" : "badge-primary")) %>'>
                                        <%# Eval("Incident_status") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="LastAction" HeaderText="最近动作" ItemStyle-Width="100px" />
                            <asp:TemplateField HeaderText="审核理由">
                                <ItemTemplate>
                                    <div class="reason-text"> <%# Eval("Reason") %> </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="操作" ItemStyle-Width="150px">
                                <ItemTemplate>
                                    <asp:HyperLink runat="server" NavigateUrl='<%# Eval("Incident_id", "~/MyMessageDetail.aspx?id={0}") %>' Text="查看详情" CssClass="btn btn-primary btn-sm action-link" />
                                    <%-- 使用 LinkButton 并添加确认提示 --%>
                                    <asp:LinkButton ID="btnDeleteMsg" runat="server" Text="删除" CommandName="DeleteMsg" CommandArgument='<%# Eval("Incident_id") %>' OnClientClick="return confirm('确定要删除该事件及其所有审核记录吗？此操作不可恢复。');" CssClass="btn btn-danger btn-sm" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
