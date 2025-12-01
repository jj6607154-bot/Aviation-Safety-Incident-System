<%@ Page Title="仪表板" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="WebForms.Default" %>

<asp:Content ID="HeadCss" ContentPlaceHolderID="HeadContent" runat="server">
  <style>
    /* 全局样式 - 增强版 */
    * {
      margin: 0;
      padding: 0;
      box-sizing: border-box;
      font-family: "微软雅黑", "Segoe UI", Arial, sans-serif;
    }

    body {
      background: linear-gradient(135deg, #f8fbff 0%, #f0f7ff 100%);
      padding: 20px;
      min-height: 100vh;
    }

    /* 标题样式 - 增强版 */
    h1 {
      color: #1890ff;
      margin-bottom: 20px;
      font-size: 24px;
      font-weight: 700;
    }

    /* 欢迎信息样式 - 增强版 */
    .summary {
      margin: 16px 0 24px;
      color: #555;
      line-height: 1.8;
      padding: 16px;
      background: linear-gradient(135deg, #e6f7ff 0%, #f0fdff 100%);
      border-radius: 12px;
      border-left: 4px solid #1890ff;
    }

    #lblWelcome {
      font-size: 17px;
      color: #0c5aa6;
      font-weight: 600;
      display: block;
      margin-bottom: 8px;
    }

    /* 导航链接样式 - 增强版 */
    .links {
      margin: 24px 0 32px;
      display: flex;
      flex-wrap: wrap;
      gap: 12px;
    }

    .links a,
    .btn-logout {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #1890ff, #36cfc9);
      color: #fff;
      padding: 12px 20px;
      border-radius: 10px;
      text-decoration: none;
      border: none;
      font-size: 15px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s ease;
      box-shadow: 0 6px 16px rgba(24, 144, 255, 0.25);
      position: relative;
      overflow: hidden;
    }

    .links a:before,
    .btn-logout:before {
      content: '';
      position: absolute;
      top: 0;
      left: -100%;
      width: 100%;
      height: 100%;
      background: linear-gradient(90deg, transparent, rgba(255,255,255,0.3), transparent);
      transition: left 0.5s;
    }

    .links a:hover:before,
    .btn-logout:hover:before {
      left: 100%;
    }

    .links a:hover,
    .btn-logout:hover {
      background: linear-gradient(135deg, #1677ff, #13c2c2);
      transform: translateY(-2px);
      box-shadow: 0 8px 20px rgba(24, 144, 255, 0.35);
      text-decoration: none;
    }

    /* section标题样式 - 增强版 */
    .section-title {
      margin: 36px 0 20px;
      color: #0c5aa6;
      font-size: 20px;
      font-weight: 700;
      border-bottom: 2px solid #e6f7ff;
      padding-bottom: 12px;
      position: relative;
    }

    .section-title:after {
      content: '';
      position: absolute;
      bottom: -2px;
      left: 0;
      width: 60px;
      height: 2px;
      background: linear-gradient(90deg, #1890ff, #36cfc9);
    }

    /* GridView表格样式 - 增强版 */
    .grid-view {
      border-collapse: collapse;
      border: 1px solid #e8f4ff;
      font-size: 14px;
      width: 100%;
      background-color: #fff;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
      border-radius: 12px;
      overflow: hidden;
    }

    .grid-view th {
      background: linear-gradient(135deg, #f8fbff, #e6f7ff);
      border: 1px solid #e8f4ff;
      padding: 16px 12px;
      text-align: center;
      color: #0c5aa6;
      font-weight: 700;
      font-size: 15px;
    }

    .grid-view td {
      border: 1px solid #f0f8ff;
      padding: 14px 12px;
      text-align: center;
      color: #555;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      transition: all 0.3s ease;
    }

    .grid-view tr:hover {
      background: #f5fbff;
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(24, 144, 255, 0.15);
    }

    /* 详情按钮样式 - 增强版 */
    .btn-detail {
      color: #1890ff;
      text-decoration: none;
      padding: 6px 12px;
      border-radius: 6px;
      transition: all 0.3s ease;
      font-weight: 500;
      border: 1px solid #e6f7ff;
      background: #f8fbff;
    }

    .btn-detail:hover {
      color: #fff;
      background: linear-gradient(135deg, #1890ff, #36cfc9);
      border-color: #1890ff;
      text-decoration: none;
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(24, 144, 255, 0.25);
    }

    /* 分页样式 - 增强版 */
    .grid-pager {
      padding: 16px;
      background: linear-gradient(135deg, #f8fbff, #e6f7ff);
      border-top: 1px solid #e8f4ff;
    }

    .grid-pager a {
      color: #1890ff;
      margin: 0 4px;
      padding: 6px 12px;
      border: 1px solid #e8f4ff;
      border-radius: 6px;
      text-decoration: none;
      transition: all 0.3s ease;
      font-weight: 500;
    }

    .grid-pager a:hover,
    .grid-pager a.active {
      background: linear-gradient(135deg, #1890ff, #36cfc9);
      color: #fff;
      border-color: #1890ff;
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(24, 144, 255, 0.25);
    }

    .grid-pager span {
      margin: 0 4px;
      padding: 6px 12px;
      color: #999;
    }

    /* 空数据样式 - 增强版 */
    .grid-view .empty-data {
      text-align: center;
      padding: 40px;
      color: #999;
      font-size: 15px;
      background: #fafbfc;
    }

    /* 提示信息样式 - 增强版 */
    #lblMessage {
      display: block;
      padding: 12px 16px;
      border-radius: 8px;
      margin-bottom: 20px;
      font-size: 14px;
      font-weight: 500;
      border-left: 4px solid #1890ff;
      background: linear-gradient(135deg, #e6f7ff, #f0fdff);
    }

    /* 首页内容卡片样式 - 增强版 */
    .news-card { 
      background: #fff; 
      border: 1px solid #e8f4ff; 
      box-shadow: 0 8px 24px rgba(0,0,0,0.08); 
      padding: 20px; 
      margin-bottom: 20px; 
      border-radius: 12px;
      transition: all 0.3s ease;
    }
    .news-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 12px 32px rgba(0,0,0,0.12);
    }
    .news-card h4 { 
      margin-bottom: 8px; 
      color: #0c5aa6; 
      font-size: 18px;
      font-weight: 600;
    }
    .news-card .meta { 
      color: #888; 
      font-size: 13px; 
      margin-bottom: 12px; 
    }
    .news-card .content img { 
      max-width: 100%; 
      height: auto; 
      border: 1px solid #eee; 
      border-radius: 8px;
    }

    /* 仪表盘美观版：头部横幅（Hero） - 增强版 */
    .hero {
      background: linear-gradient(135deg, #e6f7ff 0%, #f0fdff 100%);
      border: 1px solid #e6f7ff;
      box-shadow: 0 12px 40px rgba(24,144,255,0.15);
      border-radius: 16px;
      padding: 32px;
      margin-bottom: 28px;
      position: relative;
      overflow: hidden;
    }
    .hero:before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      width: 6px;
      height: 100%;
      background: linear-gradient(to bottom, #1890ff, #36cfc9);
    }
    .hero-title {
      font-size: 32px;
      color: #0c5aa6;
      margin-bottom: 12px;
      font-weight: 800;
    }
    .hero-subtitle {
      font-size: 17px;
      color: #4a4a4a;
      margin-bottom: 16px;
      line-height: 1.8;
    }

    /* 功能卡片 - 增强版 */
    .card-grid { 
      display: grid; 
      grid-template-columns: repeat(3, 1fr); 
      gap: 20px; 
    }
    @media (max-width: 900px) { .card-grid { grid-template-columns: repeat(2, 1fr); } }
    @media (max-width: 600px) { .card-grid { grid-template-columns: 1fr; } }
    .card {
      background: #fff;
      border: 1px solid #e8f4ff;
      border-radius: 12px;
      padding: 24px;
      box-shadow: 0 8px 24px rgba(0,0,0,0.08);
      transition: all 0.3s ease;
      position: relative;
      overflow: hidden;
    }
    .card:before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      width: 4px;
      height: 100%;
      background: linear-gradient(to bottom, #1890ff, #36cfc9);
    }
    .card:hover {
      transform: translateY(-5px);
      box-shadow: 0 16px 40px rgba(0,0,0,0.12);
    }
    .card h4 { 
      color: #0c5aa6; 
      font-size: 18px; 
      margin-bottom: 12px; 
      font-weight: 700;
    }
    .card p { 
      color: #666; 
      font-size: 14px; 
      line-height: 1.7; 
    }

    /* 首页美化：飞机元素与轮播图 - 增强版 */
    .home-banner { 
      position: relative; 
      overflow: hidden; 
      border-radius: 16px; 
      border: 1px solid #e6f7ff; 
      background: linear-gradient(135deg,#e6f7ff,#ffffff); 
      box-shadow: 0 12px 40px rgba(24,144,255,.15); 
      margin-bottom: 28px; 
    }
    .plane { 
      position: absolute; 
      top: 20px; 
      left: -120px; 
      font-size: 36px; 
      color: #0c5aa6; 
      animation: fly 8s linear infinite; 
      z-index: 2;
      filter: drop-shadow(0 4px 8px rgba(0,0,0,0.1));
    }
    @keyframes fly { 
      0% { transform: translateX(0) translateY(0) rotate(0deg); } 
      30% { transform: translateX(40vw) translateY(-8px) rotate(2deg);} 
      60% { transform: translateX(75vw) translateY(4px) rotate(-2deg);} 
      100% { transform: translateX(110vw) translateY(0) rotate(0deg);} 
    }
    .carousel { 
      position: relative; 
      width: 100%; 
      height: 360px; 
    }
    .carousel .slide { 
      position:absolute; 
      inset:0; 
      background-size: cover; 
      background-position:center; 
      border-radius: 16px; 
      opacity: 0; 
      transition: opacity .6s ease; 
    }
    .carousel .slide.show { 
      opacity: 1; 
    }
    .carousel .mask { 
      position:absolute; 
      inset:0; 
      background: linear-gradient(180deg, rgba(0,0,0,.2), rgba(0,0,0,.3)); 
      border-radius: 16px; 
    }
    .carousel .caption { 
      position:absolute; 
      left: 28px; 
      bottom: 24px; 
      color: #fff; 
      text-shadow: 0 4px 12px rgba(0,0,0,0.4); 
      font-size: 20px; 
      font-weight: 700; 
      z-index: 2;
    }
    .carousel .controls { 
      position:absolute; 
      right: 20px; 
      bottom: 20px; 
      display:flex; 
      gap: 8px; 
      z-index: 2;
    }
    .carousel .dot { 
      width: 10px; 
      height: 10px; 
      border-radius: 50%; 
      background: #fff; 
      opacity: .6; 
      cursor: pointer; 
      transition: all 0.3s ease;
    }
    .carousel .dot.active { 
      opacity: 1; 
      background: #1890ff; 
      transform: scale(1.2);
    }
    .carousel .dot:hover {
      opacity: 0.8;
      transform: scale(1.1);
    }
    .carousel .nav { 
      position:absolute; 
      top: 50%; 
      transform: translateY(-50%); 
      width: 44px; 
      height: 44px; 
      border-radius: 50%; 
      background: rgba(255,255,255,.9); 
      display: flex; 
      align-items: center; 
      justify-content: center; 
      cursor: pointer; 
      box-shadow: 0 6px 20px rgba(0,0,0,.15); 
      transition: all 0.3s ease;
      z-index: 2;
      font-size: 20px;
      font-weight: bold;
      color: #1890ff;
    }
    .carousel .nav:hover { 
      background: #fff; 
      transform: translateY(-50%) scale(1.1);
      box-shadow: 0 8px 25px rgba(0,0,0,.2);
    }
    .carousel .prev { 
      left: 20px; 
    }
    .carousel .next { 
      right: 20px; 
    }
  </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <div class="home-banner">
    <div class="plane">✈</div>
    <div class="carousel" id="homeCarousel">
      <div class="slide show" style='background-image:url("<%= ResolveUrl("~/picture/1.jpg") %>");'>
        <div class="mask"></div>
        <div class="caption">机场应急 · 联动演练</div>
      </div>
      <div class="slide" style='background-image:url("<%= ResolveUrl("~/picture/2.jpg") %>");'>
        <div class="mask"></div>
        <div class="caption">航班起降 · 安全保障</div>
      </div>
      <div class="slide" style='background-image:url("<%= ResolveUrl("~/picture/3.jpg") %>");'>
        <div class="mask"></div>
        <div class="caption">塔台管制 · 智能协同</div>
      </div>
      <div class="slide" style='background-image:url("<%= ResolveUrl("~/picture/4.jpg") %>");'>
        <div class="mask"></div>
        <div class="caption">机坪运行 · 规范流程</div>
      </div>
      <div class="slide" style='background-image:url("<%= ResolveUrl("~/picture/5.jpg") %>");'>
        <div class="mask"></div>
        <div class="caption">事故处置 · 快速响应</div>
      </div>
      <div class="nav prev" aria-label="prev">‹</div>
      <div class="nav next" aria-label="next">›</div>
      <div class="controls"></div>
    </div>
  </div>

  <!-- 系统提示信息（错误/成功） -->
  <asp:Label ID="lblMessage" runat="server" />

  <!-- 欢迎信息（按需隐藏） -->
  <asp:Panel ID="panelSummary" runat="server" Visible="false" CssClass="summary">
    <asp:Label ID="lblWelcome" runat="server" />
  </asp:Panel>

  <!-- 快速入口（仍保留角色控制，但整体页面以介绍为主） -->
  <div class="links">
    <!-- 不安全事件查询链接（小白讲解）：主页入口文案与导航保持一致 -->
    <asp:HyperLink ID="lnkToIncidents" runat="server" Text="不安全事件查询" />

    <!-- 信息发布（仅管理员可见） -->
    <asp:HyperLink ID="lnkToNewsManage" runat="server" NavigateUrl="~/NewsManage.aspx" Text="信息发布"
      Visible="false" />

    <!-- 用户管理（仅管理员可见） -->
    <asp:HyperLink ID="lnkToUserManage" runat="server" NavigateUrl="~/UserManage.aspx" Text="用户管理"
      Visible="false" />

    <!-- 登录/注册入口（首页不展示） -->
    <asp:HyperLink ID="lnkToLogin" runat="server" NavigateUrl="~/Login.aspx" Text="登录/注册" Visible="false" />

    <!-- 消息提醒（普通用户可见）：显示驳回/待补充的数量，点击查看详情 -->
    <asp:HyperLink ID="lnkMessages" runat="server" Text="消息提醒" Visible="false" />

    <!-- 退出按钮（登录后可见）：OnClick绑定的方法名必须与后端完全一致 -->
    <asp:Button ID="btnLogout" runat="server" Text="退出" OnClick="btnLogout_Click" CssClass="btn-logout"
      Visible="false" />
  </div>

  <script type="text/javascript">
      // 轮播初始化（小白讲解）：自动播放 + 支持左右切换 + 圆点指示
      (function () {
          var box = document.getElementById('homeCarousel');
          if (!box) return;
          var slides = box.getElementsByClassName('slide');
          var dotsWrap = box.getElementsByClassName('controls')[0];
          var prev = box.getElementsByClassName('prev')[0];
          var next = box.getElementsByClassName('next')[0];
          var idx = 0, timer = null;

          function show(i) {
              for (var k = 0; k < slides.length; k++) { slides[k].classList.remove('show'); }
              slides[i].classList.add('show');
              var dots = dotsWrap.children;
              for (var d = 0; d < dots.length; d++) { dots[d].classList.remove('active'); }
              if (dots[i]) dots[i].classList.add('active');
              idx = i;
          }

          function auto() {
              clearInterval(timer);
              timer = setInterval(function () { show((idx + 1) % slides.length); }, 4000);
          }

          // 构建圆点
          for (var i = 0; i < slides.length; i++) {
              var dot = document.createElement('span');
              dot.className = 'dot' + (i === 0 ? ' active' : '');
              (function (n) { dot.onclick = function () { show(n); auto(); }; })(i);
              dotsWrap.appendChild(dot);
          }

          // 左右切换
          prev.onclick = function () { show((idx - 1 + slides.length) % slides.length); auto(); };
          next.onclick = function () { show((idx + 1) % slides.length); auto(); };

          auto();
      })();
  </script>

  <!-- 美观介绍：功能卡片（简要介绍系统模块） -->
  <h3 class="section-title">系统功能简介</h3>
  <div class="card-grid">
    <div class="card">
      <h4>事件查询</h4>
      <p>按照时间、状态、关键字快速检索不安全事件，支持只读查看。</p>
    </div>
    <div class="card">
      <h4>事件上报</h4>
      <p>标准化上报流程，引导填写关键字段，减少漏项与误填。</p>
    </div>
    <div class="card">
      <h4>事件审核</h4>
      <p>审核人员按规范把关，支持退回补充与通过入库。</p>
    </div>
    <div class="card">
      <h4>信息发布</h4>
      <p>管理员发布新闻动态，支持图文内容、下架与首页展示控制。</p>
    </div>
    <div class="card">
      <h4>法律法规</h4>
      <p>在线查阅安全法律法规，关键词搜索直达权威链接。</p>
    </div>
    <div class="card">
      <h4>消息提醒</h4>
      <p>普通用户收到审核驳回/待补充提醒，一键跳转查看详情。</p>
    </div>
  </div>

  <!-- 新闻列表已迁移至独立页面 NewsPublished.aspx，首页不再保留重复控件 -->
</asp:Content>
