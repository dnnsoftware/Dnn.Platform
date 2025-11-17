<%@ Control Language="C#" AutoEventWireup="true" Inherits="DotNetNuke.Admin.Containers.ModuleActions" Codebehind="ModuleActions.ascx.cs" %>
<asp:LinkButton runat="server" ID="actionButton" aria-label="Actions" />

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
/*globals jQuery, window*/
(function ($) {
    var moduleId = <%= ModuleContext.ModuleId %>;
    var displayQuickSettings = <%= DisplayQuickSettings.ToString().ToLower() %>;
    var supportsQuickSettings = <%= SupportsQuickSettings.ToString().ToLower() %>;

    // Initialize the DNN module actions plugin
    function setUpActions() {
        var tabId = <%= ModuleContext.TabId %>;
        $('#<%= actionButton.ClientID %>').dnnModuleActions({
            actionButton: "<%=actionButton.UniqueID %>",
            moduleId: moduleId,
            tabId: tabId,
            customActions: <%= CustomActionsJSON %>,
            adminActions: <%= AdminActionsJSON %>,
            panes: <%= Panes %>,
            customText: "<%= CustomText %>",
            adminText: "<%= AdminText %>",
            moveText: "<%= MoveText %>",
            topText: '<%= Localization.GetSafeJSString(LocalizeString("MoveTop.Action")) %>',
            upText: '<%= Localization.GetSafeJSString(LocalizeString("MoveUp.Action")) %>',
            downText: '<%= Localization.GetSafeJSString(LocalizeString("MoveDown.Action")) %>',
            bottomText: '<%= Localization.GetSafeJSString(LocalizeString("MoveBottom.Action")) %>',
            movePaneText: '<%= Localization.GetSafeJSString(LocalizeString("MoveToPane.Action")) %>',
            deleteText: '<%= Localization.GetSafeJSString("DeleteItem.Text", Localization.SharedResourceFile) %>',
            yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
            noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
            confirmTitle: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>',
            sharedText: '<%= Localization.GetSafeJSString("ModuleShared.Text", Localization.SharedResourceFile) %>',
            rootFolder: '<%= Page.ResolveClientUrl("~/") %>',
            supportsMove: <%= SupportsMove.ToString().ToLower() %>,
            supportsQuickSettings: supportsQuickSettings,
            displayQuickSettings: displayQuickSettings,
            isShared: <%= IsShared.ToString().ToLower() %>,
            moduleTitle: '<%= Localization.GetSafeJSString(ModuleTitle) %>'
        });
    }

    // Positions all module menus relative to their modules
    function positionMenus() {
        $('.actionMenu').each(function () {
            var $menu = $(this);
            var id = $menu.attr('id');
            if (!id) return;

            var mId = id.split('-')[1];
            if (moduleId != mId) return;

            var container = $(".DnnModule-" + mId + " .dnnDragHint");
            if (!container.length) return;

            var root = $('ul.dnn_mact', $menu);

            // Absolute positioning relative to body
            var moduleOffset = container.offset();
            var bodyOffset = $('body').offset() || { top: 0, left: 0 };

            root.css({
                position: "absolute",
                top: moduleOffset.top,
                left: moduleOffset.left + container.outerWidth() - root.outerWidth()
            });

            // Quick Settings overlay
            if (displayQuickSettings) {
                var ul = $('#moduleActions-' + mId + ' .dnn_mact > li.actionQuickSettings > ul');
                var $self = ul.parent();
                if ($self.length > 0) {
                    var windowHeight = $(window).height();
                    var windowScroll = $(window).scrollTop();
                    var thisTop = $self.offset().top;
                    var atViewPortTop = (thisTop - windowScroll) < windowHeight / 2;
                    var ulHeight = ul.height();

                    if (!atViewPortTop) {
                        ul.css({ top: -ulHeight, right: 0 })
                          .show('slide', { direction: 'down' }, 80, function () {
                              dnn.addIframeMask(ul[0]);
                              displayQuickSettings = false;
                          });
                    } else {
                        ul.css({ top: 20, right: 0 })
                          .show('slide', { direction: 'up' }, 80, function () {
                              dnn.addIframeMask(ul[0]);
                              displayQuickSettings = false;
                          });
                    }
                }
            }
        });
    }

    // Waits until the body has the expected left margin
    function waitForBodyMargin(expectedMargin, callback) {
        var $body = $('body');
        var stableCount = 0;
        var interval = setInterval(function () {
            var currentMargin = parseInt($body.css('margin-left')) || 0;
            if (currentMargin === expectedMargin) {
                stableCount++;
                if (stableCount >= 2) { // stable for two consecutive checks
                    clearInterval(interval);
                    callback();
                }
            } else {
                stableCount = 0;
            }
        }, 50); // check every 50ms
    }

    // Safe positioning wrapper
    function safePositionMenus() {
        waitForBodyMargin(80, positionMenus);
    }

    // Throttled resize/scroll handler
    var resizeThrottle;
    $(window).on('resize scroll', function () {
        if (resizeThrottle) clearTimeout(resizeThrottle);
        resizeThrottle = setTimeout(function () {
            safePositionMenus();
            resizeThrottle = null;
        }, 100);
    });

    // Initialize everything after window load
    $(window).on('load', function () {
        setUpActions();

        // Reposition menus after any AJAX completes
        $(document).ajaxComplete(function () {
            $(window).resize();
        });

        // Initial positioning after Persona Bar / body margin applied
        safePositionMenus();
    });

}(jQuery));
</script>