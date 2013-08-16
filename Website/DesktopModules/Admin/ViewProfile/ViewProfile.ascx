<%@ Control language="C#" CodeFile="ViewProfile.ascx.cs" AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.Modules.Admin.Users.ViewProfile" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<dnn:DnnJsInclude ID="DnnJsInclude" runat="server" FilePath="~/Resources/Shared/scripts/knockout.js" />

<div id="profileOutput" runat="server" style="display:none" data-bind="visible: Visible"></div>
<asp:Label id="noPropertiesLabel" runat="server" resourcekey="NoProperties" Visible="false" />
<div id="buttonPanel" runat="server">
    <ul class="dnnActions dnnClear">
        <li><asp:HyperLink id="editLink" runat="server" resourcekey="Edit" CssClass="dnnPrimaryAction" /></li>
    </ul>
</div>
<script language="javascript" type="text/javascript">

    jQuery(document).ready(function($) {
        function ProfileViewModelModule<%=ModuleContext.ModuleId.ToString(CultureInfo.InvariantCulture) %>() {
            var self = this;
            self.AboutMeText = '<%=Localization.GetSafeJSString(LocalizeString("AboutMe")) %>';
            self.LocationText = '<%=Localization.GetSafeJSString(LocalizeString("Location")) %>';
            self.GetInTouchText = '<%=Localization.GetSafeJSString(LocalizeString("GetInTouch")) %>';
            self.EmptyAboutMeText = '<%=Localization.GetSafeJSString(LocalizeString("EmptyAboutMe")) %>';
            self.EmptyLocationText = '<%=Localization.GetSafeJSString(LocalizeString("EmptyLocation")) %>';
            self.EmptyGetInTouchText = '<%=Localization.GetSafeJSString(LocalizeString("EmptyGetInTouch")) %>';
            
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
