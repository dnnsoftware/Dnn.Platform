<header class="aperture-header">
  <div class="eyebrow-bar">
    <div class="aperture-container">
      <dnn:Login runat="server" id="dnnLogin" />
      <dnn:User runat="server" id="dnnUser" />
    </div>
  </div>
  <div class="logo-menu-bar">
    <div class="aperture-container">
      <dnn:LOGO id="dnnLOGO" runat="server" InjectSvg="true" />
      <dnn:MENU id="menu_desktop" CssClass="aperture-d-none aperture-d-md-block" MenuStyle="menus/desktop" runat="server" NodeSelector="*,0,2"></dnn:MENU>
      <dnn:MENU id="menu_mobile" CssClass="aperture-d-flex aperture-d-md-none" MenuStyle="menus/mobile" runat="server" NodeSelector="*,0,2"></dnn:MENU>
    </div>
  </div>
</header>