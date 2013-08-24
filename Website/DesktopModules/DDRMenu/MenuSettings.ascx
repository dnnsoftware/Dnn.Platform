<%@ Control Language="C#" Inherits="DotNetNuke.Web.DDRMenu.MenuSettings" AutoEventWireup="true" CodeBehind="MenuSettings.ascx.cs" %>
<%@ Register Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" TagPrefix="dnn" %>
<div class="Normal">
	<table>
		<tr>
			<td align="right" class="Normal">
				Menu style:
			</td>
			<td>
			    <dnn:TextEditControl ID="MenuStyle" runat="server" />
			</td>
		</tr>
		<tr>
			<td align="right" class="Normal">
				Node XML path:
			</td>
			<td>
			    <dnn:TextEditControl ID="NodeXmlPath" runat="server" />
			</td>
		</tr>
		<tr>
			<td align="right" class="Normal">
				Node selection expression:
			</td>
			<td>
			    <dnn:TextEditControl ID="NodeSelector" runat="server" />
			</td>
		</tr>
		<tr>
			<td align="right" class="Normal">
				Nodes to include:
			</td>
			<td>
			    <dnn:TextEditControl ID="IncludeNodes" runat="server" />
			</td>
		</tr>
		<tr>
			<td align="right" class="Normal">
				Nodes to exclude:
			</td>
			<td>
			    <dnn:TextEditControl ID="ExcludeNodes" runat="server" />
			</td>
		</tr>
		<tr>
			<td align="right" class="Normal">
				Node manipulator type:
			</td>
			<td>
			    <dnn:TextEditControl ID="NodeManipulator" runat="server" />
			</td>
		</tr>
		<tr>
			<td align="right" class="Normal">
				Include context in XML:
			</td>
			<td>
			    <dnn:CheckEditControl ID="IncludeContext" runat="server" />
			</td>
		</tr>
		<tr id="IncludeHiddenSection" runat="server">
			<td align="right" class="Normal">
				Include hidden nodes:
			</td>
			<td>
			    <dnn:CheckEditControl ID="IncludeHidden" runat="server" />
			</td>
		</tr>
		<tr>
			<td align="right" class="Normal">
				Template arguments:
			</td>
			<td>
			    <dnn:MultiLineTextEditControl ID="TemplateArguments" runat="server" />
			</td>
		</tr>
		<tr>
			<td align="right" class="Normal">
				Client options:
			</td>
			<td>
			    <dnn:MultiLineTextEditControl ID="ClientOptions" runat="server" />
			</td>
		</tr>
	</table>
</div>
