<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Footer.ascx.cs" Inherits="ClientDependency.Web.Test.Controls.Footer" %>
<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>

<CD:JsInclude ID="JsInclude1" runat="server" FilePath="~/Js/jquery-1.3.2.min.js" Priority="1" />
<CD:JsInclude ID="JsInclude2" runat="server" FilePath="~/Js/FooterScript.js" Priority="200" />

<%--Demonstrates the use of using the PathNameAlias--%>
<CD:CssInclude ID="CssInclude1" runat="server" FilePath="Controls.css" PathNameAlias="Styles" />

<div class="control footer bg-complement-1">
	This is a footer
</div>