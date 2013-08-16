<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PreviewPanelControl.ascx.cs" Inherits="DotNetNuke.Modules.DigitalAssets.PreviewPanelControl" %>
<%@ Register TagPrefix="dam" tagName="PreviewFieldsControl" src="~/DesktopModules/DigitalAssets/PreviewFieldsControl.ascx"%>
<asp:Panel runat="server" ID="ScopeWrapper">
    <div class="dnnModuleDigitalAssetsPreviewInfoTitle"><%=Title %>:</div>
    <div class="dnnModuleDigitalAssetsPreviewInfoImageContainer"><img src="<%=PreviewImageUrl %>" class="dnnModuleDigitalAssetsPreviewInfoImage"/></div>    
    <div class="dnnModuleDigitalAssetsPreviewInfoFieldsContainer">
        <dam:PreviewFieldsControl ID="FieldsControl" runat="server"></dam:PreviewFieldsControl>
    </div>
</asp:Panel>
