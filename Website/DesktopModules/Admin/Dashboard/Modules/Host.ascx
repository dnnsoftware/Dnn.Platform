<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Dashboard.Controls.Host" CodeFile="Host.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<div class="dnnForm dnnHost dnnClear">
    <dnn:propertyeditorcontrol id="ctlHost" runat="Server"
        autogenerate = "false"
        enableclientvalidation = "true"
        sortmode="SortOrderAttribute" 
        helpstyle-cssclass="dnnFormHelpContent dnnClear" 
        editmode="Edit" 
        errorstyle-cssclass="dnnFormMessage dnnFormError">
        <Fields>
            <dnn:FieldEditorControl ID="fldProduct" runat="server" DataField="Product" />
            <dnn:FieldEditorControl ID="fldVersion" runat="server" DataField="Version" />
            <dnn:FieldEditorControl ID="fldHostGUID" runat="server" DataField="HostGUID" />
            <dnn:FieldEditorControl ID="fldPermissions" runat="server" DataField="Permissions" />
            <dnn:FieldEditorControl ID="fldDataProvider" runat="server" DataField="DataProvider" />
            <dnn:FieldEditorControl ID="fldCachingProvider" runat="server" DataField="CachingProvider" />
            <dnn:FieldEditorControl ID="fldLoggingProvider" runat="server" DataField="LoggingProvider" />
            <dnn:FieldEditorControl ID="fldHtmlEditorProvider" runat="server" DataField="HtmlEditorProvider" />
            <dnn:FieldEditorControl ID="fldFriendlyUrlProvider" runat="server" DataField="FriendlyUrlProvider" />
            <dnn:FieldEditorControl ID="fldFriendlyUrlEnabled" runat="server" DataField="FriendlyUrlEnabled" />
            <dnn:FieldEditorControl ID="fldFriendlyUrlType" runat="server" DataField="FriendlyUrlType" />
            <dnn:FieldEditorControl ID="fldSchedulerMode" runat="server" DataField="SchedulerMode" />
            <dnn:FieldEditorControl ID="fldWebFarmEnabled" runat="server" DataField="WebFarmEnabled" />
            <dnn:FieldEditorControl ID="fldjQueryVersion" runat="server" DataField="JQueryVersion" />
            <dnn:FieldEditorControl ID="fldjQueryUIVersion" runat="server" DataField="JQueryUIVersion" />
        </Fields>
    </dnn:propertyeditorcontrol>
</div>