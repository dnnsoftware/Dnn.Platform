<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.Skins.Controls.Language" CodeFile="Language.ascx.cs" %>
<asp:Literal ID="litCommonHeaderTemplate" runat="server" EnableViewState="true" />
<asp:DropDownList ID="selectCulture" runat="server" AutoPostBack="true" CssClass="NormalTextBox"></asp:DropDownList>
<asp:Repeater ID="rptLanguages" runat="server">
    <ItemTemplate><asp:Literal ID="litItemTemplate" runat="server" EnableViewState="true" /></ItemTemplate>
    <AlternatingItemTemplate><asp:Literal ID="litItemTemplate" runat="server" EnableViewState="true" /></AlternatingItemTemplate>
    <HeaderTemplate><asp:Literal ID="litItemTemplate" runat="server" EnableViewState="true" /></HeaderTemplate>
    <SeparatorTemplate><asp:Literal ID="litItemTemplate" runat="server" EnableViewState="true" /></SeparatorTemplate>
    <FooterTemplate><asp:Literal ID="litItemTemplate" runat="server" EnableViewState="true" /></FooterTemplate>
</asp:Repeater>
<asp:Literal ID="litCommonFooterTemplate" runat="server" EnableViewState="true" />