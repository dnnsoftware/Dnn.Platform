<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Sidebar.ascx.cs" Inherits="ClientDependency.Web.Test.Controls.Sidebar" %>
<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>
<%@ Register Namespace="ClientDependency.Web.Test.Controls" Assembly="ClientDependency.Web.Test" TagPrefix="Web" %>

<CD:JsInclude ID="JsInclude1" runat="server" FilePath="~/Js/jquery-1.3.2.min.js" />


<%--Demonstrates the use of using the PathNameAlias--%>
<CD:CssInclude ID="CssInclude2" runat="server" FilePath="Controls.css" PathNameAlias="Styles" />

<div class="control sidebar bg-primary-3 white">
	This is a side bar
	<Web:MyCustomControl runat="server" id="MyControl"></Web:MyCustomControl>
</div>