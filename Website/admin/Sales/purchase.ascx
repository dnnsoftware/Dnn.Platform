<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Sales.Purchase" CodeFile="Purchase.ascx.cs" %>
<br>
<table cellSpacing="0" cellPadding="0" width="750" border="0" summary="Purchase Design Table">
	<tr vAlign="top">
		<td class="SubHead">Service Name:
		</td>
		<td><asp:label id="lblServiceName" runat="server" width="249px" cssclass="Normal"></asp:label></td>
	</tr>
	<tr vAlign="top">
		<td class="SubHead">Description:
		</td>
		<td><asp:label id="lblDescription" runat="server" cssclass="Normal"></asp:label></td>
	</tr>
	<tr vAlign="top">
		<td colSpan="2">&nbsp;
		</td>
	</tr>
	<tr vAlign="top">
		<td class="SubHead">Fee:
		</td>
		<td><asp:label id="lblFee" runat="server" cssclass="Normal"></asp:label>&nbsp;&nbsp;<asp:Label ID="lblFeeCurrency" Runat="server" CssClass="NormalBold"></asp:Label></td>
	</tr>
	<tr vAlign="top">
		<td class="SubHead">Per:
		</td>
		<td><asp:label id="lblFrequency" runat="server" width="244px" cssclass="Normal"></asp:label></td>
	</tr>
	<tr vAlign="top">
		<td colSpan="2">&nbsp;
		</td>
	</tr>
	<tr vAlign="top">
		<td class="SubHead"><label Class="SubHead" for="<%=txtUnits.ClientID%>">Units:</label>
		</td>
		<td>
			<asp:textbox id="txtUnits" runat="server" maxlength="10" Columns="10" cssclass="NormalTextBox"></asp:textbox>
			<asp:requiredfieldvalidator id="valUnits1" runat="server" ErrorMessage="<br>You Must Enter The Number Of Units You Wish To Purchase" ControlToValidate="txtUnits" Display="Dynamic" CssClass="NormalRed"></asp:requiredfieldvalidator>
			<asp:comparevalidator id="valUnits2" runat="server" ControlToValidate="txtUnits" ErrorMessage="<br>You Must Enter The Number Of Units You Wish To Purchase" Display="Dynamic" Operator="DataTypeCheck" Type="Integer" CssClass="NormalRed"></asp:comparevalidator>
			<asp:comparevalidator id="valUnits3" runat="server" ControlToValidate="txtUnits" ErrorMessage="<br>You Must Enter The Number Of Units You Wish To Purchase" Display="Dynamic" Operator="GreaterThan" ValueToCompare="0" CssClass="NormalRed"></asp:comparevalidator>
		</td>
	</tr>
	<tr vAlign="top">
		<td colSpan="2">&nbsp;
		</td>
	</tr>
	<tr vAlign="top">
		<td class="SubHead">Total Charges:
		</td>
		<td><asp:label id="lblTotal" runat="server" cssclass="Normal"></asp:label>&nbsp;&nbsp;<asp:Label ID="lblTotalCurrency" Runat="server" CssClass="NormalBold"></asp:Label></td>
	</tr>
</table>
<p>
	<asp:linkbutton class="CommandButton" id="cmdUpdate" runat="server" BorderStyle="none" Text="Update" CausesValidation="False">Update</asp:linkbutton>&nbsp;
	<asp:linkbutton class="CommandButton" id="cmdPurchase" runat="server" BorderStyle="none" Text="Update">Purchase</asp:linkbutton>&nbsp;
	<asp:linkbutton class="CommandButton" id="cmdCancel" runat="server" BorderStyle="none" Text="Cancel" CausesValidation="False"></asp:linkbutton>
</p>
