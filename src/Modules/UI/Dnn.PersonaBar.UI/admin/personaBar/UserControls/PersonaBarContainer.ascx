<%@ Control language="C#" Inherits="Dnn.PersonaBar.UI.UserControls.PersonaBarContainer" AutoEventWireup="false"  Codebehind="PersonaBarContainer.ascx.cs" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<asp:Panel runat="server" ID="PersonaBarPanel" Visible="False" CssClass="personalBarContainer">
	<iframe id="personaBar-iframe" allowTransparency="true" frameBorder="0" scrolling="false"></iframe>
	<iframe id="personaBar-mobi-iframe" allowTransparency="true" frameBorder="0"></iframe>
    <script type="text/javascript">
        (function($) {
            var w = window,
                d = document,
                e = d.documentElement,
                g = d.getElementsByTagName('body')[0],
                x = w.innerWidth || e.clientWidth || g.clientWidth,
                desktopIframe = document.getElementById('personaBar-iframe'),
                mobileIframe = document.getElementById('personaBar-mobi-iframe');

            w["personaBarSettings"] = <%=PersonaBarSettings%>;

            var mobile = x <= 640;

            var src = '<%= AppPath %>/DesktopModules/admin/Dnn.PersonaBar/index' + (mobile ? '.mobi': '') + '.html';
            src += '?cdv=' + '<%= BuildNumber %>';
            if (mobile) {
                desktopIframe.parentElement.removeChild(desktopIframe);
                mobileIframe.src = src;
                $(mobileIframe).wrap('<div class="pb-scroll-wrapper"></div>');
            } else {
                mobileIframe.parentElement.removeChild(mobileIframe);
                desktopIframe.src = src;

                if (/iPad/i.test(navigator.userAgent)) {
                    $(desktopIframe)
                        .addClass('ipad')
                        .width(x)
                        .wrap('<div class="pb-scroll-wrapper"></div>');
                }
            }
        })(jQuery);
    </script>
</asp:Panel>
