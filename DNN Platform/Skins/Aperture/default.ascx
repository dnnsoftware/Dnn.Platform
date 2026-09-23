<%-- 
	> This is the DNN 10 **default theme** — do not modify the distributed Aperture files directly (Skin/Container folders); they **will be overwritten on upgrade**.
	> Need a custom version? Copy the **Skin** and **Container** folders and customize the copy instead.
--%>
<!--#include file="partials/_registers.ascx" -->
<!--#include file="partials/_includes.ascx" -->

<div class="aperture-theme">
  <!-- Header/NavBar -->
  <!--#include file="partials/_header.ascx" -->
  
  <!-- Main Content -->
  <main class="aperture-main">
    <div id="BannerPane" runat="server"></div>
    <div id="ContentPane" class="aperture-content-pane" runat="server"></div> 
    <div id="FluidPane" runat="server"></div>
  </main>

  <!-- Footer -->
  <!--#include file="partials/_footer.ascx" -->
</div>
