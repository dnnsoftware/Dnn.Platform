<%@ Control Language="C#" Inherits="DotNetNuke.Modules.Admin.Authentication.DNN.Login" AutoEventWireup="false" CodeBehind="Login.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls.Internal" Assembly="DotNetNuke.Web" %>
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
		<asp:label id="lblLoginRememberMe" runat="server" CssClass="dnnFormLabel" />
		<span class="dnnLoginRememberMe"><asp:checkbox id="chkCookie" resourcekey="Remember" runat="server" /></span>
	</div>
    <div class="dnnFormItem">
        <asp:label id="lblLogin" runat="server" AssociatedControlID="cmdLogin" CssClass="dnnFormLabel" ViewStateMode="Disabled" />
        <asp:LinkButton id="cmdLogin" resourcekey="cmdLogin" cssclass="dnnPrimaryAction" text="Login" runat="server" CausesValidation="false" />
		<asp:HyperLink id="cancelLink" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" CausesValidation="false" />        
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
<dnn:DnnScriptBlock runat="server">
    <script type="text/javascript">
        /*globals jQuery, window, Sys */
        (function ($, Sys) {
            const disabledActionClass = "dnnDisabledAction";
            const actionLinks = $('a[id^="dnn_ctr<%=ModuleId > Null.NullInteger ? ModuleId.ToString() : ""%>_Login_Login_DNN"]');
            function isActionDisabled($el) {
                return $el && $el.hasClass(disabledActionClass);
            }
            function disableAction($el) {
                if ($el == null || $el.hasClass(disabledActionClass)) {
                    return;
                }
                $el.addClass(disabledActionClass);
            }
            function enableAction($el) {
                if ($el == null) {
                    return;
                }
                $el.removeClass(disabledActionClass);
            }
            function setUpLogin() {                
                $.each(actionLinks || [], function (index, action) {
                    var $action = $(action);
                    $action.click(function () {
                        var $el = $(this);
                        if (isActionDisabled($el)) {
                            return false;
                        }
                        disableAction($el);
                    });
                });
            }
		
            $(document).ready(function () {
                $(document).on('keydown', '.dnnLoginService', function (e) {
                    if ($(e.target).is('input:text,input:password') && e.keyCode === 13) {
                        var $loginButton = $('#dnn_ctr<%=ModuleId > Null.NullInteger ? ModuleId.ToString() : ""%>_Login_Login_DNN_cmdLogin');
                        if (isActionDisabled($loginButton)) {
                            return false;
                        }
                        disableAction($loginButton);
                        window.setTimeout(function () { eval($loginButton.attr('href')); }, 100);
                        e.preventDefault();
                        return false;
                    }
                });

                setUpLogin();
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                    $.each(actionLinks || [], function (index, item) {
                        enableAction($(item));
                    });
                    setUpLogin();
                });
            });
        }(jQuery, window.Sys));
    </script>
</dnn:DnnScriptBlock>
