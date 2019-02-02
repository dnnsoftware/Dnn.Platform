<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="ClientDependency.Core.Mvc" %>

<% Html.RequiresJs("~/Js/jquery-1.3.2.min.js", 1); %>

<%--Demonstrates the use of using the PathNameAlias--%>
<% Html.RequiresCss("Controls.css", "Styles"); %>

<div class="control white bg-complement-2">
    <a href="<%= Url.Content("~/") %>" class="f-primary-4 bg-complement-2">Return to landing page</a>
</div>
<div class="control header bg-primary-3 white">
	This is a header
	<ul >
	    <%foreach(var f in Directory.GetFiles(Server.MapPath("~/Views/Test")).Where(x => x.EndsWith(".cshtml") || x.EndsWith(".aspx"))) { %>
            <li><%= Html.ActionLink(Path.GetFileNameWithoutExtension(f), Path.GetFileNameWithoutExtension(f), null, new { @class = "white" }) %></li>
        <%} %>
	</ul>
</div>