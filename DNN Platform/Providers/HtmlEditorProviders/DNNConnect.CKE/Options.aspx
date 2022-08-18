<%@ Page language="c#" Codebehind="Options.aspx.cs" AutoEventWireup="True" Inherits="DNNConnect.CKEditorProvider.Options" %>
<%@ Register TagPrefix="dnncrm" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<!DOCTYPE html>
<html lang="en">
  <head runat="server">
    <meta http-equiv="content-type" content="text/html; charset=UTF-8" />
    <meta name="language" content="en" />
    <title>CKEditor Options</title>
    <asp:PlaceHolder id="favicon" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="ClientDependencyHeadCss" />
    <asp:PlaceHolder runat="server" ID="ClientDependencyHeadJs" />
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
      <asp:PlaceHolder ID="BodySCRIPTS" runat="server" />
      <asp:PlaceHolder id="phControls" runat="server" />
      <asp:PlaceHolder runat="server" ID="ClientResourcesFormBottom" />
    </form>
  
    <asp:PlaceHolder runat="server" id="ClientResourceIncludes" />

    <dnncrm:ClientResourceLoader runat="server" id="ClientResourceLoader">
      <Paths>
        <dnncrm:ClientResourcePath Name="SharedScripts" Path="~/Resources/Shared/Scripts/" />
      </Paths>
    </dnncrm:ClientResourceLoader>
  </body>
</html>
