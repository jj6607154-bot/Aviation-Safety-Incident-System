<%@ Page Title="事件查询" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="IncidentQuery.aspx.cs" Inherits="WebForms.IncidentQuery" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="Server">
    <style>
        /* 页面整体样式 */
        body {
            background: linear-gradient(135deg, #f8fbff 0%, #f0f7ff 100%);
            font-family: "微软雅黑", "Segoe UI", Arial, sans-serif;
        }

        /* 页面布局 */
        .page-wrapper { 
            min-height: 70vh; 
            display: grid; 
            gap: 24px; 
            padding: 20px;
            max-width: 1400px;
            margin: 0 auto;
        }

        /* 查询卡片样式 */
        .query-card { 
            background: #fff; 
            border: 1px solid #e8f4ff; 
            border-radius: 16px; 
            box-shadow: 0 12px 40px rgba(0,0,0,0.08); 
            padding: 28px 32px;
            position: relative;
            overflow: hidden;
        }

        .query-card:before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 4px;
            background: linear-gradient(to right, #1890ff, #36cfc9);
        }

        /* 标题样式 */
        .title { 
            margin: 0 0 8px; 
            font-size: 28px; 
            font-weight: 800; 
            color: #0c5aa6;
        }

        .subtitle { 
            margin: 0 0 20px; 
            font-size: 15px; 
            color: #666; 
            line-height: 1.6;
        }

        /* 消息提示样式 */
        .message { 
            margin-bottom: 16px; 
            font-size: 14px; 
            color: #ff4d4f;
            padding: 12px 16px;
            border-radius: 8px;
            background: linear-gradient(135deg, #fff2f0, #fff);
            border-left: 4px solid #ff4d4f;
            box-shadow: 0 2px 8px rgba(255, 77, 79, 0.1);
        }

        .message:empty { display: none; }

        .validator { 
            margin-top: 6px; 
            font-size: 12px; 
            color: #ff4d4f; 
        }

        /* 表单网格布局 */
        .form-grid { 
            display: grid; 
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); 
            gap: 20px; 
            margin-bottom: 8px;
        }

        .form-group { 
            display: flex; 
            flex-direction: column; 
        }

        .form-label { 
            margin-bottom: 8px; 
            font-size: 14px; 
            color: #0c5aa6;
            font-weight: 600;
        }

        .form-input, .form-select { 
            width: 100%; 
            padding: 12px 16px; 
            font-size: 14px; 
            border: 1px solid #e6f7ff; 
            border-radius: 8px; 
            background: #fafcff;
            transition: all 0.3s ease;
            box-shadow: 0 2px 6px rgba(0,0,0,0.04);
        }

        .form-input:focus, .form-select:focus { 
            outline: none; 
            border-color: #1890ff; 
            box-shadow: 0 0 0 3px rgba(24,144,255,0.15); 
            background: #fff;
            transform: translateY(-1px);
        }

        .form-input::placeholder {
            color: #999;
        }

        /* 按钮样式 */
        .btn-row { 
            margin-top: 16px; 
            display: flex; 
            gap: 12px; 
        }

        .btn-primary { 
            padding: 12px 24px; 
            font-size: 15px; 
            font-weight: 600; 
            color: #fff; 
            background: linear-gradient(135deg, #1890ff, #36cfc9);
            border: none; 
            border-radius: 10px; 
            cursor: pointer; 
            transition: all 0.3s ease;
            box-shadow: 0 6px 16px rgba(24, 144, 255, 0.25);
            position: relative;
            overflow: hidden;
        }

        .btn-primary:before {
            content: '';
            position: absolute;
            top: 0;
            left: -100%;
            width: 100%;
            height: 100%;
            background: linear-gradient(90deg, transparent, rgba(255,255,255,0.3), transparent);
            transition: left 0.5s;
        }

        .btn-primary:hover:before {
            left: 100%;
        }

        .btn-primary:hover { 
            background: linear-gradient(135deg, #1677ff, #13c2c2);
            transform: translateY(-2px);
            box-shadow: 0 8px 20px rgba(24, 144, 255, 0.35);
        }

        .btn-primary:active { 
            transform: translateY(0); 
        }

        /* 数据表格样式 - 优化列宽分配 */
        .data-grid { 
            width: 100%; 
            border-collapse: collapse; 
            background: #fff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 8px 24px rgba(0,0,0,0.08);
            margin-top: 8px;
            table-layout: fixed;
        }

        .data-grid th { 
            background: linear-gradient(135deg, #f8fbff, #e6f7ff);
            color: #0c5aa6; 
            padding: 16px 12px; 
            border-bottom: 2px solid #e6f7ff; 
            text-align: left; 
            font-weight: 700;
            font-size: 15px;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        .data-grid td { 
            padding: 14px 12px; 
            border-bottom: 1px solid #f0f8ff; 
            color: #555; 
            line-height: 1.6;
            transition: all 0.3s ease;
            vertical-align: top;
            overflow: hidden;
        }

        .data-grid tr:nth-child(even) td { 
            background: #fafcff; 
        }

        .data-grid tr:hover td {
            background: #eef6ff;
            transform: translateY(-1px);
            box-shadow: 0 4px 12px rgba(24, 144, 255, 0.1);
        }

        /* 列宽设置 - 描述列最大化 */
        .col-serial { width: 60px; }
        .col-id { width: 90px; }
        .col-type { width: 100px; }
        .col-time { width: 140px; }
        .col-loc { width: 120px; }
        .col-status { width: 90px; }
        .col-desc { width: auto; min-width: 300px; }

        /* 描述文本样式 - 优化显示 */
        .desc-wrap { 
            white-space: normal; 
            word-break: break-word; 
            line-height: 1.6;
            color: #555;
            max-height: 4.8em; /* 大约3行高度 */
            overflow: hidden;
            display: -webkit-box;
            -webkit-line-clamp: 3;
            -webkit-box-orient: vertical;
            position: relative;
        }

        .desc-wrap.expanded {
            max-height: none;
            -webkit-line-clamp: unset;
        }

        .toggle-desc { 
            display: inline-block; 
            margin-top: 8px; 
            color: #1890ff; 
            text-decoration: none; 
            font-size: 12px;
            font-weight: 500;
            padding: 4px 8px;
            border-radius: 4px;
            background: #f0f8ff;
            transition: all 0.3s ease;
            cursor: pointer;
        }

        .toggle-desc:hover { 
            color: #096dd9; 
            background: #e6f7ff;
            text-decoration: none;
            transform: translateY(-1px);
        }

        /* 空数据样式 */
        .empty-data {
            text-align: center;
            padding: 40px;
            color: #999;
            font-size: 15px;
            background: #fafbfc;
        }

        /* 验证摘要样式 */
        #valSummary {
            padding: 16px;
            margin-bottom: 20px;
            border-radius: 8px;
            background: linear-gradient(135deg, #fff2f0, #fff);
            border-left: 4px solid #ff4d4f;
            box-shadow: 0 2px 8px rgba(255, 77, 79, 0.1);
        }

        /* 表格容器 - 添加水平滚动 */
        .table-container {
            width: 100%;
            overflow-x: auto;
            border-radius: 12px;
            box-shadow: 0 8px 24px rgba(0,0,0,0.08);
        }

        /* 响应式调整 */
        @media (max-width: 768px) {
            .page-wrapper {
                padding: 16px;
                gap: 16px;
            }
            
            .query-card {
                padding: 20px 24px;
            }
            
            .form-grid {
                grid-template-columns: 1fr;
                gap: 16px;
            }
            
            .data-grid {
                font-size: 14px;
                min-width: 800px; /* 确保在小屏幕上可以水平滚动 */
            }
            
            .data-grid th,
            .data-grid td {
                padding: 12px 10px;
            }
            
            .col-desc {
                min-width: 200px;
            }
        }

        @media (max-width: 480px) {
            .title {
                font-size: 24px;
            }
            
            .query-card {
                padding: 16px 20px;
            }
            
            .btn-row {
                flex-direction: column;
            }
            
            .btn-primary {
                width: 100%;
            }
            
            .data-grid {
                min-width: 700px;
            }
        }
    </style>

    <div class="page-wrapper">
        <div class="query-card" role="form" aria-labelledby="queryTitle">
            <h2 id="queryTitle" class="title">事件查询</h2>
            <p class="subtitle">设置筛选条件后点击查询，系统将根据您的条件显示相关事件记录</p>
            
            <asp:Label ID="lblMessage" runat="server" CssClass="message" />
            <asp:ValidationSummary ID="valSummary" runat="server" CssClass="validator" HeaderText="请修正以下问题：" ValidationGroup="Query" />

            <div class="form-grid">
                <div class="form-group">
                    <asp:Label runat="server" CssClass="form-label" AssociatedControlID="ddlType" Text="事件类型" />
                    <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select">
                        <%-- 小白讲解：选项由后台统一绑定（DropdownOptions），包含"全部" --%>
                    </asp:DropDownList>
                </div>

                <div class="form-group">
                    <asp:Label runat="server" CssClass="form-label" AssociatedControlID="txtStartDate" Text="起始日期" />
                    <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-input" TextMode="Date" />
                </div>

                <div class="form-group">
                    <asp:Label runat="server" CssClass="form-label" AssociatedControlID="txtEndDate" Text="结束日期" />
                    <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-input" TextMode="Date" />
                    <asp:CustomValidator ID="cvDateRange" runat="server" CssClass="validator" ControlToValidate="txtEndDate" ErrorMessage="结束日期不能早于起始日期" OnServerValidate="cvDateRange_ServerValidate" ValidationGroup="Query" Display="Dynamic" />
                </div>

                <div class="form-group">
                    <asp:Label runat="server" CssClass="form-label" AssociatedControlID="txtLocation" Text="地点关键词" />
                    <asp:TextBox ID="txtLocation" runat="server" CssClass="form-input" placeholder="例如：T2航站楼、跑道、停机坪..." />
                </div>

                <div class="form-group">
                    <asp:Label runat="server" CssClass="form-label" AssociatedControlID="ddlStatus" Text="事件状态" />
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Text="-- 全部状态 --" Value="" />
                        <asp:ListItem Text="已上报" Value="已上报" />
                        <asp:ListItem Text="处理中" Value="处理中" />
                        <asp:ListItem Text="已驳回" Value="已驳回" />
                    </asp:DropDownList>
                </div>
            </div>

            <div class="btn-row">
                <asp:Button ID="btnSearch" runat="server" CssClass="btn-primary" Text="查询事件" OnClick="btnSearch_Click" ValidationGroup="Query" />
            </div>
        </div>

        <div class="table-container">
            <asp:GridView ID="gvIncidents" runat="server" AutoGenerateColumns="False" CssClass="data-grid" GridLines="None"
                AllowPaging="true" PageSize="4" PagerSettings-Mode="NumericFirstLast" PagerSettings-Position="TopAndBottom" PagerSettings-PageButtonCount="5"
                PagerSettings-FirstPageText="首页" PagerSettings-LastPageText="尾页" PagerSettings-NextPageText="下一页" PagerSettings-PreviousPageText="上一页"
                OnPageIndexChanging="gvIncidents_PageIndexChanging"
                EmptyDataText="<tr><td colspan='7' class='empty-data'>暂无符合条件的事件数据</td></tr>">
                <Columns>
                    <%-- 小白讲解：新增"编号(序号)"列，按当前页从1开始连续编号 --%>
                    <asp:TemplateField HeaderText="序号" HeaderStyle-CssClass="col-serial" ItemStyle-CssClass="col-serial">
                        <ItemTemplate>
                            <%# Container.DataItemIndex + 1 %>
                        </ItemTemplate>
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:TemplateField>
                    
                    <%-- 小白讲解：保留真实事件ID，避免需要时找不到原始编号 --%>
                    <asp:BoundField DataField="Incident_id" HeaderText="事件ID" HeaderStyle-CssClass="col-id" ItemStyle-CssClass="col-id">
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:BoundField>
                    
                    <asp:BoundField DataField="Incident_type" HeaderText="类型" HeaderStyle-CssClass="col-type" ItemStyle-CssClass="col-type" />
                    
                    <asp:BoundField DataField="Occur_time" HeaderText="发生时间" DataFormatString="{0:yyyy-MM-dd HH:mm}" HeaderStyle-CssClass="col-time" ItemStyle-CssClass="col-time" />
                    
                    <asp:BoundField DataField="Location" HeaderText="地点" HeaderStyle-CssClass="col-loc" ItemStyle-CssClass="col-loc" />
                    
                    <asp:BoundField DataField="Incident_status" HeaderText="状态" HeaderStyle-CssClass="col-status" ItemStyle-CssClass="col-status">
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:BoundField>
                    
                    <asp:TemplateField HeaderText="事件描述" HeaderStyle-CssClass="col-desc" ItemStyle-CssClass="col-desc">
                        <ItemTemplate>
                            <div class="desc-wrap" id='descWrap_<%# Container.DataItemIndex %>'><%# Eval("Description") %></div>
                            <a href="javascript:void(0)" class="toggle-desc" onclick='toggleDescription(<%# Container.DataItemIndex %>)'>展开全部</a>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div class="btn-row" style="margin-top:12px;">
            <asp:Button ID="btnNextPage" runat="server" CssClass="btn-primary" Text="下一页" OnClick="btnNextPage_Click" />
        </div>
    </div>

    <script type="text/javascript">
        function toggleDescription(index) {
            var descWrap = document.getElementById('descWrap_' + index);
            var toggleLink = descWrap.nextElementSibling;
            
            if (descWrap.classList.contains('expanded')) {
                descWrap.classList.remove('expanded');
                toggleLink.textContent = '展开全部';
            } else {
                descWrap.classList.add('expanded');
                toggleLink.textContent = '收起';
            }
        }
    </script>
</asp:Content>
