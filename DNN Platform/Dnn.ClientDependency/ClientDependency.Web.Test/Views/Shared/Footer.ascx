<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="ClientDependency.Core.Mvc" %>

<% Html.RequiresJs("~/Js/jquery-1.3.2.min.js", 1); %>
<% Html.RequiresJs("FooterScript.js", "Scripts", 200); %>

<%--Demonstrates the use of using the PathNameAlias--%>
<% Html.RequiresCss("Controls.css", "Styles"); %>

<div class="control footer bg-complement-1">
	This is a footer
</div>