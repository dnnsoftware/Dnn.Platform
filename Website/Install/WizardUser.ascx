<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Services.Install.WizardUser" CodeFile="WizardUser.ascx.cs" %>
<div class="dnnFormItem">
    <label for="<%=txtFirstName.ClientID%>"><asp:Label ID="lblFirstName" runat="server" /></label>
    <asp:TextBox ID="txtFirstName" runat="Server" />
</div>
<div class="dnnFormItem">
    <label for="<%=txtLastName.ClientID%>"><asp:Label ID="lblLastName" runat="server" /></label>
    <asp:TextBox ID="txtLastName" runat="Server" />
</div>
<div class="dnnFormItem">
    <label for="<%=txtUserName.ClientID%>"><asp:Label ID="lblUserName" runat="server" /></label>
    <asp:TextBox ID="txtUserName" runat="Server" />
</div>
<div class="dnnFormItem">
    <label for="<%=txtPassword.ClientID%>"><asp:Label ID="lblPassword" runat="server" /></label>
    <asp:TextBox ID="txtPassword" runat="Server" TextMode="password" />
</div>
<div class="dnnFormItem">
    <label for="<%=txtConfirm.ClientID%>"><asp:Label ID="lblConfirm" runat="server" /></label>
    <asp:TextBox ID="txtConfirm" runat="Server" TextMode="password" />
</div>
<div class="dnnFormItem">
    <label for="<%=txtEmail.ClientID%>"><asp:Label ID="lblEmail" runat="server" /></label>
    <asp:TextBox ID="txtEmail" runat="Server"  />
</div>
