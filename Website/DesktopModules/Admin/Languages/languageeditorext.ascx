<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Languages.LanguageEditorExt" CodeFile="LanguageEditorExt.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" TagName="TextEditor" Src="~/controls/TextEditor.ascx"%>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnLanguageTextEditor">
	<div class="dnnFormItem">
		<dnn:Label id="plFile" runat="server" ControlName="lblFile" />
		<asp:Label id="lblFile" runat="server" />
	</div>
	<div class="dnnFormItem">
		<dnn:Label id="plName" runat="server" ControlName="lblName" />
		<asp:Label id="lblName" runat="server" />
	</div>
	<div class="dnnFormItem">
		<dnn:Label id="plDefault" runat="server" ControlName="lblDefault" />
		<asp:Label id="lblDefault" runat="server" />
	</div>
	<div class="dnnLTextEditor dnnClear"><dnn:texteditor id="teContent" runat="server" height="400" width="600" /></div>
</div>
<ul class="dnnActions dnnClear">
    <li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
    <li><asp:LinkButton id="cmdCancel" runat="server" CssClass="dnnSecondaryAction" ResourceKey="cmdCancel" CausesValidation="false" /></li>
   <%-- <li><dnn:CommandButton ID="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" IconKey="Save" /></li>
    <li><dnn:CommandButton ID="cmdCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" IconKey="Lt" CausesValidation="false" /></li>--%>
</ul>