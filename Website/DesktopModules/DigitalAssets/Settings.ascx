<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Settings.ascx.cs" Inherits="DotNetNuke.Modules.DigitalAssets.Settings" %>
<%@ Register TagPrefix="dnnweb" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>

<h2 id="dnnSitePanel-BasicSettings" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("BasicSettings")%></a></h2>
<fieldset>
    <div class="dnnFormItem">
        <dnnweb:Label ID="DefaultFolderTypeLabel" runat="server" ResourceKey="DefaultFolderType" Suffix=":" HelpKey="DefaultFolderType.Help" ControlName="DefaultFolderTypeComboBox" />
		<dnnweb:DnnComboBox id="DefaultFolderTypeComboBox" DataTextField="MappingName" DataValueField="FolderMappingID" runat="server" CssClass="dnnFixedSizeComboBox" />
    </div>
</fieldset>
