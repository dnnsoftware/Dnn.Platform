<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Users.EditUser" Codebehind="EditUser.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Membership" Src="~/DesktopModules/Admin/Security/Membership.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Password" Src="~/DesktopModules/Admin/Security/Password.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Profile" Src="~/DesktopModules/Admin/Security/Profile.ascx" %>
<%@ Register TagPrefix="dnn" TagName="MemberServices" Src="~/DesktopModules/Admin/Security/MemberServices.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Register TagPrefix="dnn" TagName="UserSubscriptions" Src="~/DesktopModules/CoreMessaging/Subscriptions.ascx" %>

<%@ Import Namespace="DotNetNuke.UI.Utilities" %>

<%
  var confirmText = ClientAPI.GetSafeJSString(LocalizeString("UnregisterUser"));
  if (PortalSettings.DataConsentActive && User.UserID == UserInfo.UserID)
  {
    switch (PortalSettings.DataConsentUserDeleteAction)
      {
        case PortalSettings.UserDeleteAction.Manual:
          confirmText = ClientAPI.GetSafeJSString(Localization.GetString("ManualDelete.Confirm", "~/DesktopModules/Admin/Security/App_LocalResources/DataConsent.ascx.resx"));
          break;
        case PortalSettings.UserDeleteAction.DelayedHardDelete:
          confirmText = ClientAPI.GetSafeJSString(Localization.GetString("DelayedHardDelete.Confirm", "~/DesktopModules/Admin/Security/App_LocalResources/DataConsent.ascx.resx"));
          break;
        case PortalSettings.UserDeleteAction.HardDelete:
          confirmText = ClientAPI.GetSafeJSString(Localization.GetString("HardDelete.Confirm", "~/DesktopModules/Admin/Security/App_LocalResources/DataConsent.ascx.resx"));
          break;
      }
  }
%>

<script language="javascript" type="text/javascript">
    /*globals jQuery, window, Sys */
    (function ($, Sys) {
        function setUpDnnEditUser() {
            $('#dnnEditUser').dnnTabs().dnnPanels();
            //DNN-26777
            $('#<%= cmdDelete.ClientID %>').dnnConfirm({
                text: '<%= confirmText %>',
                            yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
                            noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
                            title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>',
                            isButton: false
                        });
            var serviceFramework = $.ServicesFramework(<%=ModuleId %>);

            var closeText = '<%= ClientAPI.GetSafeJSString(LocalizeString("Close")) %>';
            var errorTitle = '<%= ClientAPI.GetSafeJSString(LocalizeString("ErrorDialogTitle")) %>';
            var erroMessage = '<%= ClientAPI.GetSafeJSString(LocalizeString("ErrorDialogMessage")) %>';
            var successMessage = '<%= ClientAPI.GetSafeJSString(LocalizeString("SuccessMessage")) %>';

            function displayMessage(message, cssclass) {
                var messageNode = $("<div/>").addClass('dnnFormMessage ' + cssclass).text(message);
                $('#accountSettingsFieldSet').prepend(messageNode);
                messageNode.fadeOut(3000, 'easeInExpo', function () { messageNode.remove(); });
            };
            
            function toggleVanityUrl(show) {
                var $vanityUrlLabel =$('#<%= VanityUrl.ClientID %>');
                var $vanityUrlPanel = $('#<%= VanityUrlPanel.ClientID %>');
                
                if (show) {
                    $vanityUrlPanel.show();
                    $vanityUrlLabel.hide();
                } else {
                    $vanityUrlPanel.hide();
                    $vanityUrlLabel.show();
                }
            }

            toggleVanityUrl(<%=ShowVanityUrl.ToString().ToLowerInvariant() %>);

            //Clean and Validate url
            $('#updateProfileUrl').click(function(e) {
                //validate url
                var $vanityUrl = $('#<%= VanityUrlTextBox.ClientID %>');
                var $vanityUrlLabel =$('#<%= VanityUrl.ClientID %>');
                var httpAlias = $('#<%= VanityUrlAlias.ClientID %>').html();
                var vanityUrl = $vanityUrl.val();
                
                //Call Service Framework
                $.ajax({
                    type: 'POST',
                    url: serviceFramework.getServiceRoot('InternalServices') + 'ProfileService/UpdateVanityUrl',
                    data: { url: vanityUrl },
                    beforeSend: serviceFramework.setModuleHeaders
                }).done(function (result) {
                    if (result.Result == "warning") {
                        $.dnnAlert({
                            okText: closeText,
                            title: result.Title,
                            text: result.Message
                        });
                        $vanityUrl.val(result.SuggestedUrl);
                    } else {
                        displayMessage(successMessage, "dnnFormSuccess");
                        $vanityUrlLabel.html(httpAlias + vanityUrl);
                        toggleVanityUrl(false);
                    }
                }).fail(function (xhr, status, error) {
                    $.dnnAlert({
                        okText: closeText,
                        title: errorTitle,
                        text: error
                    });
                });
            });
            
            var dnn = dnn || {};
            dnn.subscriptionsSettings = <%=UserSubscriptions.GetSettingsAsJson()%>;
            dnn.subscriptionsController = new Subscription(ko, dnn.subscriptionsSettings,'dnnUserSubscriptions');
        }

        $(document).ready(function () {
            setUpDnnEditUser();
            var pageNo = <%=PageNo %>;
            if(pageNo > 0) {
                $('#dnnEditUser > ul > li:nth-child(' + pageNo + ')').find('a').click();
            }

            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setUpDnnEditUser();
            });
        });
    } (jQuery, window.Sys));
</script>

<div class="dnnForm dnnEditUser dnnClear" id="dnnEditUser" runat="server" ClientIDMode="Static">
    <ul class="dnnAdminTabNav dnnClear" id="adminTabNav" runat="server">
        <li><a href="#dnnUserDetails"><%=LocalizeString("cmdUser")%></a></li>
        <li><a href="#<%=dnnProfileDetails.ClientID%>"><%=LocalizeString("cmdProfile")%></a></li>
        <li><a href="#dnnUserSubscriptions"><%=LocalizeString("cmdCommunications")%></a></li>
        <li id="servicesTab" runat="server"><a href="#<%=dnnServicesDetails.ClientID%>"><%=LocalizeString("cmdServices")%></a></li>
    </ul>
    <div id="dnnUserDetails" class="dnnUserDetails dnnClear">
        <div class="udContent dnnClear">
			<h2 id="dnnPanel-AccountSettings" class="dnnFormSectionHead"><a href="" class="dnnLabelExpanded"><%=LocalizeString("AccountSettings")%></a></h2>
            <fieldset id="accountSettingsFieldSet">
                <dnn:DnnFormEditor id="userForm" runat="Server" FormMode="Short">
                    <Items>
                        <dnn:DnnFormLiteralItem ID="userNameReadOnly" runat="server" DataField="Username" />
                        <dnn:DnnFormTextBoxItem ID="displayName" runat="server" DataField="DisplayName" Required="true" />
                        <dnn:DnnFormTextBoxItem ID="email" runat="server" DataField="Email" Required="true" />
                   </Items>
                </dnn:DnnFormEditor>
                <asp:Panel class="dnnFormItem" ID="VanityUrlRow" runat="server" Visible="False" ViewStateMode="Disabled">
                    <dnn:Label ID="VanityUrlLabel" runat="server" />
                    <asp:Label runat="server" ID="VanityUrl" />
                    <div class="dnnFormGroup" id="VanityUrlPanel" runat="Server">
                        <asp:Label runat="server" ID="VanityUrlHeader" CssClass="NormalBold" resourcekey="VanityUrlHeader"/>
                        <div>
                            <asp:Label runat="server" ID="VanityUrlAlias" />
                            <asp:TextBox CssClass="dnnUserVanityUrl" runat="server" ID="VanityUrlTextBox" aria-label="Vanity Url" />
                            <a id="updateProfileUrl" href="#" class="dnnSecondaryAction"><%=LocalizeString("cmdUpdate")%></a>
                        </div>
                    </div>
                </asp:Panel>
            </fieldset>
			<h2 id="H1" class="dnnFormSectionHead"><a href=""><%=LocalizeString("PasswordSettings")%></a></h2>
            <fieldset>
                <div class="dnnPasswordDetails dnnClear">
    	            <dnn:Password id="ctlPassword" runat="server"></dnn:Password>
                </div>
            </fieldset>
			<h2 id="H2" class="dnnFormSectionHead"><a href=""><%=LocalizeString("AccountInfo")%></a></h2>
            <fieldset>
                <div class="dnnMembership">
                    <dnn:membership id="ctlMembership" runat="Server" />
                </div>
            </fieldset>
        </div>
        <ul id="actionsRow" runat="server" class="dnnActions dnnClear">
            <li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
            <li><asp:LinkButton id="cmdDelete" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdDelete" /></li>
        </ul>
    </div>
    <asp:Panel id="dnnProfileDetails" runat="server" class="dnnProfileDetails dnnClear">
    	<dnn:Profile id="ctlProfile" runat="server"></dnn:Profile>
    </asp:Panel>
    <div id="dnnUserSubscriptions" class="dnnUserSubscriptions">
        <dnn:UserSubscriptions id="UserSubscriptions" runat="server" LocalResourceFile="~/DesktopModules/CoreMessaging/App_LocalResources/View.ascx.resx"></dnn:UserSubscriptions>
    </div>
    <div id="dnnServicesDetails" runat="server" visible="false" class="dnnServicesDetails dnnClear">
    	<dnn:MemberServices id="ctlServices" runat="server"></dnn:MemberServices>
    </div>
</div>