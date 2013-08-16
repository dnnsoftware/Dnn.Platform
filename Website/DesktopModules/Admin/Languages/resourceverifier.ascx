<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Languages.ResourceVerifier" CodeFile="ResourceVerifier.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<div class="dnnForm dnnResourceVerifier dnnClear">
	<div class="dnnFormItem"><asp:placeholder id="PlaceHolder1" runat="server"></asp:placeholder></div>
	<ul class="dnnActions dnnClear">
		<li><asp:HyperLink id="cmdCancel" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdCancel" /></li>
	</ul>
</div>