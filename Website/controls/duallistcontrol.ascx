<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.UserControls.DualListControl" %>
<table cellspacing="0" cellpadding="2" border="0">
	<tr>
		<td align="center" class="NormalBold"><asp:Label id=Label1 runat="server" enableviewstate="False">Available</asp:Label></td>
		<td align="center">&nbsp;</td>
		<td align="center" class="NormalBold"><asp:Label id=Label2 runat="server" enableviewstate="False">Assigned</asp:Label></td>
	</tr>
	<tr>
		<td align="center" valign="top">
			<asp:ListBox ID="lstAvailable" runat="server" class="NormalTextBox" SelectionMode="Multiple"></asp:ListBox>
		</td>
		<td align="center" valign="middle">
			<table cellpadding="0" cellspacing="0" border="0">
				<tr>
					<td align="center" valign="top"><asp:linkbutton id="cmdAdd" runat="server" cssclass="CommandButton" Text="&nbsp;>&nbsp;" enableviewstate="False" /></td>
				</tr>
				<tr>
					<td align="center" valign="top"><asp:linkbutton id="cmdRemove" runat="server" cssclass="CommandButton" Text="&nbsp;<&nbsp;" enableviewstate="False" /></td>
				</tr>
				<tr>
					<td>&nbsp;</td>
				</tr>
				<tr>
					<td align="center" valign="bottom"><asp:linkbutton id="cmdAddAll" runat="server" cssclass="CommandButton" Text="&nbsp;>>&nbsp;" enableviewstate="False" /></td>
				</tr>
				<tr>
					<td align="center" valign="bottom"><asp:linkbutton id="cmdRemoveAll" runat="server" cssclass="CommandButton" Text="&nbsp;<<&nbsp;" enableviewstate="False" /></td>
				</tr>
			</table>
		</td>
		<td align="center" valign="top">
			<asp:listbox runat="server" ID="lstAssigned" class="NormalTextBox" SelectionMode="Multiple"></asp:listbox>
		</td>
	</tr>
</table>
