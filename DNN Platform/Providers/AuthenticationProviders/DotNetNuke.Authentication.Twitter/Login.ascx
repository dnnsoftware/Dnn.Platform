<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Login.ascx.cs" Inherits="DotNetNuke.Authentication.Twitter.Login" %>

<li id="loginItem" runat="server" class="twitter" >
    <asp:LinkButton runat="server" ID="loginButton" CausesValidation="False">
        <span><%=LocalizeString("SignInTwitter")%></span>
    </asp:LinkButton>
</li>
<li id="registerItem" runat="Server" class="twitter">
    <asp:LinkButton ID="registerButton" runat="server" CausesValidation="False">
        <span><%=LocalizeString("RegisterTwitter") %></span>
    </asp:LinkButton>
</li>