<footer>
  <div class="bg-light-shade">
    <div class="container">
      <div class="row">
        <div class="col-md-6">
          <div id="FooterPaneA" runat="server"></div>
        </div>
        <div class="col-md-6">
          <div id="FooterPaneB" runat="server"></div>
        </div>
      </div>
    </div>
  </div>
  <div class="bg-main-shade text-white">
    <div class="container py-2">
      <div class="row">
        <div class="col-12">
          <ul class="list-unstyled disclaimer">
            <li><dnn:COPYRIGHT id="dnnCopyright" runat="server" /></li>
            <li><dnn:TERMS id="dnnTerms" Text="Terms" runat="server" /></li>
            <li><dnn:PRIVACY id="dnnPrivacy" Text="Privacy" runat="server" /></li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</footer>
<dnn:Login runat="server" id="dnnHiddenLogin" CssClass="hiddenLogin" />