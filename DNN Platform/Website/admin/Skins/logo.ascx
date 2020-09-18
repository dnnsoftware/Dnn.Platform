<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.Skins.Controls.Logo" ViewStateMode="Disabled" CodeBehind="Logo.ascx.cs" %>
<asp:HyperLink ID="hypLogo" runat="server">
    <asp:Image ID="imgLogo" runat="server" EnableViewState="False" />
    <asp:Literal ID="litLogo" runat="server" EnableViewState="False" />
</asp:HyperLink>