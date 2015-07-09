<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Login.ascx.cs" Inherits="DotNetNuke.Authentication.LinkedIn.Login" %>

<li id="loginItem" runat="server" class="linkedIn" >
    <asp:LinkButton runat="server" ID="loginButton" CausesValidation="False">
        <span><%=LocalizeString("SignInLinkedIn")%></span>
    </asp:LinkButton>
</li>
<li id="registerItem" runat="Server" class="linkedIn">
    <asp:LinkButton ID="registerButton" runat="server" CausesValidation="False">
        <span><%=LocalizeString("RegisterLinkedIn") %></span>
    </asp:LinkButton>
</li>