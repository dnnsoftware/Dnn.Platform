<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ModuleActions.ascx.cs" Inherits="DotNetNuke.Admin.Containers.ModuleActions" %>
<asp:LinkButton runat="server" ID="actionButton" />

<script language="javascript" type="text/javascript">
    /*globals jQuery, window */
    (function ($) {
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
                IsShared : <% = IsShared.ToString().ToLower() %>
            });

            $('.dnn_mact > li.actionMenuMove > ul').jScrollPane();
            
            $('.dnn_mact li').hoverIntent({
                over: function() {
                    // detect position
                    var windowHeight = $(window).height();
                    var windowScroll  = $(window).scrollTop();
                    var thisTop = $(this).offset().top;
                    var atViewPortTop = (thisTop - windowScroll) < windowHeight / 2;

                    var ul = $(this).find('ul');
                    var ulHeight = ul.height();
                    
                    if(!atViewPortTop) {
                        ul.css({top: -ulHeight, right: 0}).show('slide', { direction: 'down'},  80, function () {
                            if($(this).parent().hasClass('actionMenuMove')) {
                                $(this).jScrollPane();    
                            }
                            dnn.addIframeMask(ul[0]);
                        });
                    }
                    else {
                        ul.css({top: 20, right: 0}).show('slide', { direction: 'up'},  80, function () {
                            if($(this).parent().hasClass('actionMenuMove')) {
                                $(this).jScrollPane();    
                            }
                            dnn.addIframeMask(ul[0]);
                        });
                    }
                 
                },
                out: function() {
                    var ul = $(this).find('ul');
                    
                    if(ul && ul.position()) {
                        if (ul.position().top > 0) {
                            ul.hide('slide', { direction: 'up' }, 80, function() {
                                dnn.removeIframeMask(ul[0]);
                            });
                        } else {
                            ul.hide('slide', { direction: 'down' }, 80, function() {
                                dnn.removeIframeMask(ul[0]);
                            });
                        }
                    }
                },
                timeout: 400,
                interval: 200
            });
        }

        // Webkit based browsers (like Chrome and Safari) can access images width and height properties only after images have been fully loaded. 
        // It will cause menu action out of scope, TO fix this, use $(window).load instead of $(document).ready
        $(window).load(function () {
            setUpActions();
        });
        
    } (jQuery));
</script>
