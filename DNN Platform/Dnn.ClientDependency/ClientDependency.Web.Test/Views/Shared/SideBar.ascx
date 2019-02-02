<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="ClientDependency.Core.Mvc" %>

<% Html.RequiresJs("~/Js/jquery-1.3.2.min.js"); %>

<%--Demonstrates the use of using the PathNameAlias--%>
<% Html.RequiresCss("Controls.css", "Styles"); %>

<div class="control sidebar bg-primary-3 white">
	This is a side bar
	<% Html.RenderPartial("CustomControl"); %>
</div>