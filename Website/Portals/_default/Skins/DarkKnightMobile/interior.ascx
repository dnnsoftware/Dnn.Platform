<%@ Control language="vb" AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Skins.Skin" %>
<%@ Register TagPrefix="dnn" TagName="LOGO" Src="~/Admin/Skins/Logo.ascx" %>
<%@ Register TagPrefix="dnn" TagName="PRIVACY" Src="~/Admin/Skins/Privacy.ascx" %>
<%@ Register TagPrefix="dnn" TagName="TERMS" Src="~/Admin/Skins/Terms.ascx" %>
<%@ Register TagPrefix="dnn" TagName="COPYRIGHT" Src="~/Admin/Skins/Copyright.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Meta" Src="~/Admin/Skins/Meta.ascx" %>
<%@ Register TagPrefix="dnn" TagName="LINKTOFULLSITE" Src="~/Admin/Skins/LinkToFullSite.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.DDRMenu.TemplateEngine" Assembly="DotNetNuke.Web.DDRMenu" %>
<%@ Register TagPrefix="dnn" TagName="MENU" src="~/DesktopModules/DDRMenu/Menu.ascx" %>
<dnn:Meta runat="server" Name="viewport" Content="initial-scale=1.0,width=device-width" /> 
<dnn:Meta runat="server" Name="robots" Content="noindex" />
<dnn:Meta runat="server" Name="dnncrawler" Content="doindex" />
<div class="dnnPE-Mobile interior">
	<div class="dnnPEM-Header dnnClear">
        <div class="dnnPEM-Logo dnnLeft"><dnn:LOGO id="dnnLogo" runat="server" /></div>
        <a href="" class="navTrigger dnnRight"><span>Menu</span></a>
	</div>
	<div><dnn:MENU MenuStyle="DNNMobileNav" runat="server"></dnn:MENU></div>
    <div class="dnnPEM-Body dnnClear"><div id="ContentPane" runat="server"></div></div>
	<dnn:MENU MenuStyle="DNNMobileSubNav" NodeSelector="CurrentChildren" runat="server"></dnn:MENU>
    <div class="dnnPEM-Footer dnnClear">
	    <div id="SocialPane" runat="server" class="socialIcns"></div>
    	<p class="dnnPEM-Copyright">
        	<dnn:COPYRIGHT ID="dnnCopyright" runat="server" /><br />
            <dnn:TERMS ID="dnnTerms" runat="server" />. <dnn:PRIVACY ID="dnnPrivacy" runat="server" /><br />
            <dnn:LINKTOFULLSITE ID="dnnLinkToFullSite" runat="server" />
		</p>
    </div>
</div>
<script type="text/javascript">
	(function ($) {
		$("a.navTrigger").click(function (event) {
			event.preventDefault();
			$("ul.dnnPEM-GlobalNav").slideToggle("9000");
		});
	})(jQuery);		
</script>
<script>
	(function ($) {
		$("ul.dnnPEM-SubNav").not($("ul.dnnPEM-SubNav").has("li")).css("display", "none");
	})(jQuery);
</script>
<script type="text/javascript">
	(function ($) {
		if ($("a.navTrigger").is(":visible") === true) {
			$("ul.dnnPEM-SubNav").insertAfter("div.dnnPEM-Body");
		}
		else {
			$("ul.dnnPEM-SubNav").appendTo("div.dnnPEM-Header");
			$("ul.dnnPEM-SubNav").attr("class", "tabletSubNav");
		}
		$(window).resize(function () {
			if ($("a.navTrigger").is(":visible") === true) {
				$("ul.tabletSubNav").insertAfter("div.dnnPEM-Body");
				$("ul.tabletSubNav").attr("class", "dnnPEM-SubNav");
			}
			else {
				$("ul.dnnPEM-SubNav").appendTo("div.dnnPEM-Header");
				$("ul.dnnPEM-SubNav").attr("class", "tabletSubNav");
			}
		});
	})(jQuery);
</script>