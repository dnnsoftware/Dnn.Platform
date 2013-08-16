<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Login.ascx.cs" Inherits="DotNetNuke.Authentication.Facebook.Login" %>

<li id="loginItem" runat="server" class="facebook" >
    <asp:LinkButton runat="server" ID="loginButton" CausesValidation="False">
        <span><%=LocalizeString("LoginFacebook")%></span>
    </asp:LinkButton>
</li>
<li id="registerItem" runat="Server" class="facebook">
    <asp:LinkButton ID="registerButton" runat="server" CausesValidation="False">
        <span><%=LocalizeString("RegisterFacebook") %></span>
    </asp:LinkButton>
</li>
