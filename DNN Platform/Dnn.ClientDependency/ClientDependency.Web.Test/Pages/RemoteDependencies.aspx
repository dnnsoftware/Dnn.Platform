<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Master.Master" AutoEventWireup="true" CodeBehind="RemoteDependencies.aspx.cs" Inherits="ClientDependency.Web.Test.Pages.RemoteDependencies " %>
<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
        
    <!-- Get jQuery UI from CDN //-->
	<CD:JsInclude ID="JsInclude1" runat="server" FilePath="https://ajax.googleapis.com/ajax/libs/jqueryui/1.8.12/jquery-ui.min.js" Priority="3"  />	

    <CD:CssInclude ID="CssInclude1" runat="server" FilePath="Content.css" PathNameAlias="Styles" />

	<div class="mainContent">
		<h2>
			Some dependencies are from remote servers</h2>
		<p>
		    On this page, we've got the jquery library loaded from our local server with a priority of "1", but we've got the jquery UI registered with a file
            path from the Google CDN with a priority of "3".
		</p>				
        <p>
            In the source of this page, ClientDependency has split the registrations for JavaScript so that everthing found before the jQuery UI lib is compressed, combined, etc.. 
            then the jQuery UI lib is registered for downloading from the CDN, then everything after is again compressed, combined, etc...
        </p>
	</div>
</asp:Content>