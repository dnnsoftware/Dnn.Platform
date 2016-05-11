<%@ Control language="C#" AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.Modules.Admin.ViewProfile.ViewProfile" Codebehind="ViewProfile.ascx.cs" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<div id="profileOutput" runat="server" style="display:none" data-bind="visible: Visible" ViewStateMode="Disabled"></div>
<asp:Label id="noPropertiesLabel" runat="server" resourcekey="NoProperties" Visible="false" />
<div id="buttonPanel" runat="server" ViewStateMode="Disabled">
    <ul class="dnnActions dnnClear">
        <li><asp:HyperLink id="editLink" runat="server" resourcekey="Edit" CssClass="dnnPrimaryAction" /></li>
    </ul>
</div>
<script language="javascript" type="text/javascript">

    jQuery(document).ready(function($) {
        function ProfileViewModelModule<%=ModuleContext.ModuleId.ToString(CultureInfo.InvariantCulture) %>() {
            var self = this;
            self.AboutMeText = '<%=HttpUtility.JavaScriptStringEncode(LocalizeString("AboutMe")) %>';
            self.LocationText = '<%=HttpUtility.JavaScriptStringEncode(LocalizeString("Location")) %>';
            self.GetInTouchText = '<%=HttpUtility.JavaScriptStringEncode(LocalizeString("GetInTouch")) %>';
            self.EmptyAboutMeText = '<%=HttpUtility.JavaScriptStringEncode(LocalizeString("EmptyAboutMe")) %>';
            self.EmptyLocationText = '<%=HttpUtility.JavaScriptStringEncode(LocalizeString("EmptyLocation")) %>';
            self.EmptyGetInTouchText = '<%=HttpUtility.JavaScriptStringEncode(LocalizeString("EmptyGetInTouch")) %>';
            
            <% = ProfileProperties %>

            self.Location = ko.computed(function() {
                var city = self.City();
                var region = self.Region();
                var location = (city != null) ? city : '';
                if (location != '' && region != null && region != '') {
                    location += ', ';
                }
                if (region != null) {
                    location += region;
                }

                return location;
            });

            self.Visible = true;
        };

        try {
            ko.applyBindings(new ProfileViewModelModule<%=ModuleContext.ModuleId.ToString(CultureInfo.InvariantCulture) %>(), document.getElementById($('#<%= profileOutput.ClientID %>').attr("id")));
        } catch (e) {
    
        }

        });

</script>
