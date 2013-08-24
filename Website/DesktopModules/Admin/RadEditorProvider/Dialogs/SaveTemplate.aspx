<%@ Page Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Providers.RadEditorProvider.SaveTemplate" CodeBehind="SaveTemplate.aspx.cs" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
	<title><asp:Literal ID="lblTitle" runat="server"></asp:Literal></title>
	<meta http-equiv="PAGE-ENTER" content="RevealTrans(Duration=0,Transition=1)" />    
	<base target="_self" />
	<style type="text/css">
		.FolderListComboItem { white-space:nowrap; }
		body
		{
			font-family: Verdana;
			font-size: 11px;
			color: #333;
		}
	</style>    
</head>
<body>
<telerik:RadScriptBlock ID="RadScriptBlock1" runat="server">
<script type="text/javascript">

	function getRadWindow()
	{
		if (window.radWindow)
			return window.radWindow;
		if (window.frameElement && window.frameElement.radWindow)
			return window.frameElement.radWindow;
		return null;
	}

	function initDialog()
	{
		var args = getRadWindow().ClientParameters;
		if (args) {

			var textInput = document.getElementById("<%=htmlText2.ClientID%>");

			if (textInput) {
				textInput.value = args[0];
			}
			else {
				alert('textinput not found!');
			}
		}
		else {
			alert('no args loaded');
		}    		    		
	}

	if (window.attachEvent) {
		window.attachEvent("onload", initDialog);
	}
	else 
	{
		if (window.addEventListener) 
		{
			window.addEventListener("load", initDialog, false);
		}            
	}
		</script>
</telerik:RadScriptBlock>

	<form id="form2" runat="server">
	<div style="text-align:center">
		<div id="msgSuccess" runat="server" visible="false" style="padding:10px;"></div>
		<div id="msgError" runat="server" visible="false" style="padding:10px;"></div>
		<asp:Button id="cmdClose" runat="server" Visible="false" Text="Close" OnClientClick="getRadWindow().close(); return false;" />
	</div>
	<asp:TextBox id="htmlText2" runat="server" TextMode="MultiLine" style="display:none;" />
	<asp:HiddenField id="htmlText" runat="server" value="" />
	<div id="divInputArea" runat="server" style="padding:10px;" class="Normal">
		<div style="margin-bottom:5px;"><asp:Label id="lblFolders" runat="server" Text="Folder" />:</div>
		<div><telerik:RadComboBox ID="FolderList" runat="server" Width="285px" DropDownCssClass="FolderListComboItem" /></div>
		<div style="margin-top:15px;margin-bottom:5px;"><asp:Label id="lblFileName" runat="server" Text="File name" />:</div>
		<div><asp:TextBox id="FileName" runat="server" MaxLength="25" Width="250px" />.html</div>
		<div style="margin-top:15px"><asp:Checkbox id="Overwrite" runat="server" Checked="true" /><asp:Label id="lblOverwrite" AssociatedControlID="Overwrite" runat="server" Text="Overwrite if file exists?" /></div>
		<div style="margin-top:25px"><asp:Button id="cmdSave" runat="server" Text="Save" OnClick="Save_OnClick" />
			&nbsp;&nbsp;
			<asp:Button id="cmdCancel" runat="server" Text="Cancel" OnClientClick="getRadWindow().close(); return false;" />
		</div>
	</div>
	</form>
</body>
</html>