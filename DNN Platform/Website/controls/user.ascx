<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.UserControls.User" %>
<%@ Register TagPrefix="wc" Namespace="DotNetNuke.UI.WebControls" Assembly="CountryListBox" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>
<table cellSpacing="0" cellPadding="1" border="0" summary="User Design Table">
	<tr>
		<td class="SubHead" width="175">
			<dnn:label id="plFirstName" runat="server" controlname="txtFirstName" text="First Name:"></dnn:label></td>
		<td class="NormalBold" noWrap>
			<asp:textbox id="txtFirstName" tabIndex="1" runat="server" cssclass="NormalTextBox" size="25"
				maxlength="50"></asp:textbox>&nbsp;*
			<asp:requiredfieldvalidator id="valFirstName" runat="server" cssclass="NormalRed" display="Dynamic" errormessage="<br>First Name Is Required."
				controltovalidate="txtFirstName" resourcekey="valFirstName"></asp:requiredfieldvalidator></td>
	</tr>
	<tr>
		<td class="SubHead" width="175">
			<dnn:label id="plLastName" runat="server" controlname="txtLastName" text="Last Name:"></dnn:label></td>
		<td class="NormalBold" noWrap>
			<asp:textbox id="txtLastName" tabIndex="2" runat="server" cssclass="NormalTextBox" size="25"
				maxlength="50"></asp:textbox>&nbsp;*
			<asp:requiredfieldvalidator id="valLastName" runat="server" cssclass="NormalRed" display="Dynamic" errormessage="<br>Last Name Is Required."
				controltovalidate="txtLastName" resourcekey="valLastName"></asp:requiredfieldvalidator></td>
	</tr>
	<tr>
		<td class="SubHead" width="175">
			<dnn:label id="plUserName" runat="server" controlname="txtUsername" text="User Name:"></dnn:label></td>
		<td class="NormalBold" noWrap>
			<asp:textbox id="txtUsername" tabIndex="3" runat="server" cssclass="NormalTextBox" size="25"
				maxlength="175"></asp:textbox>
			<asp:label id="lblUsernameAsterisk" runat="server">*</asp:label>
			<asp:label id="lblUsername" runat="server"></asp:label>
			<asp:requiredfieldvalidator id="valUsername" runat="server" cssclass="NormalRed" display="Dynamic" errormessage="<br>Username Is Required."
				controltovalidate="txtUsername" resourcekey="valUsername"></asp:requiredfieldvalidator></td>
	</tr>
	<tr id="PasswordRow" runat="server">
		<td class="SubHead" width="175">
			<dnn:label id="plPassword" runat="server" controlname="txtPassword" text="Password:"></dnn:label></td>
		<td class="NormalBold" noWrap>
			<asp:textbox id="txtPassword" tabIndex="4" runat="server" cssclass="NormalTextBox" size="25"
				maxlength="20" textmode="Password"></asp:textbox>&nbsp;*
			<asp:requiredfieldvalidator id="valPassword" runat="server" cssclass="NormalRed" display="Dynamic" errormessage="<br>Password Is Required."
				controltovalidate="txtPassword" resourcekey="valPassword"></asp:requiredfieldvalidator></td>
	</tr>
	<tr id="ConfirmPasswordRow" runat="server">
		<td class="SubHead" width="175">
			<dnn:label id="plConfirm" runat="server" controlname="txtConfirm" text="Confirm:"></dnn:label></td>
		<td class="NormalBold" noWrap>
			<asp:textbox id="txtConfirm" tabIndex="5" runat="server" cssclass="NormalTextBox" size="25" maxlength="20"
				textmode="Password"></asp:textbox>&nbsp;*
			<asp:requiredfieldvalidator id="valConfirm1" runat="server" cssclass="NormalRed" display="Dynamic" errormessage="<br>Password Confirmation Is Required."
				controltovalidate="txtConfirm" resourcekey="valConfirm1"></asp:requiredfieldvalidator>
			<asp:comparevalidator id="valConfirm2" runat="server" cssclass="NormalRed" display="Dynamic" errormessage="<br>Password Values Entered Do Not Match."
				controltovalidate="txtConfirm" resourcekey="valConfirm2" controltocompare="txtPassword"></asp:comparevalidator></td>
	</tr>
	<tr>
		<td class="SubHead" width="175">
			<dnn:label id="plEmail" runat="server" controlname="txtEmail" text="Email Address:"></dnn:label></td>
		<td class="NormalBold" noWrap>
			<asp:textbox id="txtEmail" tabIndex="6" runat="server" cssclass="NormalTextBox" size="25" maxlength="175"></asp:textbox>&nbsp;*
			<asp:requiredfieldvalidator id="valEmail1" runat="server" cssclass="NormalRed" display="Dynamic" errormessage="<br>Email Is Required."
				controltovalidate="txtEmail" resourcekey="valEmail1"></asp:requiredfieldvalidator>
			<asp:regularexpressionvalidator id="valEmail2" runat="server" cssclass="NormalRed" display="Dynamic" errormessage="<br>Email Must be Valid."
				controltovalidate="txtEmail" resourcekey="valEmail2" validationexpression="[\w\.-]+(\+[\w-]*)?@([\w-]+\.)+[\w-]+"></asp:regularexpressionvalidator></td>
	</tr>
	<tr>
		<td class="SubHead" width="175">
			<dnn:label id="plWebsite" runat="server" controlname="txtWebsite" text="Website:"></dnn:label></td>
		<td class="NormalBold" noWrap>
			<asp:textbox id="txtWebsite" tabIndex="7" runat="server" cssclass="NormalTextBox" size="25" maxlength="175"></asp:textbox></td>
	</tr>
	<tr>
		<td class="SubHead" width="175">
			<dnn:label id="plIM" runat="server" controlname="txtIM" text="Instant Messaging ID:"></dnn:label></td>
		<td class="NormalBold" noWrap>
			<asp:textbox id="txtIM" tabIndex="8" runat="server" cssclass="NormalTextBox" size="25" maxlength="175"></asp:textbox></td>
	</tr>
</table>
