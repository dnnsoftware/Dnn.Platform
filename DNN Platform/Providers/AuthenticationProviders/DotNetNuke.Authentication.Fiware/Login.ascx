<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Login.ascx.cs" Inherits="DotNetNuke.Authentication.Fiware.Login" %>

<li id="loginItem" runat="server" class="fiware" >
    <asp:LinkButton runat="server" ID="loginButton" CausesValidation="False">
        <span><%=LocalizeString("SignInFiware")%></span>
    </asp:LinkButton>
</li>
<li id="registerItem" runat="Server" class="fiware">
    <asp:LinkButton ID="registerButton" runat="server" CausesValidation="False">
        <span><%=LocalizeString("RegisterFiware") %></span>
    </asp:LinkButton>
</li>