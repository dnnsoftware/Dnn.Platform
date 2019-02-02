<%@ Page Language="C#" MasterPageFile="~/Pages/Master.Master" AutoEventWireup="true" CodeBehind="HtmlIncludeTest.aspx.cs" Inherits="ClientDependency.Web.Test.Pages.HtmlIncludeTest" %>

<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>
<asp:Content runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
	<CD:CssInclude ID="CssInclude1" runat="server" FilePath="Content.css" PathNameAlias="Styles" />
	<CD:CssInclude ID="CssInclude2" runat="server" FilePath="relative.css" />
    <CD:HtmlInclude runat="server">
      <link rel="stylesheet" type="text/css" href="/css/content.css" />
      <link rel="stylesheet" type="text/css" href="relative.css" />
      <script src='/js/htmlincludetest1.js' type='text/javascript'></script>
      <script type="text/javascript" src="/js/htmlincludetest2.js"></script>
    </CD:HtmlInclude>
	<div class="mainContent">
		<h2>
			Test using the HtmlInclude control to register CSS and JS includes</h2>
		<p>
			On this page multiple includes of type CSS and JS are being loaded using the HtmlInclude control 
            which parses regular HTML include tags to find dependencies.</p>
        <p class="red">
            This paragraph is styled using a relative css path
        </p>			
	</div>
</asp:Content>
