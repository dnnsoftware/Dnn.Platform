<%@ Control Language="C#" Inherits="DotNetNuke.Modules.Admin.Authentication.DNN.Login" AutoEventWireup="false" CodeBehind="Login.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<div class="dnnForm dnnLoginService dnnClear">
    <div class="dnnFormItem">
		<div class="dnnLabel">
			<asp:label id="plUsername" AssociatedControlID="txtUsername" runat="server" CssClass="dnnFormLabel" />
		</div>        
        <asp:textbox id="txtUsername" runat="server" />
    </div>
    <div class="dnnFormItem">
		<div class="dnnLabel">
			<asp:label id="plPassword" AssociatedControlID="txtPassword" runat="server" resourcekey="Password" CssClass="dnnFormLabel" ViewStateMode="Disabled" />
		</div>
        <asp:textbox id="txtPassword" textmode="Password" runat="server" />
    </div>
    <div class="dnnFormItem" id="divCaptcha1" runat="server" visible="false">		
        <asp:label id="plCaptcha" AssociatedControlID="ctlCaptcha" runat="server" resourcekey="Captcha" CssClass="dnnFormLabel" />
    </div>
    <div class="dnnFormItem dnnCaptcha" id="divCaptcha2" runat="server" visible="false">
        <dnn:captchacontrol id="ctlCaptcha" captchawidth="130" captchaheight="40" runat="server" errorstyle-cssclass="dnnFormMessage dnnFormError dnnCaptcha" ViewStateMode="Disabled" />
    </div>
    <div class="dnnFormItem">
        <asp:label id="lblLogin" runat="server" AssociatedControlID="cmdLogin" CssClass="dnnFormLabel" ViewStateMode="Disabled" />
        <asp:LinkButton id="cmdLogin" resourcekey="cmdLogin" cssclass="dnnPrimaryAction" text="Login" runat="server" CausesValidation="false" />
		<asp:HyperLink id="cancelLink" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" CausesValidation="false" />
        
    </div>
	<div class="dnnFormItem">
		<asp:label id="lblLoginRememberMe" runat="server" CssClass="dnnFormLabel" />
		<span class="dnnLoginRememberMe"><asp:checkbox id="chkCookie" resourcekey="Remember" runat="server" /></span>
	</div>
    <div class="dnnFormItem">
        <span class="dnnFormLabel">&nbsp;</span>
        <div class="dnnLoginActions">
            <ul class="dnnActions dnnClear">
                <li id="liRegister" runat="server"><asp:HyperLink ID="registerLink" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdRegister" ViewStateMode="Disabled" /></li>                
                <li id="liPassword" runat="server"><asp:HyperLink ID="passwordLink" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdPassword" ViewStateMode="Disabled" /></li>
            </ul>
        </div>
    </div>
</div>
<script type="text/javascript">
	/*globals jQuery, window, Sys */
	(function ($, Sys) {
		function setUpLogin() {
			var actionLinks = $("a#dnn_ctr<%#ModuleId > Null.NullInteger ? ModuleId.ToString() : ""%>_Login_Login_DNN_cmdLogin");
			actionLinks.click(function () {
				if ($(this).hasClass("dnnDisabledAction")) {
					return false;
				}

				actionLinks.addClass("dnnDisabledAction");
			});
		}
		
		$(document).ready(function () {

			$('.dnnLoginService').on('keydown', function(e) {
				if (e.keyCode === 13) {
					var $loginButton = $('#dnn_ctr<%#ModuleId > Null.NullInteger ? ModuleId.ToString() : ""%>_Login_Login_DNN_cmdLogin');
					if ($loginButton.hasClass("dnnDisabledAction")) {
						return false;
					}

					$loginButton.addClass("dnnDisabledAction");
					eval($loginButton.attr('href'));
					e.preventDefault();
					return false;
				}
			});

			setUpLogin();
			Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
				setUpLogin();
			});
		});
	}(jQuery, window.Sys));
</script>  
