<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.UserControls.Address" %>
<%@ Register TagPrefix="wc" Namespace="DotNetNuke.UI.WebControls" Assembly="CountryListBox" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>
<div class="dnnForm dnnAddress dnnClear">
	<div id="divStreet" runat="server" class="dnnFormItem">
		<dnn:label id="plStreet" runat="server" controlname="txtStreet" />
		<asp:textbox id="txtStreet" runat="server" MaxLength="50" />
		<asp:checkbox ID="chkStreet" Runat="server" Visible="False" AutoPostBack="True" />
		<asp:requiredfieldvalidator id="valStreet" runat="server" CssClass="dnnFormMessage dnnFormError" ControlToValidate="txtStreet" Display="Dynamic" />
	</div>
	<div id="divUnit" runat="server" class="dnnFormItem">
		<dnn:label id="plUnit" runat="server" controlname="txtUnit" />
		<asp:textbox id="txtUnit" runat="server" MaxLength="50" />
	</div>
	<div id="divCity" runat="server" class="dnnFormItem">
		<dnn:label id="plCity" runat="server" controlname="txtCity" />
		<asp:textbox id="txtCity" runat="server" MaxLength="50" />
		<asp:checkbox ID="chkCity" Runat="server" Visible="False" AutoPostBack="True" />
		<asp:requiredfieldvalidator id="valCity" runat="server" CssClass="dnnFormMessage dnnFormError" ControlToValidate="txtCity" Display="Dynamic" />
	</div>
	<div id="divCountry" runat="server" class="dnnFormItem dnnAddressCountry">
		<dnn:label id="plCountry" runat="server" controlname="cboCountry" />
		<div class="dnnLeft">
			<wc:CountryListBox TestIP="" LocalhostCountryCode="US" id="cboCountry" DataValueField="Value" DataTextField="Text" AutoPostBack="True" runat="server" />
		</div>
		<asp:checkbox ID="chkCountry" Runat="server" Visible="False" AutoPostBack="True" />        
		<asp:label ID="lblCountryRequired" Runat="server" />
		<asp:requiredfieldvalidator id="valCountry" runat="server" CssClass="dnnFormMessage dnnFormError" ControlToValidate="cboCountry" Display="Dynamic" />
	</div>
	<div id="divRegion" runat="server" class="dnnFormItem">
		<dnn:label id="plRegion" runat="server" controlname="cboRegion" />
		<asp:DropDownList id="cboRegion" runat="server" DataValueField="Value" DataTextField="Text" Visible="False" />
		<asp:textbox id="txtRegion" runat="server" MaxLength="50" />
		<asp:checkbox ID="chkRegion" Runat="server" Visible="False" AutoPostBack="True" />
		<asp:requiredfieldvalidator id="valRegion1" runat="server" CssClass="dnnFormMessage dnnFormError" ControlToValidate="cboRegion" Display="Dynamic" />
		<asp:requiredfieldvalidator id="valRegion2" runat="server" CssClass="dnnFormMessage dnnFormError" ControlToValidate="txtRegion" Display="Dynamic" />
	</div>
	<div id="divPostal" runat="server" class="dnnFormItem">
		<dnn:label id="plPostal" runat="server" controlname="txtPostal" />
		<asp:textbox id="txtPostal" runat="server" MaxLength="50" />
		<asp:checkbox ID="chkPostal" Runat="server" Visible="False" AutoPostBack="True" />
		<asp:requiredfieldvalidator id="valPostal" runat="server" CssClass="dnnFormMessage dnnFormError" ControlToValidate="txtPostal" ErrorMessage="<br>Postal Code Is Required." Display="Dynamic" />
	</div>
	<div id="divTelephone" runat="server" class="dnnFormItem">
		<dnn:label id="plTelephone" runat="server" controlname="txtTelephone" />
		<asp:textbox id="txtTelephone" runat="server" MaxLength="50" />
		<asp:checkbox ID="chkTelephone" Runat="server" Visible="False" AutoPostBack="True" />
		<asp:requiredfieldvalidator id="valTelephone" runat="server" CssClass="dnnFormMessage dnnFormError" ControlToValidate="txtTelephone" Display="Dynamic" />
	</div>
	<div id="divCell" runat="server" class="dnnFormItem">
		<dnn:label id="plCell" runat="server" controlname="txtCell" />
		<asp:textbox id="txtCell" runat="server" MaxLength="50" />
		<asp:checkbox ID="chkCell" Runat="server" Visible="False" AutoPostBack="True" />
		<asp:requiredfieldvalidator id="valCell" runat="server" CssClass="dnnFormMessage dnnFormError" ControlToValidate="txtCell" Display="Dynamic" />
	</div>
	<div id="divFax" runat="server" class="dnnFormItem">
		<dnn:label id="plFax" runat="server" controlname="txtFax" />
		<asp:textbox id="txtFax" runat="server" MaxLength="50" />
		<asp:checkbox ID="chkFax" Runat="server" Visible="False" AutoPostBack="True" />
		<asp:requiredfieldvalidator id="valFax" runat="server" CssClass="dnnFormMessage dnnFormError" ControlToValidate="txtFax" Display="Dynamic" />
	</div>
</div>