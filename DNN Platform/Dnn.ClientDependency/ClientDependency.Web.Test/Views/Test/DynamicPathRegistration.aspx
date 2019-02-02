<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ClientDependency.Web.Test.Models.TestModel>" %>

<%@ Import Namespace="ClientDependency.Core.Mvc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Dynamic Path Registration
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <% Html.RegisterPathAlias("NewCssPath", "~/Css/TestPath"); %>

    <% Html.RequiresCss("Content.css", "Styles"); %>
    <% Html.RequiresCss("BodyGradient.css", "NewCssPath"); %>
    <% Html.RequiresJs("HeaderOrange.js", "NewJsPath"); %>


    <div class="mainContent">
        <h2>Dynamic path registration</h2>
        <div>
            <p>
                In the MVC Action for this page, we've dynamically added a path registration and have dynamically added a 2nd one in the view.
            </p>
            <p>
                There are a few extension methods to acheive this, the direct way is to get an instance of the DependencyRenderer by calling the extension method
GetLoader() on either the ControllerContext, ViewContext or HttpContextBase, then you can just use the AddPath methods.
                <br />
                Otherwise, if you are working in a view, you can use the HtmlHelper methods: RegisterPathAlias
            </p>
        </div>
    </div>

</asp:Content>
