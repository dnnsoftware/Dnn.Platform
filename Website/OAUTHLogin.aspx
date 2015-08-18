<%@ Page Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Services.OAUTHLogin.OAUTHLogin" CodeFile="OAUTHLogin.aspx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.WebControls" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" lang="en-US">
<head>
    <meta name="revisit-after" content="1 days" />
    <meta name="robots" content="noindex,nofollow" />
    <title runat="server" id="Title">DNN OAUTH Login</title>
    <link id="DefaultStylesheet" runat="server" rel="stylesheet" type="text/css" href="~/Portals/_default/default.css" />
    <link id="InstallStylesheet" runat="server" rel="stylesheet" type="text/css" href="~/Install/install.css" />
</head>
<body>
    <form id="Form" runat="server">
      <div class="oauthLogin">
            <h2>DNN OAUTH Login</h2>
            
            <div class="dnnFormItem">
                Username
                <asp:textbox id="txtUsername" runat="server" />
            </div>
            
            <div class="dnnFormItem">
                Password
                <asp:textbox id="txtPassword" textmode="password" runat="server" />
            </div>
            <ul class="dnnActions dnnClear">
                <li><asp:LinkButton id="cmdLogin"  cssclass="dnnSecondaryAction" runat="server" CausesValidation="false" Text="Login" /></li>
            </ul>
        </div>
    </form>
</body>
</html>
