<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Login.ascx.cs" Inherits="DotNetNuke.Authentication.LiveConnect.Login" %>

<li id="loginItem" runat="server" class="windowslive" >
    <asp:LinkButton runat="server" ID="loginButton" CausesValidation="False">
        <span><%=LocalizeString("SignInLive")%></span>
    </asp:LinkButton>
</li>
<li id="registerItem" runat="Server" class="windowslive">
    <asp:LinkButton ID="registerButton" runat="server" CausesValidation="False">
        <span><%=LocalizeString("RegisterLive") %></span>
    </asp:LinkButton>
</li>