<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Control Language="C#" Inherits="DotNetNuke.Modules.Admin.Security.SendPassword" AutoEventWireup="false" Codebehind="SendPassword.ascx.cs" %>
<div class="dnnForm dnnSendPassword dnnClear">

	<asp:panel id="pnlRecover" runat="server" >
        
        <div class="dnnFormMessage dnnFormInfo"><asp:Label ID="lblHelp" runat="Server" /></div>
	    
        <div id="divPassword" runat="server" class="dnnSendPasswordContent">
		    <div class="dnnFormItem" id="divUsername" runat="server">
			    <dnn:label id="plUsername" controlname="txtUsername" runat="server" />
			    <asp:textbox id="txtUsername" CssClass="dnnFormRequired" runat="server" />
		    </div>
		    <div class="dnnFormItem" id="divEmail" runat="server">
			    <dnn:label id="plEmail" controlname="txtEmail" runat="server" />
			    <asp:textbox id="txtEmail" runat="server" />
		    </div>
		    <div id="divCaptcha" runat="server" class="dnnFormItem">
			    <dnn:label id="plCaptcha" controlname="ctlCaptcha" runat="server" />
			    <dnn:captchacontrol id="ctlCaptcha" captchawidth="130" captchaheight="40" runat="server" errorstyle-cssclass="dnnFormMessage dnnFormError" />
		    </div>
	    </div>

    </asp:panel>

	<ul class="dnnActions dnnClear">
	    <li id="liSend" runat="server"><asp:LinkButton id="cmdSendPassword" cssclass="dnnPrimaryAction" runat="server" /></li>
	    <li id="liCancel" runat="server"><asp:HyperLink id="lnkCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
	</ul>

</div>