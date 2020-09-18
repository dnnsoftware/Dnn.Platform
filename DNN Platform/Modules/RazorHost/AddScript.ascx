<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="AddScript.ascx.cs" Inherits="DotNetNuke.Modules.RazorHost.AddScript" %>
<%@ Register Assembly="DotnetNuke" Namespace="DotNetNuke.UI.WebControls" TagPrefix="dnn" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnRazorHost dnnClear" id="dnnRazorHost">
    <fieldset>
        <legend></legend>
        <div class="dnnFormItem">
            <asp:Label ID="createNewScriptLabel" runat="server" resourceKey="createNewScript" />
        </div>

        <div class="dnnFormItem">
            <dnn:Label id="fileTypeLabel" runat="Server"/>
            <asp:DropDownList ID="scriptFileType" runat="server" RepeatDirection="Horizontal" AutoPostBack="true">
				<asp:ListItem Value="CSHTML" ResourceKey="CSHTML" Selected="True" />
				<asp:ListItem Value="VBHTML" ResourceKey="VBHTML" />
			</asp:DropDownList>
        </div>

        <div class="dnnFormItem">
            <dnn:Label id="fileNameLabel" runat="Server"/>
			<asp:TextBox ID="fileName" runat="server" width="150px" />
			<asp:Label ID="fileExtension" runat="server" />
        </div>
    </fieldset>

    <ul class="dnnActions dnnClear">
        <li><asp:linkbutton id="cmdAdd" resourcekey="cmdAdd" runat="server" cssclass="PrimaryAction"/></li>
        <li><asp:linkbutton id="cmdCancel" resourcekey="cmdCancel" runat="server" cssclass="SecondaryAction" CausesValidation="false" /></li>
    </ul>

</div>




