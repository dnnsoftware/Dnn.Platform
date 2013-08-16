<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.EditExtension" CodeFile="EditExtension.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Security.Permissions.Controls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" TagName="Audit" Src="~/controls/ModuleAuditControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div class="dnnForm dnnEditExtension dnnClear" id="dnnEditExtension">
    <div id="extensionSection" runat="server"><asp:PlaceHolder ID="phEditor" runat="server" /></div>
    <h2 class="dnnFormSectionHead" id="dnnPanel-ExtensionPackageSettings"><a href="" class="dnnLabelExpanded"><%=LocalizeString("PackageSettings")%></a></h2>
    <fieldset>
        <asp:Label ID="lblHelp" runat="server" />
        <div id="trLanguagePackType" runat="server" class="dnnFormItem">
            <dnn:Label ID="plPackageType" runat="server" ControlName="rbPackageType" />
            <asp:RadioButtonList ID="rbPackageType" runat="server" RepeatDirection="Horizontal">
                <asp:ListItem Value="Core" resourcekey="Core" />
                <asp:ListItem Value="Package" resourcekey="Package" />
            </asp:RadioButtonList>
            <asp:RequiredFieldValidator ID="valPackageType" runat="server" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ControlToValidate="rbPackageType" ResourceKey="PackageType.Error" />
        </div>
        <dnn:DnnFormEditor id="packageForm" runat="Server" FormMode="Short">
            <Items>
                <dnn:DnnFormLiteralItem ID="moduleName" runat="server" DataField = "Name" />
                <dnn:DnnFormLiteralItem ID="packageType" runat="server" DataField = "PackageType" />
                <dnn:DnnFormTextBoxItem ID="packageFriendlyName" runat="server" DataField = "FriendlyName" Required="true" />
                <dnn:DnnFormTextBoxItem ID="iconFile" runat="server" DataField = "IconFile" />
                <dnn:DnnFormTextBoxItem ID="description" runat="server" DataField = "Description" TextMode="MultiLine" Rows="10" />
                <dnn:DnnFormEditControlItem ID="version" runat="server" DataField = "Version" ControlType="DotNetNuke.UI.WebControls.VersionEditControl, DotNetNuke"/>
                <dnn:DnnFormTextBoxItem ID="license" runat="server" DataField = "License" TextMode="MultiLine" Rows="10" />
                <dnn:DnnFormTextBoxItem ID="releaseNotes" runat="server" DataField = "ReleaseNotes" TextMode="MultiLine" Rows="10" />
                <dnn:DnnFormTextBoxItem ID="owner" runat="server" DataField = "Owner" />
                <dnn:DnnFormTextBoxItem ID="organization" runat="server" DataField = "Organization" />
                <dnn:DnnFormTextBoxItem ID="url" runat="server" DataField = "Url" />
                <dnn:DnnFormTextBoxItem ID="email" runat="server" DataField = "Email" />
            </Items>
        </dnn:DnnFormEditor>
        <dnn:DnnFormEditor id="packageFormReadOnly" runat="Server" FormMode="Short">
            <Items>
                <dnn:DnnFormLiteralItem ID="DnnFormLiteralItem1" runat="server" DataField = "Name" />
                <dnn:DnnFormLiteralItem ID="DnnFormLiteralItem2" runat="server" DataField = "PackageType" />
                <dnn:DnnFormLiteralItem ID="DnnFormTextBoxItem1" runat="server" DataField = "FriendlyName"/>
                <dnn:DnnFormLiteralItem ID="DnnFormTextBoxItem2" runat="server" DataField = "IconFile" />
                <dnn:DnnFormLiteralItem ID="DnnFormTextBoxItem3" runat="server" DataField = "Description" />
                <dnn:DnnFormLiteralItem ID="DnnFormEditControlItem1" runat="server" DataField = "Version" />
                <dnn:DnnFormLiteralItem ID="DnnFormTextBoxItem4" runat="server" DataField = "License" />
                <dnn:DnnFormLiteralItem ID="DnnFormTextBoxItem5" runat="server" DataField = "ReleaseNotes" />
                <dnn:DnnFormLiteralItem ID="DnnFormTextBoxItem6" runat="server" DataField = "Owner" />
                <dnn:DnnFormLiteralItem ID="DnnFormTextBoxItem7" runat="server" DataField = "Organization" />
                <dnn:DnnFormLiteralItem ID="DnnFormTextBoxItem8" runat="server" DataField = "Url" />
                <dnn:DnnFormLiteralItem ID="DnnFormTextBoxItem9" runat="server" DataField = "Email" />
            </Items>
        </dnn:DnnFormEditor>    
    </fieldset>
    <ul class="dnnActions dnnClear">
    	<li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
        <li><asp:LinkButton id="cmdDelete" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdDelete" Causesvalidation="False" /></li>
        <li><asp:LinkButton id="cmdPackage" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdPackage" Causesvalidation="False" /></li>
        <li><asp:LinkButton id="cmdCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" Causesvalidation="False" /></li>
    </ul>
</div>
<dnn:audit id="ctlAudit" runat="server" />
<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
    function setUpDnnExtensions() {
        $('#dnnEditExtension').dnnPanels();
    }
    $(document).ready(function () {
        setUpDnnExtensions();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            setUpDnnExtensions();
        });
    });
} (jQuery, window.Sys));
</script>