<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.UnInstall" CodeFile="UnInstall.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnUnInstallExtension dnnClear" id="dnnUnInstallExtension" runat="server">
    <fieldset>
        <dnn:DnnFormEditor id="packageForm" runat="Server" FormMode="Short">
            <Items>
                <dnn:DnnFormLiteralItem ID="name" runat="server" DataField = "Name" />
                <dnn:DnnFormLiteralItem ID="packageType" runat="server" DataField = "PackageType" />
                <dnn:DnnFormLiteralItem ID="friendlyName" runat="server" DataField = "FriendlyName"/>
                <dnn:DnnFormLiteralItem ID="description" runat="server" DataField = "Description" />
                <dnn:DnnFormLiteralItem ID="version" runat="server" DataField = "Version" />
                <dnn:DnnFormLiteralItem ID="license" runat="server" DataField = "License" />
            </Items>
        </dnn:DnnFormEditor>   
        <div id="deleteRow" runat="server" class="dnnFormItem">
            <dnn:Label ID="plDelete" runat="server" ControlName="rbPackageType" />
            <asp:CheckBox ID="chkDelete" runat="server" />
        </div>    
    </fieldset>
    <ul class="dnnActions dnnClear">
    	<li><asp:LinkButton id="cmdUninstall" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUninstall" /></li>
        <li><asp:HyperLink id="cmdReturn1" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdReturn" Causesvalidation="False" /></li>
    </ul>
</div>
<div class="dnnClear"><asp:Label ID="lblMessage" runat="server" EnableViewState="False" /></div>
<div id="tblLogs" runat="server" visible="False">
	<h2 class="dnnFormSectionHead"><asp:Label ID="lblLogTitle" runat="server" resourcekey="LogTitle" /></h2>
	<div class="dnnClear"><asp:PlaceHolder ID="phPaLogs" runat="server" /></div>
    <ul class="dnnActions dnnClear"><li><asp:HyperLink ID="cmdReturn2" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdReturn" /></li></ul>
</div>
<dnn:DnnScriptBlock ID="scriptBlock1" runat="server">
	<script type="text/javascript">
		/*globals jQuery */
		(function ($) {
		    var yesText = '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>';
		    var noText = '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>';
		    var titleText = '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>';
			$('#<%= cmdUninstall.ClientID %>').dnnConfirm({
			    text: '<%= Localization.GetSafeJSString("DeleteItem.Text", Localization.SharedResourceFile) %>',
				yesText: yesText,
				noText: noText,
				title: titleText
			});
		} (jQuery));
	</script>
</dnn:DnnScriptBlock>