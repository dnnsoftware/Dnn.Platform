<%@ Control Inherits="DotNetNuke.Modules.Html.Settings" CodeBehind="Settings.ascx.cs" language="C#" AutoEventWireup="false" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnncl" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<dnncl:DnnCssInclude ID="customJS" runat="server" FilePath="DesktopModules/HTML/edit.css" AddTag="false" />

<div class="dnnForm dnnHTMLSettings dnnClear">
	<div class="dnnFormItem">
		<dnn:label id="plReplaceTokens" controlname="chkReplaceTokens" runat="server" />
		<asp:CheckBox ID="chkReplaceTokens" runat="server" />
	</div>
	<div class="dnnFormItem">
		<dnn:label id="plDecorate" controlname="cbDecorate" runat="server" />
		<asp:CheckBox ID="cbDecorate" runat="server" />
	</div>
		<div class="dnnFormItem">
        <dnn:label id="plSearchDescLength" runat="server" controlname="txtSearchDescLength" />
        <asp:TextBox ID="txtSearchDescLength" runat="server" />
        <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="txtSearchDescLength"
            Display="Dynamic" CssClass="dnnFormMessage dnnFormError" ValidationExpression="^\d+$" resourcekey="valSearchDescLength.ErrorMessage" />
    </div>
	<div class="dnnFormItem">
		<dnn:label id="plWorkflow" controlname="cboWorkflow" runat="server" suffix=":" />		
        <dnn:DnnComboBox ID="cboWorkflow" runat="server" DataTextField="WorkflowName" DataValueField="WorkflowID" AutoPostBack="True" />
	</div>
	<div class="dnnFormMessage dnnFormInfo">
		<asp:Label ID="lblDescription" runat="server" />
	</div>
	<div class="dnnFormItem" id="divApplyTo" runat="server">
		<dnn:label id="plApplyTo" controlname="rblApplyTo" runat="server" />
		<asp:RadioButtonList ID="rblApplyTo" runat="server" RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons">
			<asp:ListItem Value="Module" ResourceKey="Module" />
			<asp:ListItem Value="Page" ResourceKey="Page" />
			<asp:ListItem Value="Site" ResourceKey="Site" />
		</asp:RadioButtonList>
		<asp:CheckBox ID="chkReplace" runat="server" resourcekey="chkReplace" CssClass="inline" />
	</div>
</div>