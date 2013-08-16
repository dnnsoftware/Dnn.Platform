<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.Containers.DropDownActions" CodeFile="DropDownActions.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<script>
	function cmdGo_OnClick(o)
	{
		if (o.selectedIndex > -1)
		{
			var sVal = dnn.getVar('__dnn_CSAction_' + o.id + '_' + o.options[o.selectedIndex].value);
			if (sVal != null)
			{
				var bRet = false;
				eval('var bRet = ' + sVal);
				if (bRet == false)
					return false;
			}

		}
		return true;
	}
</script>

<table cellpadding="0" cellspacing="0" border="0">
	<tr>
		<td nowrap>
			<span id="spActions" runat="server"/>
			<dnn:DnnImageButton id="cmdGo" runat="server" IconKey="Fwd" AlternateText="Go" ToolTip="Go"></dnn:DnnImageButton>
		</td>
	</tr>
</table>
