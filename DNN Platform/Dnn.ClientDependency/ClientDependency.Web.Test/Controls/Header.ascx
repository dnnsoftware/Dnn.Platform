<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Header.ascx.cs" Inherits="ClientDependency.Web.Test.Controls.Header" %>
<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>

<%--<CD:JsInclude ID="JsInclude1" runat="server" FilePath="~/Js/jquery-1.3.2.min.js" />--%>

<%--Demonstrates the use of using the PathNameAlias--%>
<CD:CssInclude ID="CssInclude1" runat="server" FilePath="Controls.css" PathNameAlias="Styles" />

<div class="control white bg-complement-2">
    <a href="<%= ResolveClientUrl("~/") %>" class="f-primary-4 bg-complement-2">Return to landing page</a>
</div>
<div class="control header bg-primary-3 white">
	This is a header
	<ul >
		<li><a class="white" href="<%=ResolveClientUrl("~/Pages/Default.aspx") %>">Default Provider</a></li>
		<li><a class="white" href="<%=ResolveClientUrl("~/Pages/HtmlIncludeTest.aspx") %>">HtmlInclude control tests</a></li>
		<li><a class="white" href="<%=ResolveClientUrl("~/Pages/LazyLoadProviderTest.aspx") %>">Lazy Load Provider with dynamic registration</a></li>
		<li><a class="white" href="<%=ResolveClientUrl("~/Pages/ForcedProviders.aspx") %>">Default Provider with a Forced providers on certain dependencies</a></li>
		<li><a class="white" href="<%=ResolveClientUrl("~/Pages/RogueScriptDetectionTest.aspx") %>">Rogue script detection test</a></li>
        <li><a class="white" href="<%=ResolveClientUrl("~/Pages/EncodingTest.aspx") %>">File Encoding test</a></li>
        <li><a class="white" href="<%=ResolveClientUrl("~/Pages/EmbeddedResourceTest.aspx") %>">Embedded Resource test</a></li>
        <li><a class="white" href="<%=ResolveClientUrl("~/Pages/RemoteDependencies.aspx") %>">Remote Dependencies test</a></li>
        <li><a class="white" href="<%=ResolveClientUrl("~/Pages/HtmlAttributes.aspx") %>">Html attribute tests</a></li>
        <li><a class="white" href="<%=ResolveClientUrl("~/Pages/DependencyGroups.aspx") %>">Dependency group tests</a></li>
	</ul>
</div>