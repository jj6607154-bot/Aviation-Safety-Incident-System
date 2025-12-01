<%@ Page Language="C#" AutoEventWireup="true" CodeFile="IncidentReport.aspx.cs" Inherits="Aviation_Safety_Incident_System.IncidentReport" %>
<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>事件上报系统</title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
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
            font-family: 'Microsoft YaHei','微软雅黑','Segoe UI', Arial, sans-serif;
        }

        body {
            background: linear-gradient(135deg, #f5f7fa 0%, #e4edf5 100%);
            color: #333;
            line-height: 1.6;
            padding: 20px;
            min-height: 100vh;
        }

        .container {
            max-width: 1000px;
            margin: 0 auto;
            background: white;
            border-radius: var(--border-radius);
            box-shadow: var(--box-shadow);
            overflow: hidden;
        }

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

        .form-container {
            padding: 30px;
        }

        .form-group {
            margin-bottom: 25px;
            position: relative;
        }

        .form-row {
            display: flex;
            gap: 20px;
            margin-bottom: 25px;
        }

        .form-group.half {
            flex: 1;
        }

        .form-label {
            display: block;
            margin-bottom: 8px;
            font-weight: 600;
            color: var(--dark);
            font-size: 15px;
            cursor: pointer;
            transition: var(--transition);
            display: flex;
            align-items: center;
        }

        .form-label:hover {
            color: var(--primary);
        }

        .form-label i {
            margin-right: 8px;
            font-size: 16px;
            color: var(--primary);
        }

        .form-control {
            width: 100%;
            padding: 12px 15px;
            border: 1px solid var(--light-gray);
            border-radius: var(--border-radius);
            font-size: 15px;
            transition: var(--transition);
            background-color: white;
        }

        .form-control:focus {
            outline: none;
            border-color: var(--primary);
            box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.2);
        }

        select.form-control {
            appearance: none;
            background-image: url("data:image/svg+xml;charset=UTF-8,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%23343a40' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3e%3cpolyline points='6 9 12 15 18 9'%3e%3c/polyline%3e%3c/svg%3e");
            background-repeat: no-repeat;
            background-position: right 15px center;
            background-size: 16px;
            padding-right: 45px;
        }

        /* 固定大小的事件描述框 */
        .description-container {
            position: relative;
            margin-top: 10px;
        }
        
        .description-box {
            width: 100%;
            height: 180px;
            padding: 12px 15px;
            border: 1px solid var(--light-gray);
            border-radius: var(--border-radius);
            font-size: 15px;
            resize: none;
            overflow-y: auto;
            transition: var(--transition);
            background-color: white;
            line-height: 1.5;
        }
        
        .description-box:focus {
            outline: none;
            border-color: var(--primary);
            box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.2);
        }
        
        /* 自定义滚动条 */
        .description-box::-webkit-scrollbar {
            width: 8px;
        }
        
        .description-box::-webkit-scrollbar-track {
            background: #f1f1f1;
            border-radius: 4px;
        }
        
        .description-box::-webkit-scrollbar-thumb {
            background: var(--primary);
            border-radius: 4px;
        }
        
        .description-box::-webkit-scrollbar-thumb:hover {
            background: var(--primary-dark);
        }

        .char-counter {
            position: absolute;
            bottom: 10px;
            right: 15px;
            font-size: 13px;
            color: var(--gray);
            background: rgba(255, 255, 255, 0.8);
            padding: 2px 6px;
            border-radius: 10px;
        }

        .required::after {
            content: " *";
            color: var(--danger);
        }

        .actions {
            display: flex;
            justify-content: flex-end;
            gap: 15px;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid var(--light-gray);
        }

        .btn {
            padding: 12px 25px;
            border: none;
            border-radius: var(--border-radius);
            font-size: 15px;
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
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(52, 152, 219, 0.3);
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

        /* 顶部导航字体与按钮样式统一为微软雅黑 */
        .top-nav, .top-nav a, .top-nav span, .top-nav strong {
            font-family: 'Microsoft YaHei','微软雅黑','Segoe UI', Arial, sans-serif;
            font-size: 14px;
        }
        .top-nav .btn-logout {
            color:#333; background:#fff; padding:4px 10px; border-radius:4px; text-decoration:none; border:1px solid rgba(0,0,0,0.2);
            font-family: 'Microsoft YaHei','微软雅黑','Segoe UI', Arial, sans-serif;
        }

        .form-section {
            margin-bottom: 35px;
            padding: 20px;
            background: #fafbfc;
            border-radius: var(--border-radius);
            border: 1px solid var(--light-gray);
        }

        .section-title {
            font-size: 18px;
            font-weight: 600;
            margin-bottom: 20px;
            padding-bottom: 10px;
            border-bottom: 1px solid var(--light-gray);
            color: var(--primary);
            display: flex;
            align-items: center;
        }

        .section-title i {
            margin-right: 10px;
        }

        .custom-input-container {
            display: flex;
            gap: 15px;
            align-items: center;
        }

        .custom-input-container .form-control {
            flex: 1;
        }

        .form-hint {
            font-size: 13px;
            color: var(--gray);
            margin-top: 5px;
            display: flex;
            align-items: center;
        }

        .form-hint i {
            margin-right: 5px;
        }

        @media (max-width: 768px) {
            .form-row {
                flex-direction: column;
                gap: 0;
            }
            
            .form-container {
                padding: 20px;
            }
            
            .page-header {
                padding: 20px;
            }
            
            .form-section {
                padding: 15px;
            }
        }
    </style>
</head>
<body>
    <div class="top-nav" style="background:#1890ff;color:#fff;padding:10px 16px;display:flex;align-items:center;">
        <div style="flex:1 1 auto;">
            <a href="<%= ResolveUrl("~/Default.aspx") %>" style="color:#fff;margin-right:12px;text-decoration:none;">首页</a>
            <a href="<%= ResolveUrl("~/IncidentQuery.aspx") %>" style="color:#fff;margin-right:12px;text-decoration:none;">不安全事件查询</a>
            <a href="<%= ResolveUrl("~/Regulations.aspx") %>" style="color:#fff;margin-right:12px;text-decoration:none;">安全法律法规</a>
            <% 
                var _utObj_nav = Session["User_type"]; 
                int _ut_nav = _utObj_nav==null?0:System.Convert.ToInt32(_utObj_nav); 
                if (_ut_nav == 1) { Response.Write("<a href=\"" + ResolveUrl("~/NewsManage.aspx") + "\" style=\"color:#fff;margin-right:12px;text-decoration:none;\">新闻发布</a>"); }
            %>
            <a href="<%= ResolveUrl("~/NewsPublished.aspx") %>" style="color:#fff;margin-right:12px;text-decoration:none;">已发布新闻</a>
            <% 
                var _utObj = Session["User_type"]; 
                int _ut = _utObj==null?0:System.Convert.ToInt32(_utObj); 
                if (_ut != 3) { Response.Write("<a href=\"" + ResolveUrl("~/IncidentReport.aspx") + "\" style=\"color:#fff;margin-right:12px;text-decoration:none;\">事件上报</a>"); }
                if (_ut == 3) { Response.Write("<a href=\"" + ResolveUrl("~/IncidentAudit.aspx") + "\" style=\"color:#fff;margin-right:12px;text-decoration:none;\">事件审核</a>"); }
            %>
        </div>
        <div style="flex:0 0 auto;display:flex;align-items:center;gap:12px;">
            <% 
                var __uname = Session["User_name"] as string; 
                var __utObj = Session["User_type"]; 
                int __ut = __utObj==null?0:System.Convert.ToInt32(__utObj); 
                var __role = (__ut==1?"管理员":__ut==2?"普通用户":__ut==3?"审核人员":"未登录"); 
                if (!string.IsNullOrEmpty(__uname)) { 
            %>
                <span style="color:#fff;">欢迎，<strong style="color:#fff;"><%: __uname %>（<%: __role %>）</strong></span>
                <a href="<%= ResolveUrl("~/Default.aspx?logout=1") %>" class="btn-logout">退出</a>
            <% } else { %>
                <span style="color:#fff;">未登录</span>
                <a href="<%= ResolveUrl("~/Login.aspx") %>" class="btn-logout">登录</a>
                <a href="<%= ResolveUrl("~/Register.aspx") %>" class="btn-logout">注册</a>
            <% } %>
        </div>
    </div>
    <form id="form1" runat="server">
    <div class="container">
        <div class="page-header">
            <h1><i class="fas fa-clipboard-list"></i>事件上报</h1>
            <p>请填写事件详细信息，带 <span style="color: #e74c3c;">*</span> 的为必填项</p>
        </div>
        
        <div class="form-container">
            <div class="alert alert-danger" id="errorAlert" style="display: none;">
                <i class="fas fa-exclamation-circle"></i>
                <asp:Label ID="lblMessage" runat="server" />
            </div>
            
            <div class="form-section">
                <h2 class="section-title"><i class="fas fa-info-circle"></i>基本信息</h2>
                
                <div class="form-group">
                    <label class="form-label required" for="ddlType">
                        <i class="fas fa-tag"></i>事件类型
                    </label>
                    <div class="custom-input-container">
                        <asp:DropDownList ID="ddlType" runat="server" CssClass="form-control" ClientIDMode="Static"></asp:DropDownList>
                        <asp:TextBox ID="txtCustomType" runat="server" CssClass="form-control" ClientIDMode="Static" placeholder="自定义类型（选填）" />
                    </div>
                </div>
                
                <div class="form-row">
                    <div class="form-group half">
                        <label class="form-label required" for="txtOccurTime">
                            <i class="fas fa-calendar-alt"></i>发生日期
                        </label>
                        <asp:TextBox ID="txtOccurTime" runat="server" CssClass="form-control" ClientIDMode="Static" TextMode="Date" />
                        <div class="form-hint"><i class="fas fa-info-circle"></i>请选择事件发生的日期</div>
                    </div>
                    
                    <div class="form-group half">
                        <label class="form-label required" for="ddlLocation">
                            <i class="fas fa-map-marker-alt"></i>地点
                        </label>
                        <div class="custom-input-container">
                            <asp:DropDownList ID="ddlLocation" runat="server" CssClass="form-control" ClientIDMode="Static"></asp:DropDownList>
                            <asp:TextBox ID="txtLocationCustom" runat="server" CssClass="form-control" ClientIDMode="Static" placeholder="请输入自定义地点" Style="display: none;" />
                        </div>
                        <div class="form-hint"><i class="fas fa-info-circle"></i>请选择或输入事件发生地点</div>
                    </div>
                </div>
            </div>
            
            <div class="form-section">
                <h2 class="section-title"><i class="fas fa-align-left"></i>详细描述</h2>
                
                <div class="form-group">
                    <label class="form-label required" for="txtDescription">
                        <i class="fas fa-file-alt"></i>详细描述（不少于20字）
                    </label>
                    <div class="description-container">
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="description-box" ClientIDMode="Static" TextMode="MultiLine" placeholder="请详细描述事件的经过、原因和影响..." />
                        <div class="char-counter">0</div>
                    </div>
                    <div class="form-hint"><i class="fas fa-info-circle"></i>描述框已固定大小，可使用滚动条查看完整内容，字数不限</div>
                </div>
            </div>
            
            <div class="actions">
                <button class="btn btn-outline" id="btnCancel" type="button">
                    <i class="fas fa-times"></i>取消
                </button>
                <asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" Text="提交上报" OnClick="btnSubmit_Click" />
            </div>
        </div>
    </div>
    </form>

    <script>
        document.addEventListener('DOMContentLoaded', function () {
            // 字符计数器（无字数限制）
            const description = document.getElementById('txtDescription');
            const charCounter = document.querySelector('.char-counter');

            description.addEventListener('input', function () {
                charCounter.textContent = description.value.length;

                if (description.value.length < 20) {
                    charCounter.style.color = '#e74c3c';
                } else {
                    charCounter.style.color = '#27ae60';
                }
            });

            // 地点选择逻辑
            const locationSelect = document.getElementById('ddlLocation');
            const customLocation = document.getElementById('txtLocationCustom');

            locationSelect.addEventListener('change', function () {
                if (this.value === 'custom') {
                    customLocation.style.display = 'block';
                } else {
                    customLocation.style.display = 'none';
                }
            });

            // 取消与错误提示引用
            const cancelBtn = document.getElementById('btnCancel');
            const errorAlert = document.getElementById('errorAlert');

            // 取消按钮
            cancelBtn.addEventListener('click', function () {
                if (confirm('确定要取消吗？所有未保存的信息将会丢失。')) {
                    // 在实际应用中这里应该返回上一页或清空表单
                    document.querySelectorAll('.form-control').forEach(input => {
                        input.value = '';
                    });
                    description.value = '';
                    charCounter.textContent = '0';
                    charCounter.style.color = '#6c757d';
                    errorAlert.style.display = 'none';
                }
            });

            // 初始化日期为今天
            const today = new Date();
            const formattedDate = today.toISOString().split('T')[0];
            const occurInput = document.getElementById('txtOccurTime');
            if (occurInput) occurInput.value = formattedDate;
        });
    </script>
</body>
</html>
