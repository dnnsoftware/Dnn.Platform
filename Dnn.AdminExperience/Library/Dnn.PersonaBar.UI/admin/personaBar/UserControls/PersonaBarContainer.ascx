<%@ Control language="C#" Inherits="Dnn.PersonaBar.UI.UserControls.PersonaBarContainer" AutoEventWireup="false"  Codebehind="PersonaBarContainer.ascx.cs" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<asp:Panel runat="server" ID="PersonaBarPanel" Visible="False" CssClass="personaBarContainer personalBarContainer">
    <div id="personabar-placeholder" class="personabar-placeholder">
        <!--
            A temporary UI placeholder to avoid the sliding effect when PersonaBar loads
        -->
    </div>
	<iframe id="personaBar-iframe" allowTransparency="true" frameBorder="0" scrolling="false"></iframe>
    <script type="text/javascript">
        (function($) {
            // Add a class to parent body immediately to indicate PB will be visible
            // This is useful to make room for PB placeholder
            var parentBody = window.parent.document.body;
            var parentBody$ = $(parentBody);
            parentBody$.addClass('personabar-visible');

            $(document.body).ready(function() {
                var w = window,
                    d = document,
                    e = d.documentElement,
                    g = d.getElementsByTagName('body')[0],
                    x = w.innerWidth || e.clientWidth || g.clientWidth,
                    desktopIframe = document.getElementById('personaBar-iframe');

                w["personaBarSettings"] = <%=PersonaBarSettings%>;

                var src = '<%= AppPath %>/DesktopModules/admin/Dnn.PersonaBar/index.html';
                src += '?cdv=' + '<%= BuildNumber %>';

                desktopIframe.src = src;

                if (/iPad/i.test(navigator.userAgent)) {
                    $(desktopIframe)
                        .addClass('ipad')
                        .width(x)
                        .wrap('<div class="pb-scroll-wrapper"></div>');
                }
            });
        })(jQuery);
    </script>
</asp:Panel>
