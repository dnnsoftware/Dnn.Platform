<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.Skins.Controls.Language" CodeFile="Language.ascx.cs" ViewStateMode="Disabled" %>
<asp:Literal ID="litCommonHeaderTemplate" runat="server" />
<asp:DropDownList ID="selectCulture" runat="server" AutoPostBack="true" CssClass="NormalTextBox"/>
<asp:Repeater ID="rptLanguages" runat="server">
    <ItemTemplate><asp:Literal ID="litItemTemplate" runat="server" /></ItemTemplate>
    <AlternatingItemTemplate><asp:Literal ID="litItemTemplate" runat="server" /></AlternatingItemTemplate>
    <HeaderTemplate><asp:Literal ID="litItemTemplate" runat="server" /></HeaderTemplate>
    <SeparatorTemplate><asp:Literal ID="litItemTemplate" runat="server" /></SeparatorTemplate>
    <FooterTemplate><asp:Literal ID="litItemTemplate" runat="server" /></FooterTemplate>
</asp:Repeater>
<asp:Literal ID="litCommonFooterTemplate" runat="server"/>