<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Master.Master" AutoEventWireup="true" CodeBehind="LazyLoadProviderTest.aspx.cs" Inherits="ClientDependency.Web.Test.Pages.LazyLoadProviderTest" %>
<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
	<div class="mainContent">
		<h2>
			Using the Lazy Load Provider and dynamically registering my css file in the code behind.</h2>
		<p>
			This example actually changes the current request's ClientDependencyLoader's default provider at runtime to the LazyLoadProvider</p>
		<p>
		    //Changes the provider to be used at runtime in the code behind<br />
            ClientDependencyLoader.Instance.ProviderName = LazyLoadProvider.DefaultName;</p>
        <p>
            //dynamically register the dependency in the code behind<br />
            ClientDependencyLoader.Instance.RegisterDependency("Content.css", "Styles", ClientDependencyType.Css);</p>
	</div>
</asp:Content>