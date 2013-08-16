<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Host.RequestFilters" CodeFile="RequestFilters.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div id="lblErr" class="NormalRed" runat="server" visible="false"></div>
<asp:DataList ID="rptRules" runat="server" CssClass="dnnRequestFilter">
	<HeaderStyle CssClass="" />
    <FooterStyle CssClass="" />
    <ItemTemplate>
        <table width="100%">
            <tr>
                <td rowspan="5" valign="top">
                    <dnn:DnnImageButton ID="cmdEdit" runat="server" CommandName="Edit" IconKey="Edit" CausesValidation="false" />
                    <dnn:DnnImageButton ID="cmdDelete" runat="server" CommandName="Delete" IconKey="Delete" CausesValidation="false" />
                </td>
                <td width="188"><dnn:label id="plServerVar" runat="server" controlname="lblServerVar" suffix=":" /></td>
                <td ><asp:label runat="server" Text='<%#Eval("ServerVariable") %>' ID="lblServerVar" /></td>
            </tr>
            <tr>
                <td><dnn:label id="plOperator" runat="server" controlname="lblOperator" suffix=":" /></td>
                <td><asp:label runat="server" Text='<%#Eval("Operator") %>' ID="lblOperator" /></td>
            </tr>
            <tr>
                <td><dnn:label id="plValue" runat="server" controlname="lblValue" suffix=":" /></td>
                <td><asp:label runat="server" Text='<%#Eval("RawValue") %>' ID="lblValue" /></td>
            </tr>
            <tr>
                <td><dnn:label id="plAction" runat="server" controlname="lblAction" suffix=":" /></td>
                <td><asp:label runat="server" Text='<%#Eval("Action") %>' ID="lblAction" /></td>
            </tr>
            <tr>
                <td><dnn:label id="plLocation" runat="server" controlname="lblLocation" suffix=":" /></td>
                <td><asp:label runat="server" Text='<%#Eval("Location") %>' ID="lblLocation" /></td>
            </tr>
        </table>
    </ItemTemplate>
    <EditItemTemplate>
        <fieldset>
	        <div class="dnnFormMessage dnnFormWarning"><asp:Label ID="lblWarning" runat="server" Text="Simple warning" resourcekey="lblWarning" /></div>
			<div class="dnnFormItem">
				<dnn:label id="plServerVar" runat="server" controlname="txtServerVar" suffix=":" />
				<asp:TextBox ID="txtServerVar" runat="server" Text='<%#Eval("ServerVariable") %>' />
                <asp:Label ID="lblServerVarLink" runat="server" text="Simple Link" resourcekey="lblServerVarLink" />
			</div>
			<div class="dnnFormItem">
				<dnn:label id="plOperator" runat="server" controlname="ddlOperator" suffix=":" />
				<dnn:DnnComboBox ID="ddlOperator" runat="server">
					<Items>
						<dnn:DnnComboBoxItem Value="Equal" Text="Equal"></dnn:DnnComboBoxItem>
						<dnn:DnnComboBoxItem Value="NotEqual" Text="NotEqual"></dnn:DnnComboBoxItem>
						<dnn:DnnComboBoxItem Value="Regex" Text="Regex"></dnn:DnnComboBoxItem>
					</Items>
                </dnn:DnnComboBox>
			</div>
			<div class="dnnFormItem">
				<dnn:label id="plValue" runat="server" controlname="txtValue" suffix=":" />
				<asp:TextBox ID="txtValue" runat="server" Text='<%#Eval("RawValue") %>' />
			</div>
			<div class="dnnFormItem">
				<dnn:label id="plAction" runat="server" controlname="ddlAction" suffix=":" />
				<dnn:DnnComboBox ID="ddlAction" runat="server">
					<Items>
						<dnn:DnnComboBoxItem Value="Redirect" Text="Redirect"></dnn:DnnComboBoxItem>
						<dnn:DnnComboBoxItem Value="PermanentRedirect" Text="PermanentRedirect"></dnn:DnnComboBoxItem>
						<dnn:DnnComboBoxItem Value="NotFound" Text="NotFound"></dnn:DnnComboBoxItem>
					</Items>
                </dnn:DnnComboBox>
			</div>
			<div class="dnnFormItem">
				<dnn:label id="plLocation" runat="server" controlname="txtLocation" suffix=":" />
				<asp:TextBox ID="txtLocation" runat="server" Text='<%#Eval("Location") %>' />
			</div>
			<ul class="dnnActions dnnClear">
				<asp:LinkButton ID="cmdSave" runat="server" CommandName="Update" ResourceKey="Save" CssClass="dnnPrimaryAction" />
                <asp:LinkButton ID="cmdDelete" runat="server" CommandName="Cancel" ResourceKey="Cancel" CssClass="dnnSecondaryAction" CausesValidation="false" />
			</ul>
		</fieldset>
    </EditItemTemplate>
    <SeparatorTemplate><div></div></SeparatorTemplate>
</asp:DataList>
<ul class="dnnActions rfAddRule dnnClear"><li><asp:LinkButton ID="cmdAddRule" runat="server" resourcekey="cmdAdd" CssClass="dnnPrimaryAction" CausesValidation="false" /></li></ul>