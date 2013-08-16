<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.Extensions" CodeFile="Extensions.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.WebControls" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>                
<%@ Register TagPrefix="dnn" TagName="InstalledExtensions" Src="~/DesktopModules/Admin/Extensions/InstalledExtensions.ascx" %>                
<%@ Register TagPrefix="dnn" TagName="AvailableExtensions" Src="~/DesktopModules/Admin/Extensions/AvailableExtensions.ascx" %>                
<%@ Register TagPrefix="dnn" TagName="PurchasedExtensions" Src="~/DesktopModules/Admin/Extensions/PurchasedExtensions.ascx" %>    
<%@ Register TagPrefix="dnn" TagName="MoreExtensions" Src="~/DesktopModules/Admin/Extensions/MoreExtensions.ascx" %> 
<script type="text/javascript">
	/*globals jQuery, window, Sys */
	(function ($, Sys) {
		function setUpDnnExtensions() {
			//$('#dnnExtensions').dnnTabs().tabs('select', window.location.hash).dnnPanels();
			$('#dnnExtensions').dnnTabs().tabs().dnnPanels();
			$('#availableExtensions .dnnFormExpandContent a').dnnExpandAll({
			    expandText: '<%=Localization.GetSafeJSString("ExpandAll", Localization.SharedResourceFile)%>',
			    collapseText: '<%=Localization.GetSafeJSString("CollapseAll", Localization.SharedResourceFile)%>',
				targetArea: '#availableExtensions'
			});
			$('#installedExtensions .dnnFormExpandContent a').dnnExpandAll({
			    expandText: '<%=Localization.GetSafeJSString("ExpandAll", Localization.SharedResourceFile)%>',
			    collapseText: '<%=Localization.GetSafeJSString("CollapseAll", Localization.SharedResourceFile)%>',
				targetArea: '#installedExtensions'
			});
		}
		$(document).ready(function () {
			setUpDnnExtensions();
			Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
				setUpDnnExtensions();
			});

			if (location.hash) {
				$("a[href=" + location.hash + "]").click();
			}
		});
	} (jQuery, window.Sys));
</script>   
<ul class="dnnActions dnnRight dnnClear">
	<li><asp:Hyperlink id="cmdInstall" runat="server" CssClass="dnnPrimaryAction" resourcekey="ExtensionInstall.Action"  /></li>
	<li><dnn:ActionLink id="createExtensionLink" runat="server" ControlKey="NewExtension" CssClass="dnnSecondaryAction" resourcekey="CreateExtension.Action" /></li>
	<li><dnn:ActionLink id="createModuleLink" runat="server" ControlKey="EditModuleDefinition" CssClass="dnnSecondaryAction" resourcekey="CreateModule.Action" /></li>
</ul>
<div class="dnnForm dnnExtensions dnnClear" id="dnnExtensions">
	<ul class="dnnAdminTabNav dnnClear">
		<li id="installedExtensionsTab" runat="server" visible="false"><a href="#installedExtensions"><%=LocalizeString("InstalledExtensions")%></a></li>
		<li id="availableExtensionsTab" runat="server" visible="false"><a href="#availableExtensions"><%=LocalizeString("AvailableExtensions")%></a></li>
		<li id="purchasedExtensionsTab" runat="server" visible="false"><a href="#purchasedExtensions"><%=LocalizeString("PurchasedExtensions")%></a></li>
		<li id="moreExtensionsTab" runat="server" visible="false"><a href="#moreExtensions"><%=LocalizeString("MoreExtensions")%></a></li>
	</ul>
	<div id="installedExtensions" class="exInstalledExtensions dnnClear">
		<div class="dnnFormExpandContent"><a href=""><%=Localization.GetString("ExpandAll", Localization.SharedResourceFile)%></a></div>
		<dnn:InstalledExtensions id="installedExtensionsControl" runat="Server"/>
	</div>
	<div id="availableExtensions" class="exAvailableExtensions dnnClear">
		<div id="availableExtensionsTabExpand" runat="server" Visible="false" class="dnnFormExpandContent"><a href=""><%=Localization.GetString("ExpandAll", Localization.SharedResourceFile)%></a></div>
		<div class="exaeContent dnnClear">
			<dnn:AvailableExtensions id="availableExtensionsControl" runat="Server" Visible="false"/>
		</div>
	</div>
    <div id="purchasedExtensions" class="exPurchasedExtensions dnnClear">
		<div class="exmeContent dnnClear">
		    <dnn:PurchasedExtensions id="purchasedExtensionsControl" runat="Server" Visible="false"/>
		</div>
    </div>
	<div id="moreExtensions" class="exMoreExtensions dnnClear">
		<div class="exmeContent dnnClear">
		    <dnn:MoreExtensions id="moreExtensionsControl" runat="Server" Visible="false"/>
		</div>
	</div>
</div>