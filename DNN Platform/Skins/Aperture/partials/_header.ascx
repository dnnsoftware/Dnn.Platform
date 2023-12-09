<header>
  <div class="eyebrow-bar">
    <div class="aperture-container">
      <dnn:Login runat="server" id="dnnLogin" />
      <dnn:User runat="server" id="dnnUser" />
    </div>
  </div>
  <div class="logo-menu-bar">
    <div class="aperture-container">
      <dnn:LOGO id="dnnLOGO" runat="server" />
      <dnn:MENU id="menu_desktop" CssClass="dnn-d-none dnn-d-md-block" MenuStyle="menus/desktop" runat="server" NodeSelector="*,0,2"></dnn:MENU>
      <dnn:MENU id="menu_mobile" CssClass="dnn-d-flex dnn-d-md-none" MenuStyle="menus/mobile" runat="server" NodeSelector="*,0,2"></dnn:MENU>
    </div>
  </div>
</header>