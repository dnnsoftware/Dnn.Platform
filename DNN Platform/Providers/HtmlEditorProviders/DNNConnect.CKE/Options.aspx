<%@ Page language="c#" Codebehind="Options.aspx.cs" AutoEventWireup="True" Inherits="DNNConnect.CKEditorProvider.Options" %>

<!DOCTYPE html>
<html lang="en">
  <head>
    <meta http-equiv="content-type" content="text/html; charset=UTF-8" />
    <meta name="language" content="en" />
    <title>CKEditor Options</title>
    <asp:PlaceHolder id="favicon" runat="server"></asp:PlaceHolder>
    <script type="text/javascript">
        var saveGroupName = '<%= DotNetNuke.Services.Localization.Localization.GetString("SaveGroupName.Text", this.ResXFile, this.LangCode) %>';
        var deleteGroup = '<%= DotNetNuke.Services.Localization.Localization.GetString("DeleteGroup.Text", this.ResXFile, this.LangCode) %>';
        var editGroupName = '<%= DotNetNuke.Services.Localization.Localization.GetString("EditGroupName.Text", this.ResXFile, this.LangCode) %>';
        var newGroupName = '<%= DotNetNuke.Services.Localization.Localization.GetString("NewGroupName.Text", this.ResXFile, this.LangCode) %>';
        var deleteToolbar = '<%= DotNetNuke.Services.Localization.Localization.GetString("DeleteToolbarButton.Text", this.ResXFile, this.LangCode) %>';
        var newRowName = '<%= DotNetNuke.Services.Localization.Localization.GetString("RowBreak.Text", this.ResXFile, this.LangCode) %>';
	</script>
    <style type="text/css">
        .groupButtons li {margin-left: -36px;}
    </style>

  </head>
  <body>
    <form id="ckOptionsForm" runat="server">
	  <asp:PlaceHolder id="phControls" runat="server"></asp:PlaceHolder>
	</form>
  </body>
</html>
