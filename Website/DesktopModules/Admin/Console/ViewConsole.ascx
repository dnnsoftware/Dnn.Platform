<%@ Control Language="C#" AutoEventWireup="false" Inherits="DesktopModules.Admin.Console.ViewConsole" CodeFile="ViewConsole.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<script type="text/javascript">
	jQuery(document).ready(function($) {
		$("#<%=Console.ClientID %>").dnnConsole({<%=GetClientSideSettings() %>});
	});
</script>

<div id="Console" runat="server" class="console">
	<asp:DropDownList ID="IconSize" runat="server" />
	<asp:DropDownList ID="View" runat="server" />
    <%--<dnn:DnnComboBox ID="IconSize" runat="server" />
    <dnn:DnnComboBox ID="View" runat="server" />--%>
	<br id="SettingsBreak" runat="server" style="clear:both" />
	<div>
	<asp:Repeater ID="DetailView" runat="server">
	</asp:Repeater>
	</div>
	<br style="clear:both" />
</div>
