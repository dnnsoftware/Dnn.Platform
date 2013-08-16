<%@ Control Inherits="DotNetNuke.Modules.Admin.Host.EditIPFilters" Language="C#" AutoEventWireup="false" CodeFile="EditIPFilters.ascx.cs" %>
<%@ Import Namespace="System" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<fieldset class="dnnhsEditIPSettings">
    <div class="dnnFormItem">
        <dnn:Label ID="plRuleSpecifity" ControlName="radIPOrRange" runat="server" />
        <asp:RadioButtonList ID="radIPOrRange" CssClass="dnnHSRadioButtons" runat="server" RepeatDirection="Horizontal"
                             RepeatLayout="Flow" Width="300">
            <asp:ListItem Value="single" resourcekey="IPSingle" Selected="True" />
            <asp:ListItem Value="range" resourcekey="IPRange" />
        </asp:RadioButtonList>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="plFirstIP" ControlName="txtFirstIP" runat="server" />
        <asp:TextBox ID="txtFirstIP" runat="Server" CssClass="dnnFormRequired" />
        <asp:RequiredFieldValidator id="valFirstIP" runat="server" ControlToValidate="txtFirstIP" Display="Dynamic" CssClass="dnnFormError NormalRed" resourcekey="IPValidation.ErrorMessage" />
        <asp:RegularExpressionValidator ValidationExpression="([0-9]{1,3}\.|\*\.){3}([0-9]{1,3}|\*){1}" Display="Dynamic" CssClass="dnnFormError NormalRed" resourcekey="IPValidation.ErrorMessage" ID="ipValidator" runat="server" ControlToValidate="txtFirstIP"></asp:RegularExpressionValidator>
    </div>
    <div id="EditIPRangeRow" class="dnnFormItem">
        <dnn:Label ID="plSubnet" ControlName="txtSubnet" runat="server" />
        <asp:TextBox ID="txtSubnet" runat="server" MaxLength="15" />
        <asp:RegularExpressionValidator ValidationExpression="([0-9]{1,3}\.|\*\.){3}([0-9]{1,3}|\*){1}" Display="Dynamic" CssClass="dnnFormError" resourcekey="IPValidation.ErrorMessage" ID="subnetValidator" runat="server" ControlToValidate="txtSubnet"></asp:RegularExpressionValidator>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="plRuleType" runat="server" ControlName="txtSubnet" />
        <asp:DropDownList runat="server" ID="cboType">
            <asp:ListItem resourcekey="IPAllow" Value="1" Selected="True" />
            <asp:ListItem resourcekey="IPDeny" Value="2" />
        </asp:DropDownList>
    </div>
</fieldset>
<ul class="dnnActions rfAddRule dnnClear"><li><asp:LinkButton ID="cmdSaveFilter"  runat="server" resourcekey="cmdSave" CssClass="dnnPrimaryAction" Text="Save" /></li><li><asp:LinkButton ID="lnkCancel"  runat="server" resourcekey="cmdCancel" CssClass="dnnSecondaryAction" Text="Cancel" CausesValidation="False" /></li></ul>
<script language="javascript" type="text/javascript">
    /*globals jQuery, window, Sys */
    (function($, Sys) {

        function toggleRange(animation) {
            var smtpVal = $('#<%= radIPOrRange.ClientID %> input:checked').val();
            if (smtpVal == "range") {
                animation ? $('#EditIPRangeRow').slideDown() : $('#EditIPRangeRow').show();
                $('#EditIPRangeRow').prop('disabled', false);
            } else {
                animation ? $('#EditIPRangeRow').slideUp() : $('#SEditIPRangeRow').hide();
                $('#EditIPRangeRow').prop('disabled', true);
            }
        }

        $('#<%= radIPOrRange.ClientID %>').change(function() {
            toggleRange(true);
        });
        $('#EditIPRangeRow').slideUp();

    }(jQuery, window.Sys));
</script>