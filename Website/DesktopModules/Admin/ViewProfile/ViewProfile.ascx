<%@ Control Language="C#" AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.Modules.Admin.ViewProfile.ViewProfile" CodeBehind="ViewProfile.ascx.cs" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<div id="profileOutput" runat="server" style="display: none" data-bind="visible: Visible" viewstatemode="Disabled"></div>
<asp:Label ID="noPropertiesLabel" runat="server" resourcekey="NoProperties" Visible="false" />
<div id="buttonPanel" runat="server" viewstatemode="Disabled">
    <ul class="dnnActions dnnClear">
        <li>
            <asp:HyperLink ID="editLink" runat="server" resourcekey="Edit" CssClass="dnnPrimaryAction" /></li>
    </ul>
</div>
<asp:Panel runat="server" ID="pnlScripts">
    <script language="javascript" type="text/javascript">
        jQuery(document).ready(function($) {
            var $container = $('#<%= profileOutput.ClientID %>');
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
                    var city = typeof self.City === "function" ? self.City() : "";
                    var region = typeof self.Region === "function" ? self.Region() : "";
                    var location = (city != null) ? city : '';
                    if (location != '' && region != null && region != '') {
                        location += ', ';
                    }
                    if (region != null) {
                        location += region;
                    }

                    return location;
                });

                //process all binded functions to apply empty values on not exist properties.
                $container.find('*[data-bind]').each(function() {
                    var binding = $(this).attr('data-bind');

                    $.each(binding.match(/\w+?\(\)/gi), function (index, name) {
                        name = name.replace('()', '');
                        if (typeof self[name] === "undefined") {
                            self[name] = ko.observable('');
                        }
                    });

                    $.each(binding.split(','), function (index, part) {
                        var parts = part ? part.split(':') : [];
                        if (parts.length === 2) {
                            var name = parts[1].trim().replace('()', '');
                            if (name && name.length > 4 && name.substr(name.length - 4, 4) === "Text" && typeof self[name] === "undefined") {
                                self[name] = '';
                            }
                        }
                    });
                });

                self.Visible = true;
            };

            try {
                ko.applyBindings(new ProfileViewModelModule<%=ModuleContext.ModuleId.ToString(CultureInfo.InvariantCulture) %>(), $container[0]);
            } catch (e) {

            }
        });
    </script>
</asp:Panel>

