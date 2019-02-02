<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Master.Master" AutoEventWireup="true" CodeBehind="ForcedProviders.aspx.cs" Inherits="ClientDependency.Web.Test.Pages.FocedProviders" %>
<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
    
    <!-- Force page header provider //-->
    <CD:CssInclude ID="CssInclude2" runat="server" FilePath="OverrideStyles.css" PathNameAlias="Styles" ForceProvider="PageHeaderProvider" />    
    
    <!-- Force lazy load provider //-->
	<CD:JsInclude ID="JsInclude1" runat="server" FilePath="~/Js/SomeLazyLoadScript.js" ForceProvider="LazyLoadProvider"  />
	<CD:CssInclude ID="CssInclude1" runat="server" FilePath="Test.css" PathNameAlias="Styles" ForceProvider="LazyLoadProvider" />

	<div class="mainContent">
		<h2>
			Some dependencies are being forced to use certain providers here!</h2>
		<p>
		    On this page there are 3 providers being run: LoaderControlProvider (default), PageHeaderProvider and the LazyLoadProvider.
		    If you have a look at the html source, you can see where the scripts and stylesheets are being loaded.
		    You can also turn debug on/off for various providers and you'll see that it renders composite script/css paths or just standard ones for each of the locations.
		</p>
		<p class="lazyLoaded">
			
		</p>
		<p>
			An example of how to force a provider with the JsInclude control:<br />
			&lt;CD:JsInclude ID="JsInclude2" runat="server" FilePath="~/Js/SomeLazyLoadScript.js" ForceProvider="LazyLoadProvider" /&gt; </p>
	</div>
</asp:Content>