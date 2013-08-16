<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SmtpServerSettings.ascx.cs" 
    Inherits="DotNetNuke.Modules.Admin.AdvancedSettings.SmtpServerSettings" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
    function toggleSmtpCredentials(animation) {
        var smtpVal = $('#<%= optSMTPAuthentication.ClientID %> input:checked').val(); /*0,1,2*/
        if (smtpVal == "1") {
            animation ? $('#SMTPUserNameRow,#SMTPPasswordRow').slideDown() : $('#SMTPUserNameRow,#SMTPPasswordRow').show();
        }
        else {
            animation ? $('#SMTPUserNameRow,#SMTPPasswordRow').slideUp() : $('#SMTPUserNameRow,#SMTPPasswordRow').hide();
        }
    }
    
    function setUpDnnSmtpServerSettings() {
        toggleSmtpCredentials(false);
        $('#<%= optSMTPAuthentication.ClientID %>').change(function() {
            toggleSmtpCredentials(true);
        });
    }
    
    $(document).ready(function () {
        setUpDnnSmtpServerSettings();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            setUpDnnSmtpServerSettings();
        });
    });
} (jQuery, window.Sys));
</script>
<fieldset class="dnnhsSMTPSettings">
    <div class="dnnFormItem">
        <dnn:Label ID="plSMTPServer" ControlName="txtSMTPServer" runat="server" />
        <asp:TextBox ID="txtSMTPServer" runat="server" MaxLength="256" Width="225" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="plSMTPAuthentication" ControlName="optSMTPAuthentication" runat="server" />
        <asp:RadioButtonList ID="optSMTPAuthentication" CssClass="dnnHSRadioButtons" runat="server"
            RepeatLayout="Flow">
            <asp:ListItem Value="0" resourcekey="SMTPAnonymous" />
            <asp:ListItem Value="1" resourcekey="SMTPBasic" />
            <asp:ListItem Value="2" resourcekey="SMTPNTLM" />
        </asp:RadioButtonList>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="plSMTPEnableSSL" ControlName="chkSMTPEnableSSL" runat="server" />
        <asp:CheckBox ID="chkSMTPEnableSSL" runat="server" />
    </div>
    <div id="SMTPUserNameRow" class="dnnFormItem">
        <dnn:Label ID="plSMTPUsername" ControlName="txtSMTPUsername" runat="server" />
        <asp:TextBox ID="txtSMTPUsername" runat="server" MaxLength="256" Width="300" />
    </div>
    <div id="SMTPPasswordRow" class="dnnFormItem">
        <dnn:Label ID="plSMTPPassword" ControlName="txtSMTPPassword" runat="server" />
        <asp:TextBox ID="txtSMTPPassword" runat="server" MaxLength="256" Width="300" TextMode="Password" />
    </div>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdEmail" resourcekey="EmailTest" runat="server" CssClass="dnnPrimaryAction" /></li>
    </ul>
</fieldset>