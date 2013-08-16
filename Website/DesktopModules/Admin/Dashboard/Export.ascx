<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Dashboard.Export" CodeFile="Export.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnDashboardExport dnnClear" id="dnnDashboardExport">
    <fieldset>
        <div class="dnnFormItem">
            <dnn:label id="plFileName" runat="server" controlname="txtFileName" />
            <asp:textbox id="txtFileName" runat="server" maxlength="200"  />&nbsp;.xml
            <asp:RequiredFieldValidator ID="valFileName" Display="Dynamic" CssClass="dnnFormMessage dnnFormError" resourcekey="valFileName.Error" ControlToValidate="txtFileName" runat="server" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear" >
        <li><asp:LinkButton id="cmdSave" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdSave" /></li>
        <li><asp:HyperLink id="cmdCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
    </ul>  
</div>