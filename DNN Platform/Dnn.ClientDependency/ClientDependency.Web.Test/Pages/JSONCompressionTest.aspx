<%@ Page Language="C#" MasterPageFile="~/Pages/Master.Master" AutoEventWireup="true" CodeBehind="JSONCompressionTest.aspx.cs" Inherits="ClientDependency.Web.Test.Pages.JSONCompressionTest" %>
<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
    <CD:CssInclude ID="CssInclude1" runat="server" FilePath="Content.css" PathNameAlias="Styles" />
    <CD:JsInclude ID="JsInclude2" runat="server" FilePath="~/Js/jquery-1.3.2.min.js" Priority="1" />

	<CD:JsInclude ID="JsInclude1" runat="server" FilePath="~/Js/MessageService.js" Priority="100"   />	

    <asp:ScriptManager runat="server"></asp:ScriptManager>

	<div class="mainContent">
		<h2>
			We're calling a JSON web service here</h2>
		<p>
		    By setting MIME Type compressions, we can compress the JSON output from the server.
		    By default the server will output cache the response and vary it based on all parameters
		    passed in. You can also specify a regex file path to match in case you don't want all
		    specified requests for the mime types compressed. This can be done using IIS 7 as well
		    but this will also allow you to compress in IIS 6, plus gives you more control over exactly
		    what is getting compressed.
		</p>
		<p>
		    Web service response: 
		</p>
		<div id="response" style="font-weight:bold;">...loading...</div>
	</div>
	
	<script type="text/javascript">
	    $(document).ready(function() {
	        setTimeout(function() {
	            Messaging.GetMessage(function(rVal) {
	                $("#response").html(rVal);
	            });
	        }, 1000);
	    });
	</script>
	
</asp:Content>