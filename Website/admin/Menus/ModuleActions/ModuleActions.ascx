<%@ Control Language="C#" AutoEventWireup="true" Inherits="DotNetNuke.Admin.Containers.ModuleActions" Codebehind="ModuleActions.ascx.cs" %>
<asp:LinkButton runat="server" ID="actionButton" />

<%
    if (SupportsQuickSettings)
    {
        %>
        <li id="moduleActions-<% = ModuleContext.Configuration.ModuleID %>-QuickSettings" style="display:none">
            <div>
                <div class="qsHeader"><%=Localization.GetSafeJSString("QuickSettings", Localization.SharedResourceFile) %></div>
                <div class="qsContainer">
                    <asp:Panel id="quickSettings" runat="server">
    
                    </asp:Panel>
                </div>
                <div class="qsFooter">
                    <a class="secondarybtn"><%=Localization.GetSafeJSString("Cancel", Localization.SharedResourceFile) %></a>
                    <a class="primarybtn"><%=Localization.GetSafeJSString("Save", Localization.SharedResourceFile) %></a>
                </div>
            </div>
        </li>
        <%
    }
%>

<script type="text/javascript">
    /*globals jQuery, window */
    (function ($) {
        var displayQuickSettings = <% = DisplayQuickSettings.ToString().ToLower() %>;

        function setUpActions() {
            var moduleId = <% = ModuleContext.Configuration.ModuleID %>;
            var tabId = <% = ModuleContext.Configuration.TabID %>;

            //Initialise the actions menu plugin
            $('#<%= actionButton.ClientID %>').dnnModuleActions({
                    actionButton: "<% =actionButton.UniqueID %>",
                    moduleId: moduleId,
                    tabId: tabId,
                    customActions: <% = CustomActionsJSON %>,
                    adminActions: <% = AdminActionsJSON %>,
                    panes: <% = Panes %>,
                    customText: "<% = CustomText %>",
                    adminText: "<% = AdminText %>",
                    moveText: "<% = MoveText %>",
                    topText: '<% = Localization.GetSafeJSString(LocalizeString("MoveTop.Action"))%>',
                    upText: '<% = Localization.GetSafeJSString(LocalizeString("MoveUp.Action"))%>',
                    downText: '<% = Localization.GetSafeJSString(LocalizeString("MoveDown.Action"))%>',
                    bottomText: '<% = Localization.GetSafeJSString(LocalizeString("MoveBottom.Action"))%>',
                    movePaneText: '<% = Localization.GetSafeJSString(LocalizeString("MoveToPane.Action"))%>',
                    deleteText: '<%= Localization.GetSafeJSString("DeleteItem.Text", Localization.SharedResourceFile) %>',
                    yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
                    noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
                    confirmTitle: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>',
                    rootFolder: '<%= Page.ResolveClientUrl("~/") %>',
                    supportsMove: <% = SupportsMove.ToString().ToLower() %>,
                    supportsQuickSettings: <% = SupportsQuickSettings.ToString().ToLower() %>,
                    displayQuickSettings: displayQuickSettings,
                    isShared : <% = IsShared.ToString().ToLower() %>
                }
            );
        }

        // register window resize on ajaxComplete to reposition action menus - only in edit mode
        // after page fully load
        var resizeThrottle;
        var rootMenuWidth = (<% = SupportsQuickSettings.ToString().ToLower() %>) ? 85 : 65;
        $(window).resize(function () {
            if (resizeThrottle) {
                clearTimeout(resizeThrottle);
                resizeThrottle = null;
            }
            resizeThrottle = setTimeout(
                function () {
                    var menu = $('.actionMenu');
                    menu.each(function () {
                        var $this = $(this);
                        var id = $this.attr('id');
                        if (id) {
                            var mId = id.split('-')[1];
                            var container = $(".DnnModule-" + mId);
                            var root = $('ul.dnn_mact', $this);
                            var containerPosition = container.offset();
                            var containerWidth = container.width();

                            root.css({
                                position: "absolute",
                                marginLeft: 0,
                                marginTop: 0,
                                top: containerPosition.top,
                                left: containerPosition.left + containerWidth - rootMenuWidth
                            });

                            if (displayQuickSettings) {
                                var ul = $('#moduleActions-' + mId + ' .dnn_mact > li.actionQuickSettings > ul');
                                var $self = ul.parent();
                                var windowHeight = $(window).height();
                                var windowScroll = $(window).scrollTop();
                                var thisTop = $self.offset().top;
                                var atViewPortTop = (thisTop - windowScroll) < windowHeight / 2;

                                var ulHeight = ul.height();

                                if ($self.hasClass('actionQuickSettings')) {
                                    ul.css({ width: containerWidth });
                                }

                                if (!atViewPortTop) {
                                    ul.css({
                                        top: -ulHeight,
                                        right: 0
                                    }).show('slide', { direction: 'down' }, 80, function () {
                                        dnn.addIframeMask(ul[0]);
                                    });
                                }
                                else {
                                    ul.css({
                                        top: 20,
                                        right: 0
                                    }).show('slide', { direction: 'up' }, 80, function () {
                                        dnn.addIframeMask(ul[0]);
                                    });
                                }
                            }
                        }
                    });
                    resizeThrottle = null;
                },
                100
            );
        });

        // Webkit based browsers (like Chrome and Safari) can access images width and height properties only after images have been fully loaded. 
        // It will cause menu action out of scope, TO fix this, use $(window).load instead of $(document).ready
        $(window).load(function () {
            setUpActions();

            $(document).ajaxComplete(function () {
                $(window).resize();
            });
            $(window).resize();
        });        

    } (jQuery));
</script>
