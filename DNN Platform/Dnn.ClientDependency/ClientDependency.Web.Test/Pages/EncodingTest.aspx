<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Master.Master" AutoEventWireup="true" CodeBehind="EncodingTest.aspx.cs" Inherits="ClientDependency.Web.Test.Pages.EncodingTest" %>
<%@ Register Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" TagPrefix="CD" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript" src="http://ajax.microsoft.com/ajax/3.5/MicrosoftAjax.js "></script>
</asp:Content>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
    
    <CD:CssInclude ID="CssInclude1" runat="server" FilePath="Content.css" PathNameAlias="Styles" />        
    
    <!-- Include a JS file that is encoded in UTF8 format natively with BOM  //-->
	<CD:JsInclude ID="JsInclude1" runat="server" FilePath="~/Js/EncodingTestUTF8.js" />    
    <!-- Include a JS file that is encoded in UTF8 format natively without BOM  //-->
	<CD:JsInclude ID="JsInclude2" runat="server" FilePath="~/Js/EncodingTestUTF8WithoutBOM.js" />
    <!-- Include a JS file that is encoded in ANSI format natively  //-->
	<CD:JsInclude ID="JsInclude3" runat="server" FilePath="~/Js/EncodingTestANSI.js" />
    
    <!-- Include a JS file that is encoded in UCS2 Big Endian //-->
	<CD:JsInclude ID="JsInclude4" runat="server" FilePath="~/Js/EncodingTestUCS2BigEndian.js" />

    <!-- Include a JS file that is encoded in UCS2 Little Endian //-->
	<CD:JsInclude ID="JsInclude5" runat="server" FilePath="~/Js/EncodingTestUCS2LittleEndian.js" />

    <!-- Include a JS file that is downloaded via a web request and ensure encoding is ok //-->    
	<CD:JsInclude ID="JsInclude6" runat="server" FilePath="~/Services/MessageService.asmx/js" />

    

	<div class="mainContent">
		<h2>
			Test reading and combining of files with different encoding</h2>
		<p>
		    Some files are encoded differently and we need to make sure that when we read and combine these files that it is done seamlessly. In the past there were issues with BOM (byte order marks) 
            being interpretted as real text and included in the combined files. This page demonstrates that reading encoding of different file format is working.
		</p>
        <p>
            ClientDependency will initialize its StreamReaders and StreamWriters to use UTF-8 encoding always.
        </p>
		
	</div>
</asp:Content>
