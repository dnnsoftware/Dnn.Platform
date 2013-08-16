<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Modules.Export" CodeFile="Export.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnExport dnnClear" id="dnnLogViewer">
    <fieldset>
		<div class="dnnFormItem">
            <dnn:label id="plFolder" runat="server" controlname="cboFolders" suffix=":" />
            <dnn:DnnFolderDropDownList ID="cboFolders" runat="server" />
        </div>
		<div class="dnnFormItem">
            <dnn:label id="plFile" runat="server" controlname="txtFile" suffix=":" />
            <asp:textbox id="txtFile" runat="server" maxlength="200" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton id="cmdExport" resourcekey="cmdExport" runat="server" cssclass="dnnPrimaryAction" /></li>
        <li><asp:HyperLink id="cmdCancel" resourcekey="cmdCancel" runat="server" cssclass="dnnSecondaryAction" causesvalidation="False" /></li>
    </ul>
</div>