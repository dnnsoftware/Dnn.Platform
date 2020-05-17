﻿<%@ Control Language="C#" AutoEventWireup="true" Inherits="DotNetNuke.Modules.Admin.Users.Register" Codebehind="Register.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls.Internal" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>

<div class="dnnForm dnnRegistrationForm" id="RegistrationForm" runat="server">
    <div class="dnnFormItem">
        <div class="dnnFormMessage dnnFormInfo"><asp:label id="userHelpLabel" runat="server" ViewStateMode="Disabled"/></div>
    </div>
    <br/>
    <div class="dnnFormItem dnnClear">
        <dnn:DnnFormEditor id="userForm" runat="Server" FormMode="Short" EncryptIds="True" />
        <div class="dnnSocialRegistration">
            <div id="mainContainer">
                <ul class="buttonList">
                    <asp:PlaceHolder ID="socialLoginControls" runat="server"/>
                </ul>
            </div>
        </div>
    </div>
    <div id="captchaRow" runat="server" visible="false" class="dnnFormItem dnnCaptcha">
        <dnn:label id="captchaLabel" controlname="ctlCaptcha" runat="server" />
        <dnn:captchacontrol id="ctlCaptcha" captchawidth="130" captchaheight="40" ErrorStyle-CssClass="dnnFormMessage dnnFormError dnnCaptcha" runat="server" />
    </div>
    <input runat="server" id="gotcha" type="text" name="gotcha" style="display: none;" autocomplete="off" aria-label="gotcha" />
    <ul id="actionsRow" runat="server" class="dnnActions dnnClear">
        <li><asp:LinkButton id="registerButton" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdRegister" /></li>
        <li><asp:HyperLink ID="cancelLink" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" CausesValidation="false" /></li>
    </ul>
</div>
<asp:HyperLink ID="closeLink" runat="server" CssClass="dnnSecondaryAction" resourcekey="Close" CausesValidation="false" Visible="False" />
<script type="text/javascript">
    $(function () {
        $('.dnnFormItem .dnnLabel').each(function () {
            var next = $(this).next();
            if (next.hasClass('dnnFormRequired'))
                $(this).find('span').addClass('dnnFormRequired');
        });
		
		// SOCIAL-2069: fix for WP8
		if (navigator.userAgent.match(/IEMobile\/10\.0/)) {
			$(window.parent).resize(function () {				
				$('.dnnFormItem .dnnFormMessage.dnnFormError').hide();
			});
		}		
    });
</script>
