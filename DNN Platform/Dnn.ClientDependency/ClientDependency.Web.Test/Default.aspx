<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ClientDependency.Web.Test.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <link rel="Stylesheet" href="Css/Site.css" />
    <link rel="Stylesheet" href="Css/ColorScheme.css" />
    <link rel="Stylesheet" href="Css/Default.css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <div class="choose">Pick your poison</div>
        <div class="link"><a href="<%=ResolveClientUrl("~/Test/Default") %>">MVC</a></div>
        <div class="link"><a href="<%=ResolveClientUrl("~/Pages") %>">Forms</a></div>
    </div>
    </form>
</body>
</html>
