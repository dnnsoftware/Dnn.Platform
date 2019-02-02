<%@ Page Language="C#" MasterPageFile="~/Pages/Master.Master" AutoEventWireup="true" CodeBehind="DependencyGroups.aspx.cs" Inherits="ClientDependency.Web.Test.Pages.DependencyGroups" %>

<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>
<asp:Content runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
	<CD:CssInclude ID="CssInclude1" runat="server" FilePath="Content.css" PathNameAlias="Styles" />
    <CD:JsInclude ID="JsInclude3" runat="server" FilePath="AnotherTest.js" PathNameAlias="Scripts"/>
    <CD:JsInclude ID="JsInclude2" runat="server" FilePath="jquery-ui-1.8.13.custom.min.js" PathNameAlias="Scripts" Group="1"  Priority="2"/>
    <CD:JsInclude ID="JsInclude1" runat="server" FilePath="jquery-1.3.2.min.js" PathNameAlias="Scripts" Group="1" Priority="1" />
    	
	<div class="mainContent">
		<h2>
			Dependency groups</h2>
		<p>
			On this page, we've got the jquery libraries grouped so that they are shared across pages, other dependencies are loaded normally.</p>
        
	</div>
</asp:Content>
