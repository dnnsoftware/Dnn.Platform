<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.Skins.Controls.User" CodeFile="User.ascx.cs" ViewStateMode="Disabled" %>
<asp:HyperLink ID="registerLink" runat="server" CssClass="SkinObject" rel="nofollow" />
<div class="registerGroup" runat="server" id="registerGroup">
    <ul class="buttonGroup">
        <li class="userMessages alpha" runat="server" ID="messageGroup"><asp:HyperLink ID="messageLink" runat="server"/></li>
        <li class="userNotifications omega" runat="server" ID="notificationGroup"><asp:HyperLink ID="notificationLink" runat="server"/></li>
    	<li class="userDisplayName"><asp:HyperLink ID="enhancedRegisterLink" runat="server" rel="nofollow"/></li>
        <li class="userProfileImg" runat="server" ID="avatarGroup"><asp:HyperLink ID="avatar" runat="server"/></li>                                       
    </ul>
</div>