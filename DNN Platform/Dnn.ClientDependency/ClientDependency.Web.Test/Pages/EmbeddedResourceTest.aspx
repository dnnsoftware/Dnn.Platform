<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Master.Master" AutoEventWireup="true" CodeBehind="EmbeddedResourceTest.aspx.cs" Inherits="ClientDependency.Web.Test.Pages.EmbeddedResourceTest" %>

<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
	<CD:CssInclude ID="CssInclude1" runat="server" FilePath="Content.css" PathNameAlias="Styles" />
	<CD:CssInclude ID="CssInclude2" runat="server" FilePath="relative.css" />
	<div class="mainContent">
		<h2>
			Registering an embedded resource via ClientDependency!</h2>
		<p>
			This font is big because an embedded style resource has been added to the ClientDependency output
        </p>			
	</div>
</asp:Content>
