<%@ Control Language="C#" AutoEventWireup="false" CodeFile="TranslationStatus.ascx.cs" Inherits="DotNetNuke.Modules.Admin.Languages.TranslationStatus" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="ModuleLocalization" Src="~/Admin/Modules/ModuleLocalization.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" TagName="TabLocalization" Src="~/DesktopModules/Admin/Tabs/TabLocalization.ascx" %>
<dnn:DnnTreeView ID="pagesTreeView" runat="server" DataFieldID="TabId" DataFieldParentID="ParentId">
    <NodeTemplate>
        <dnn:TabLocalization id="tabLocalization" runat="server" 
            IsSelf="True"
            ShowFooter="false"
            ShowEditColumn="false"
            ShowViewColumn="false"
            ShowLanguageColumn="false" />
        <dnn:ModuleLocalization id="moduleLocalization" runat="server"
            ShowFooter="false"
            ShowEditColumn="false" 
            ShowLanguageColumn="false" />
   </NodeTemplate>
</dnn:DnnTreeView>
<div>
    <dnn:CommandButton ID="localizeModuleButton" resourcekey="unbindModule" runat="server" CssClass="CommandButton"  IconKey="ModuleBind" CausesValidation="False" />&nbsp;&nbsp;&nbsp;
    <dnn:CommandButton ID="delocalizeModuleButton" resourcekey="bindModule" runat="server" CssClass="CommandButton" IconKey="ModuleUnbind" CausesValidation="False" />&nbsp;&nbsp;&nbsp;
    <dnn:CommandButton ID="markModuleTranslatedButton" resourcekey="markModuleTranslated" runat="server" CssClass="CommandButton" IconKey="Translate" CausesValidation="False" />&nbsp;&nbsp;&nbsp;
    <dnn:CommandButton ID="markModuleUnTranslatedButton" resourcekey="markModuleUnTranslated" runat="server" CssClass="CommandButton" IconKey="Untranslate" CausesValidation="False" />
</div>