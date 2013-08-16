<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Languages.EditLanguage" CodeFile="EditLanguage.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnLanguages dnnClear" id="dnnLanguages">
    <fieldset>
        <div class="dnnFormItem">
            <dnn:Label ID="languageLabel" runat="server" ControlName="languageComboBox" />
            <dnn:DnnLanguageLabel ID="languageLanguageLabel" runat="server"  />
            <dnn:DnnLanguageComboBox ID="languageComboBox" runat="server" LanguagesListType="All" cssClass="dnnLanguageCombo" ShowModeButtons="false" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="fallBackLabel" runat="server" ControlName="fallBackComboBox" />
            <dnn:DnnLanguageLabel ID="fallbackLanguageLabel" runat="server"  />
            <dnn:DnnLanguageComboBox ID="fallBackComboBox" runat="server" LanguagesListType="Supported" cssClass="dnnLanguageCombo" />
        </div>
        <div id="translatorsRow" runat="server" class="dnnFormItem">
            <dnn:Label ID="translatorsLabel" runat="server" ControlName="translatorRoles" />
            <dnn:RolesSelectionGrid  runat="server" ID="translatorRoles" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
    	<li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate"/></li>
    	<li><asp:LinkButton id="cmdDelete" runat="server" CssClass="dnnSecondaryAction" ResourceKey="cmdDelete"/></li>
    	<li><asp:LinkButton id="cmdCancel" runat="server" CssClass="dnnSecondaryAction" ResourceKey="cmdCancel" /></li>
    </ul>
</div>