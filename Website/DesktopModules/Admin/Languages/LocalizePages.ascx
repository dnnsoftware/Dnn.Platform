<%@ Control Language="C#" AutoEventWireup="false" CodeFile="LocalizePages.ascx.cs" Inherits="DotNetNuke.Modules.Admin.Languages.LocalizePages" Explicit="True" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnLanguages dnnClear" id="dnnLanguages">
    <fieldset>
        <div class="dnnFormItem">
            <h2 class="dnnFormSectionHead">
                <asp:Label ID="headerLabel" runat="server" resourceKey="header" /></h2>
        </div>
        <div class="dnnFormMessage dnnFormInfo">
            <%= String.Format(Localization.GetString("localize", LocalResourceFile), Locale, PortalSettings.DefaultLanguage)%>
        </div>

        <div class="dnnFormItem">
            <dnn:Label ID="siteDefaultLabel" runat="server" resourceKey="siteDefaultLabel" />
            <dnn:dnnlanguagelabel id="defaultLanguageLabel" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="PagesToLocalizeLabel" runat="server" />
            <asp:Label runat="server" ID="PagesToLocalize"></asp:Label>
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="updateButton" runat="server" CssClass="dnnPrimaryAction" resourcekey="Update" /></li>
        <li>
            <asp:LinkButton ID="cancelButton" runat="server" CssClass="dnnSecondaryAction" ResourceKey="Cancel" /></li>
    </ul>
</div>
<div class="enableLocalizationProgress" style="text-align: center">
    <dnn:dnnprogressmanager id="progressManager" runat="server" />
    <dnn:dnnprogressarea id="pageCreationProgressArea" runat="server" timeelapsed="true" />
</div>
<dnn:dnnscriptblock id="scriptBlock" runat="server">
	<script type="text/javascript">
	    (function ($) {
	        $("#<%=updateButton.ClientID %>").click(function (e) {
	            $(this).hide();
	        });
	    }(jQuery));
	</script>
</dnn:dnnscriptblock>
