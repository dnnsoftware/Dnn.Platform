<%@ Page Language="C#" MasterPageFile="~/Pages/Master.Master" AutoEventWireup="true" CodeBehind="HtmlAttributes.aspx.cs" Inherits="ClientDependency.Web.Test.Pages.HtmlAttributes" %>

<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>
<asp:Content runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
	<CD:CssInclude ID="CssInclude1" runat="server" FilePath="Content.css" PathNameAlias="Styles" />
	<CD:CssInclude ID="CssInclude2" runat="server" FilePath="relative.css" CssMedia="Screen" />
    <CD:CssInclude ID="CssInclude3" runat="server" FilePath="Print.css" PathNameAlias="Styles" HtmlAttributesAsString="media:'print,projection'" />
	<CD:JsInclude ID="JsInclude1" runat="server" FilePath="JQueryTemplate.js" PathNameAlias="Scripts" HtmlAttributesAsString="type:text/html" />
    <div class="mainContent">
		<h2>
			Testing some custom Html attributes on script/link tags</h2>
		<p>
			Here were rendering out a stylesheet that has a media="print" attribute and a JS file that has a type="text/html" attribute (for things like jquery templates)</p>        
	</div>
</asp:Content>
