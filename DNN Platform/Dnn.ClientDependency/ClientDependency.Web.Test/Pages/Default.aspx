<%@ Page Language="C#" MasterPageFile="~/Pages/Master.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ClientDependency.Web.Test.Pages._Default" %>

<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>
<asp:Content runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
	<CD:CssInclude ID="CssInclude1" runat="server" FilePath="Content.css" PathNameAlias="Styles" />
	<CD:CssInclude ID="CssInclude2" runat="server" FilePath="relative.css" />
	<div class="mainContent">
		<h2>
			Using the default provider specified in the web.config</h2>
		<p>
			Nothing fancy here, just rendering the script and style blocks using the default provider. 
			This library ships with the LoaderControlProvider set to the default
			provider which will render the script/style blocks where the ClientDependencyLoader is placed in page/control markup.</p>
        <p class="red">
            This paragraph is styled using a relative css path
        </p>			
	</div>
</asp:Content>
