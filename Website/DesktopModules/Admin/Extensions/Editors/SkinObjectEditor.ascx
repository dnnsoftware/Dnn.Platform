<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.SkinObjects.SkinObjectEditor" CodeFile="SkinObjectEditor.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<h2 class="dnnFormSectionHead" id="moduleSettingsHead" runat='server'><a href="" class="dnnLabelExpanded"><%=LocalizeString("Title")%></a></h2>
<fieldset>
    <div id ="helpPanel" runat="server" class="dnnFormMessage dnnFormInfo"><asp:Label ID="lblHelp" runat="server" resourcekey="Help" /></div>
    <dnn:DnnFormEditor id="skinObjectForm" runat="Server" FormMode="Short">
        <Items>
            <dnn:DnnFormTextBoxItem ID="controlKey" runat="server" DataField = "ControlKey" />
            <dnn:DnnFormTextBoxItem ID="controlSrc" runat="server" DataField = "ControlSrc" />
            <dnn:DnnFormToggleButtonItem ID="supportsPartialRendering" runat="server" DataField = "SupportsPartialRendering" />
        </Items>
    </dnn:DnnFormEditor>
    <dnn:DnnFormEditor id="skinObjectFormReadOnly" runat="Server" FormMode="Short">
        <Items>
            <dnn:DnnFormLiteralItem ID="controlKeyReadOnly" runat="server" DataField = "ControlKey" />
            <dnn:DnnFormLiteralItem ID="controlSrcReadOnly" runat="server" DataField = "ControlSrc" />
            <dnn:DnnFormLiteralItem ID="supportsPartialRenderingReadOnly" runat="server" DataField = "SupportsPartialRendering"  />
        </Items>
    </dnn:DnnFormEditor>
</fieldset>