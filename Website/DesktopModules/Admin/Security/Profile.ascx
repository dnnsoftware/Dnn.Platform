<%@ Control Language="C#" AutoEventWireup="false" Inherits="DesktopModules.Admin.Security.DNNProfile" Codebehind="Profile.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
    function setUpProfile() {
    	$('.dnnButtonDropdown').dnnSettingDropdown();
        $('#<%=ProfileProperties.ClientID%>').parent().dnnPanels();
        $('input[data-name="Country"]').attr('autocomplete', getRandomString());
        $('input[data-name="Region"]').attr('autocomplete', getRandomString());

    }
    function getRandomString() {
        return (Math.random() + 1).toString(36).substring(2, 6) + (Math.random() + 1).toString(36).substring(2, 6);
    }

    $(document).ready(function () {
        setUpProfile();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            setUpProfile();
        });
    });
} (jQuery, window.Sys));
</script>
<div class="dnnForm dnnProfile dnnClear">
	<dnn:ProfileEditorControl id="ProfileProperties" runat="Server" 
            GroupByMode="Section"
            ViewStateMode="Disabled"
            enableClientValidation="true" />
    <div class="dnnClear"></div>
	<ul id="actionsRow" runat="server" class="dnnActions dnnClear">
		<li><asp:LinkButton class="dnnPrimaryAction" id="cmdUpdate" runat="server" resourcekey="cmdUpdate" /></li>
	</ul>
</div>