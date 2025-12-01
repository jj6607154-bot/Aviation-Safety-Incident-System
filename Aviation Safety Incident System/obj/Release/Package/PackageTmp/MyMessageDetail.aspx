<%@ Page Title="消息详情" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyMessageDetail.aspx.cs" Inherits="WebForms.MyMessageDetail" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        :root {
            --primary: #3498db;
            --primary-dark: #2980b9;
            --secondary: #2ecc71;
            --secondary-dark: #27ae60;
            --danger: #e74c3c;
            --danger-dark: #c0392b;
            --warning: #f39c12;
            --warning-dark: #d35400;
            --light: #f8f9fa;
            --dark: #343a40;
            --gray: #6c757d;
            --light-gray: #e9ecef;
            --border-radius: 8px;
            --box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            --transition: all 0.3s ease;
        }

        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Segoe UI', 'Microsoft YaHei', sans-serif;
        }

        .container { background: white; border-radius: var(--border-radius); box-shadow: var(--box-shadow); overflow: hidden; }

        .page-header {
            background: linear-gradient(135deg, var(--primary), var(--primary-dark));
            color: white;
            padding: 25px 30px;
            border-bottom: 1px solid rgba(0,0,0,0.1);
            position: relative;
        }

        .page-header::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 4px;
            background: linear-gradient(90deg, var(--secondary), var(--primary), var(--warning));
        }

        .page-header h1 {
            font-size: 28px;
            font-weight: 600;
            margin-bottom: 5px;
            display: flex;
            align-items: center;
        }

        .page-header h1 i {
            margin-right: 12px;
            font-size: 32px;
        }

        .page-header p {
            opacity: 0.9;
            font-size: 15px;
        }

        .content-container {
            padding: 30px;
        }

        .alert {
            padding: 15px;
            border-radius: var(--border-radius);
            margin-bottom: 25px;
            display: flex;
            align-items: center;
        }

        .alert-danger {
            background-color: rgba(231, 76, 60, 0.1);
            color: var(--danger);
            border-left: 4px solid var(--danger);
        }

        .alert i {
            margin-right: 10px;
            font-size: 18px;
        }

        .card { background: white; border-radius: var(--border-radius); box-shadow: var(--box-shadow); margin-bottom: 25px; overflow: hidden; }

        .card-header {
            background: #f8f9fa;
            padding: 15px 20px;
            border-bottom: 1px solid var(--light-gray);
            font-weight: 600;
            color: var(--primary);
            display: flex;
            align-items: center;
        }

        .card-header i {
            margin-right: 10px;
            font-size: 18px;
        }

        .card-body {
            padding: 20px;
        }

        .info-table {
            width: 100%;
            border-collapse: collapse;
        }

        .info-table tr {
            border-bottom: 1px solid var(--light-gray);
        }

        .info-table tr:last-child {
            border-bottom: none;
        }

        .info-table td {
            padding: 12px 15px;
            vertical-align: top;
        }

        .info-table td:first-child {
            font-weight: 600;
            color: var(--dark);
            width: 120px;
            white-space: nowrap;
        }

        .info-table .status-pending {
            color: var(--warning);
            font-weight: 600;
        }

        .info-table .status-approved {
            color: var(--secondary);
            font-weight: 600;
        }

        .info-table .status-rejected {
            color: var(--danger);
            font-weight: 600;
        }

        .description-content {
            background: #f8f9fa;
            padding: 15px;
            border-radius: var(--border-radius);
            line-height: 1.6;
            max-height: 200px;
            overflow-y: auto;
        }

        .description-content::-webkit-scrollbar {
            width: 6px;
        }

        .description-content::-webkit-scrollbar-track {
            background: #f1f1f1;
            border-radius: 4px;
        }

        .description-content::-webkit-scrollbar-thumb {
            background: var(--primary);
            border-radius: 4px;
        }

        .description-content::-webkit-scrollbar-thumb:hover {
            background: var(--primary-dark);
        }

        .log-table {
            width: 100%;
            border-collapse: collapse;
        }

        .log-table th {
            background: #f8f9fa;
            padding: 12px 15px;
            text-align: left;
            font-weight: 600;
            color: var(--dark);
            border-bottom: 2px solid var(--light-gray);
        }

        .log-table td {
            padding: 12px 15px;
            border-bottom: 1px solid var(--light-gray);
        }

        .log-table tr:last-child td {
            border-bottom: none;
        }

        .log-table tr:nth-child(even) {
            background-color: #f8f9fa;
        }

        .reason-content { max-width: 380px; word-break: break-word; line-height: 1.7; }

        .hint-box {
            background: #fff6d5;
            border-left: 4px solid var(--warning);
            padding: 15px;
            border-radius: var(--border-radius);
            margin-top: 25px;
        }

        .hint-box h4 {
            color: var(--warning-dark);
            margin-bottom: 8px;
            display: flex;
            align-items: center;
        }

        .hint-box h4 i {
            margin-right: 8px;
        }

        .hint-box p {
            color: #856404;
            font-size: 14px;
            line-height: 1.5;
        }

        .action-buttons {
            display: flex;
            gap: 15px;
            margin-top: 25px;
        }

        .btn {
            padding: 10px 20px;
            border: none;
            border-radius: var(--border-radius);
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
            transition: var(--transition);
            display: inline-flex;
            align-items: center;
            justify-content: center;
        }

        .btn i {
            margin-right: 8px;
        }

        .btn-primary {
            background: var(--primary);
            color: white;
        }

        .btn-primary:hover {
            background: var(--primary-dark);
        }

        .btn-outline {
            background: transparent;
            color: var(--gray);
            border: 1px solid var(--light-gray);
        }

        .btn-outline:hover {
            background: var(--light-gray);
            color: var(--dark);
        }

        @media (max-width: 768px) {
            .content-container {
                padding: 20px;
            }
            
            .page-header {
                padding: 20px;
            }
            
            .info-table td:first-child {
                width: 100px;
            }
            
            .log-table {
                display: block;
                overflow-x: auto;
            }
        }
    </style>
    <div class="page-header">
        <h1><i class="fas fa-envelope-open-text"></i>消息详情</h1>
        <p>查看事件的详细信息与审核记录</p>
    </div>
    
    <div class="content-container">
        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-danger" />
        
        <div class="card">
            <div class="card-header">
                <i class="fas fa-info-circle"></i>事件基本信息
            </div>
            <div class="card-body">
                <table class="info-table">
                    <tr>
                        <td>事件ID：</td>
                        <td><asp:Label ID="lblIncidentId" runat="server" /></td>
                    </tr>
                    <tr>
                        <td>类型：</td>
                        <td><asp:Label ID="lblType" runat="server" /></td>
                    </tr>
                    <tr>
                        <td>发生时间：</td>
                        <td><asp:Label ID="lblOccurTime" runat="server" /></td>
                    </tr>
                    <tr>
                        <td>地点：</td>
                        <td><asp:Label ID="lblLocation" runat="server" /></td>
                    </tr>
                    <tr>
                        <td>当前状态：</td>
                        <td><asp:Label ID="lblStatus" runat="server" CssClass="status-pending" /></td>
                    </tr>
                    <tr>
                        <td style="vertical-align: top;">描述：</td>
                        <td>
                            <div class="description-content"><asp:Label ID="lblDescription" runat="server" /></div>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
        
        <div class="card">
            <div class="card-header">
                <i class="fas fa-clipboard-list"></i>审核日志
            </div>
            <div class="card-body">
                <asp:GridView ID="gvLogs" runat="server" AutoGenerateColumns="False" CssClass="log-table" GridLines="None">
                    <Columns>
                        <asp:TemplateField HeaderText="序号">
                            <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Action_time" HeaderText="时间" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                        <asp:BoundField DataField="Action" HeaderText="动作" />
                        <asp:BoundField DataField="Auditor_name" HeaderText="审核人" />
                        <asp:TemplateField HeaderText="理由">
                            <ItemTemplate>
                                <div class="reason-content"><%# Eval("Reason") %></div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
        
        
    </div>
</asp:Content>
