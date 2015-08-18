<%@ Page Language="C#" AutoEventWireup="true" CodeFile="OAUTHAuthorize2.aspx.cs" Inherits="OAUTHAuthorize2" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <h2>Authorize -cathal</h2>
        <div><b>Warning</b>: Never give your login credentials to another web site or application.</div>
        <p>
            The Awesome Client 1 application is requesting to access private
            data in your account.  By clicking 'Approve' below, you authorize Awesome Client 1 to perform the following actions
        </p>
        <p><b>Requested access: </b></p>
        <ul><li>DNN-ALL</li></ul>

    <form id="Form"  action="<%="http://" + DotNetNuke.Common.Globals.GetDomainName(HttpContext.Current.Request) + "/DesktopModules/internalservices/API/OAUth/ProcessAuthorization" %>" method="post">
      <input id="client_id" name="client_id" type="hidden" value="client1" />
            <input id="redirect_uri" name="redirect_uri" type="hidden" value="<% =Request.QueryString["redirect_uri"] %>" />
            <input id="state" name="state" type="hidden" value="j2FlXtsh_9ssGibjfY7p2" />
            <input id="scope" name="scope" type="hidden" value="DNN-ALL" />
            <input id="response_type" name="response_type" type="hidden" value="token" />
<div>
                <button type="submit" value="True" name="IsApproved">Approve</button>
                <button type="submit" value="False" name="IsApproved">Cancel</button>
            </div>

    </form>
</body>
</html>
