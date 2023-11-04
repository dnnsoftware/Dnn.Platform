<header class="sticky-top">
  <div class="bg-main-accent py-1">
    <div class="container">
      <div class="row justify-content-end">
        <div class="col-auto">
          <ul class="list-unstyled user-controls">
            <li><dnn:Login runat="server" id="dnnLogin" /></li>
            <li><dnn:User runat="server" id="dnnUser" /></li>
            <li><dnn:Search runat="server" id="dnnSearch" ShowSite="false" ShowWeb="false" Submit="<i class='fas fa-search'></i>" /></li>
            <li style="display:none;"><dnn:Language runat="server" id="dnnLanguage" ShowMenu="false" ShowLinks="false" /></li>
          </ul>
        </div>
      </div>
    </div>
  </div>
  <div class="bg-light-shade">
    <div class="container">
      <div class="row justify-content-between flex-nowrap">
        <div class="col-auto">
          <dnn:LOGO id="dnnLOGO" runat="server" />
        </div>
        <div class="col-auto ms-auto d-none d-xl-block">
          <dnn:MENU id="menu_desktop" MenuStyle="menus/desktop" runat="server" NodeSelector="*,0,2"></dnn:MENU>
        </div>
        <div class="col-auto ms-auto d-flex align-items-center d-xl-none">
          <dnn:MENU id="menu_mobile" MenuStyle="menus/mobile" runat="server" NodeSelector="*,0,2"></dnn:MENU>
        </div>
      </div>
    </div>
  </div>
</header>