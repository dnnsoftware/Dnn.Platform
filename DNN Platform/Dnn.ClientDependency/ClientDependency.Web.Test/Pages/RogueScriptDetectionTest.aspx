<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Master.Master" AutoEventWireup="true" CodeBehind="RogueScriptDetectionTest.aspx.cs" Inherits="ClientDependency.Web.Test.Pages.RogueScriptDetectionTest" %>
<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    
    <%--Standard style registration--%>
    <CD:CssInclude ID="CssInclude1" runat="server" FilePath="Content.css" PathNameAlias="Styles" /> 

    <%--Rogue style registration--%>
    <link rel="Stylesheet" type="text/css" href="../Css/OverrideStyles.css" />
    
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    
    
    <%--Load the jquery ui from cdn and make sure it isn't replaced--%>
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.0/jquery-ui.min.js"></script>
    
    <%--Load first Rogue Script--%>
    <script type="text/javascript" src="../Js/RogueScript1.js"></script>
    
    <div class="mainContent">
		<h2>
			Replacing rogue scripts/styles with composite files</h2>
		<p>
		    This page demonstrates the replacement of 'Rogue' script/style (link) tags that exist in the raw html
		    of the page. These scripts get replaced with the compression handler URL and are handled then
		    just like other scripts that are being rendered via a client dependency object.		    
		</p>
		<p>
		    The term 'Rogue' refers to a script/styles that hasn't been registered on the page with the ClientDependency
		    framework and instead is registered as raw script/style tags.
		</p>
		<p>
		    IMPORTANT: Please note that although Rogue scripts/styles get replaced with compressed scripts/styles 
		    using this framework, it still means that there will be more requests when they are not
		    properly registered because rogue scripts are not combined into one request!
		</p>
	</div>
	
	<%--Load a second Rogue Script--%>
	<script type="text/javascript" src="../Js/RogueScript2.js"></script>	
	
</asp:Content>
