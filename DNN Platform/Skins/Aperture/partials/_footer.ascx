<footer>
  <div class="aperture-container">
        <div class="footer-left">
          <div class="footer-menu">
            <dnn:MENU id="menu_footer" CssClass="dnn-d-none dnn-d-md-block" MenuStyle="menus/footer" runat="server" NodeSelector="*,0,1"></dnn:MENU>
          </div>
          <div class="footer-terms-privacy">
            <dnn:TERMS id="dnnTerms" Text="Terms" runat="server" CssClass="dnn-terms" /><dnn:PRIVACY id="dnnPrivacy" Text="Privacy" runat="server" />
          </div>
          <div class="footer-copyright">
            <dnn:COPYRIGHT id="dnnCopyright" runat="server" />
          </div>
        </div>
        <div id="FooterPane" runat="server" class="footer-right"></div>
      </div>
    </div>
  </div>
</footer>
<dnn:Login runat="server" id="dnnHiddenLogin" CssClass="hiddenLogin" />