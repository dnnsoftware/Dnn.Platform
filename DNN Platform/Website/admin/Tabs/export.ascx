<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Tabs.Export" Codebehind="Export.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnTabExport dnnClear">
    <div class="dnnFormItem">
        <dnn:label id="plFolder" runat="server" controlname="cboFolders" />
        <dnn:DnnFolderDropDownList ID="cboFolders" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plFile" runat="server" controlname="txtFile" CssClass="dnnFormRequired"  />
        <asp:textbox id="txtFile" runat="server" maxlength="200" />
        <asp:requiredfieldvalidator id="valFileName" runat="server" controltovalidate="txtFile" display="Dynamic" resourcekey="valFileName.ErrorMessage" CssClass="dnnFormMessage dnnFormError" SetFocusOnError="true" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plDescription" runat="server" controlname="txtDescription" CssClass="dnnFormRequired"  />
        <asp:textbox id="txtDescription" runat="server" enableviewstate="False" TextMode="MultiLine" />
        <asp:requiredfieldvalidator id="valDescription" runat="server" controltovalidate="txtDescription" display="Dynamic" resourcekey="valDescription.ErrorMessage" CssClass="dnnFormMessage dnnFormError" SetFocusOnError="true" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plContent" runat="server" controlname="chkContent" />
        <asp:CheckBox id="chkContent" runat="server" />
    </div>
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton id="cmdExport" resourcekey="cmdExport" runat="server" cssclass="dnnPrimaryAction" /></li>
        <li><asp:HyperLink id="cmdCancel" resourcekey="cmdCancel" runat="server" cssclass="dnnSecondaryAction" /></li>
    </ul>
</div>
