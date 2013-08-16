<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.Skins.Controls.ModuleMessage" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<asp:Panel id="dnnSkinMessage" runat="server" CssClass="dnnModuleMessage">
    <asp:label id="lblHeading" runat="server" visible="False" enableviewstate="False" CssClass="dnnModMessageHeading" />
    <asp:label id="lblMessage" runat="server" enableviewstate="False" />
</asp:Panel>
<dnn:DnnScriptBlock ID="scrollScript" runat="server">
	<script type="text/javascript">
		jQuery(document).ready(function ($) {
			var $body = window.opera ? (document.compatMode == "CSS1Compat" ? $('html') : $('body')) : $('html,body');
			var scrollTop = $('#<%=dnnSkinMessage.ClientID %>').offset().top - parseInt($(document.body).css("margin-top"));
			$body.animate({ scrollTop: scrollTop }, 'fast');
		});
	</script>
</dnn:DnnScriptBlock>