<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register Assembly="DotNetNuke.WebControls" Namespace="DotNetNuke.UI.WebControls" TagPrefix="DNN" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Vendors.BannerOptions" CodeFile="BannerOptions.ascx.cs" %>
<div class="dnnForm dnnBannerOptions dnnClear">
    <div class="dnnFormItem">
        <dnn:label id="plSource" runat="server" controlname="optSource" suffix=":" />
        <asp:RadioButtonList id="optSource" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" CssClass="dnnFormRadioButtons">
			<asp:ListItem Value="G" resourcekey="Host">Host</asp:ListItem>
			<asp:ListItem Value="L" resourcekey="Site">Site</asp:ListItem>
		</asp:RadioButtonList>
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plType" runat="server" controlname="cboType" suffix=":" />
        <asp:DropDownList ID="cboType" Runat="server" DataTextField="BannerTypeName" DataValueField="BannerTypeId" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plGroup" runat="server" controlname="DNNTxtBannerGroup" suffix=":" />
        <dnn:DNNTextSuggest id="DNNTxtBannerGroup" runat="server" Columns="30" LookupDelay="500" MaxLength="100" TextSuggestCssClass="SuggestTextMenu GroupSuggestMenu" DefaultNodeCssClassOver="SuggestNodeOver" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plCount" runat="server" controlname="txtCount" suffix=":" />
        <asp:TextBox id="txtCount" Runat="server" />
        <asp:RegularExpressionValidator id="valCount" ControlToValidate="txtCount" ValidationExpression="^[0-9]*$" Display="Dynamic" resourcekey="valCount.ErrorMessage" runat="server" CssClass="dnnFormMessage dnnFormError" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plOrientation" runat="server" controlname="optOrientation" suffix=":" />
        <asp:RadioButtonList id="optOrientation" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" CssClass="dnnFormRadioButtons">
			<asp:ListItem Value="V" resourcekey="Vertical">Vertical</asp:ListItem>
			<asp:ListItem Value="H" resourcekey="Horizontal">Horizontal</asp:ListItem>
		</asp:RadioButtonList>
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plBorder" runat="server" controlname="txtBorder" suffix=":" />
        <asp:TextBox id="txtBorder" Runat="server" />
        <asp:RegularExpressionValidator id="valBorder" ControlToValidate="txtBorder" ValidationExpression="^[0-9]*$" Display="Dynamic" resourcekey="valBorder.ErrorMessage" runat="server" CssClass="dnnFormMessage dnnFormError" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plBorderColor" runat="server" controlname="txtBorderColor" suffix=":" />
        <asp:TextBox id="txtBorderColor" Runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plPadding" suffix=":" controlname="txtPadding" runat="server" />
        <asp:TextBox id="txtPadding" Runat="server" />
        <asp:CompareValidator id="valPadding" runat="server" resourcekey="valPadding.ErrorMessage" 	Display="Dynamic" ControlToValidate="txtPadding" Operator="DataTypeCheck" Type="Integer" CssClass="dnnFormMessage dnnFormError" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plRowHeight" runat="server" controlname="txtRowHeight" suffix=":" />
        <asp:TextBox id="txtRowHeight" Runat="server" />
        <asp:RegularExpressionValidator id="valRowHeight" ControlToValidate="txtRowHeight" ValidationExpression="^[0-9]*$" Display="Dynamic" resourcekey="valRowHeight.ErrorMessage" runat="server" CssClass="dnnFormMessage dnnFormError" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plColWidth" runat="server" controlname="txtColWidth" suffix=":" />
        <asp:TextBox id="txtColWidth" Runat="server" />
        <asp:RegularExpressionValidator id="valColWidth" ControlToValidate="txtColWidth" ValidationExpression="^[0-9]*$" Display="Dynamic" resourcekey="valColWidth.ErrorMessage" runat="server" CssClass="dnnFormMessage dnnFormError" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plBannerClickThroughURL" runat="server" controlname="txtBannerClickThroughURL" suffix=":" />
        <asp:TextBox id="txtBannerClickThroughURL" Runat="server" />
        <asp:RegularExpressionValidator id="valBannerClickThroughURL" ControlToValidate="txtBannerClickThroughURL" ValidationExpression="(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?" Display="Dynamic" resourcekey="valBannerClickThroughURL.ErrorMessage" runat="server" CssClass="dnnFormMessage dnnFormError" />
    </div>
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton id="cmdUpdate" Text="Update" resourcekey="cmdUpdate" runat="server" class="dnnPrimaryAction" /></li>
        <li><asp:LinkButton id="cmdCancel" Text="Cancel" resourcekey="cmdCancel" CausesValidation="False" runat="server" class="dnnSecondaryAction" /></li>
    </ul>
</div>