<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ClientDependency.Web.Test.Models.TestModel>" %>

<%@ Import Namespace="ClientDependency.Core.Mvc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Custom Html Attributes Test
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <%--Load the jquery ui from cdn and make sure it isn't replaced--%>
    <% Html.RequiresJs("http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.0/jquery-ui.min.js", 3); %>
    <% Html.RequiresJs("JQueryTemplate.js", "Scripts", new { type = "text/html" }); %>

    <% Html.RequiresCss("Content.css", "Styles"); %>
    <% Html.RequiresCss("Print.css", "Styles", new { media = "print" }); %>

    <div class="mainContent">
        <h2>Some dependencies are have custom html attributes</h2>
        <div>
            <p>
                On this page we have 2 dependencies registered with custom html attributes:
            </p>
            <ul>
                <li>A print style sheet with a custom media='print' attribute</li>
                <li>A js dependency with a type='text/html' which could be used as a jquery template</li>
            </ul>
        </div>
    </div>
</asp:Content>
