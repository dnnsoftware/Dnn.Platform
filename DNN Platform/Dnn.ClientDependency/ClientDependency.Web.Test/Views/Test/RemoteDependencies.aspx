<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ClientDependency.Web.Test.Models.TestModel>" %>

<%@ Import Namespace="ClientDependency.Core.Mvc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Rogue Dependency Test
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <%--Load the jquery ui from cdn and make sure it isn't replaced--%>
    <% Html.RequiresJs("http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.0/jquery-ui.min.js", 3); %>

    <% Html.RequiresCss("Content.css", "Styles"); %>

    <div class="mainContent">
        <h2>Some dependencies are from remote servers</h2>
        <div>
            <p>
                On this page, we've got the jquery library loaded from our local server with a priority of '1', but we've got the jquery UI registered with a file path from the Google CDN with a priority of '3'
            </p>
            <p>
                In the source of this page, ClientDependency has split the registrations for JavaScript so that everthing found before the jQuery UI lib is compressed, combined, etc.. then the jQuery UI lib is registered for downloading from the CDN, then everything after is again compressed, combined, etc...
            </p>
        </div>
    </div>
</asp:Content>
