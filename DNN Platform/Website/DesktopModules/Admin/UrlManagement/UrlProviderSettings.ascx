<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UrlProviderSettings.ascx.cs" Inherits="DotNetNuke.Modules.UrlManagement.ProviderSettings" %>
<asp:PlaceHolder id="providerSettingsPlaceHolder" runat="server" />
<ul class="dnnActions dnnClear">
    <li><asp:LinkButton ID="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
    <li><asp:LinkButton ID="cmdCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" CausesValidation="False" /></li>
</ul>
