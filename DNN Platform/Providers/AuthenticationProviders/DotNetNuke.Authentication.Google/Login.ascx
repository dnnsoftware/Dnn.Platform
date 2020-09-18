<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Login.ascx.cs" Inherits="DotNetNuke.Authentication.Google.Login" %>
<li id="loginItem" runat="server" class="googleplus" >
    <asp:LinkButton runat="server" ID="loginButton" CausesValidation="False">
        <span><%=LocalizeString("LoginGoogle") %></span>
    </asp:LinkButton>
</li>
<li id="registerItem" runat="Server" class="googleplus">
    <asp:LinkButton ID="registerButton" runat="server" CausesValidation="False">
        <span><%=LocalizeString("RegisterGoogle") %></span>
    </asp:LinkButton>
</li>