<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FriendlyUrls.ascx.cs" Inherits="DotNetNuke.Modules.UrlManagement.FriendlyUrls" %>
<%@ Register TagPrefix="dnn" TagName="FriendlyUrls" Src="~/DesktopModules/Admin/HostSettings/FriendlyUrls.ascx" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelcontrol.ascx" %>

<asp:Placeholder ID="basicUrlPanel" runat="server" Visible="True">
    <div class="dnnFormItem">
        <dnn:Label ID="plUseFriendlyUrls" ControlName="chkUseFriendlyUrls" runat="server" />
        <asp:CheckBox ID="chkUseFriendlyUrls" runat="server" />
    </div>
</asp:Placeholder>
<div id="friendlyUrlsRow" class="dnnFormItem" runat="server">
    <dnn:FriendlyUrls ID="friendlyUrls" runat="server" />
</div>

<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {

    function setUpDnnFriendlyUrlSettings() {
        if (document.getElementById('<%=chkUseFriendlyUrls.ClientID %>')) {
            toggleSection('friendlyUrlsRow', document.getElementById('<%=chkUseFriendlyUrls.ClientID %>').checked);            

            $("#<%=chkUseFriendlyUrls.ClientID %>").change(function (e) {
                toggleSection('friendlyUrlsRow', this.checked);
            });
        }
    }

    $(document).ready(function () {
        setUpDnnFriendlyUrlSettings();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            setUpDnnFriendlyUrlSettings();
        });
    });
    
    function toggleSection(id, isToggled) {
        $("div[id$='" + id + "']").toggle(isToggled);
    }

}(jQuery, window.Sys));
</script>
