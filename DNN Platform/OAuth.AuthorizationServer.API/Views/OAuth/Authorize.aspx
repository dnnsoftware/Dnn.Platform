<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<OAuth.AuthorizationServer.API.Models.AccountAuthorizeModel>" %>
<%@ Import Namespace="DotNetOpenAuth.OAuth2" %>

<!DOCTYPE html>

<html>
    <head id="Head1" runat="server">
        <title>Authorize</title>
    </head>
    <body>
        <h2>Authorize -cathal</h2>
        <div><b>Warning</b>: Never give your login credentials to another web site or application.</div>
        <p>
            The <%: Model.Client.Name%> application is requesting to access private
            data in your account.  By clicking 'Approve' below, you authorize <%: Model.Client.Name %> to perform the following actions
        </p>
        <p><b>Requested access: </b></p>
        <ul>
            <%-- Display description of each scope to the user --%>
            <% foreach (var scope in Model.Scopes) 
                { %>
            <li><%: scope.Description %></li>
            <% } %>
        </ul>
        <% Html.BeginForm("ProcessAuthorization", "OAuth", FormMethod.Post); %>
            <%-- Need all this stuff in the form for use by DotNetOpenAuth.  Names of fields are defined by OAuth2 spec --%>
            <%: Html.AntiForgeryToken() %>
            <%: Html.Hidden("client_id", Model.AuthorizationRequest.ClientIdentifier) %>
            <%: Html.Hidden("redirect_uri", Model.AuthorizationRequest.Callback)%>
            <%: Html.Hidden("state", Model.AuthorizationRequest.ClientState) %>
            <%: Html.Hidden("scope", OAuthUtilities.JoinScopes(Model.AuthorizationRequest.Scope)) %>
            <%: Html.Hidden("response_type", (Model.AuthorizationRequest.ResponseType == DotNetOpenAuth.OAuth2.Messages.EndUserAuthorizationResponseType.AccessToken ? "token" : "code")) %>
            <div>
                <button type="submit" value="True" name="IsApproved">Approve</button>
                <button type="submit" value="False" name="IsApproved">Cancel</button>
            </div>
        <% Html.EndForm(); %>
        <script type="text/javascript">
            //<![CDATA[
            // Frame busting code (to protect us from being hosted in an iframe).
            // This protects us from click-jacking.
            if (document.location !== window.top.location) {
                window.top.location = document.location;
            }
            //]]>
        </script>
    </body>
</html>
