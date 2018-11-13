(function ($) {
    $.fn.dnnModuleActions = function (options) {
        var opts = $.extend({}, $.fn.dnnModuleActions.defaultOptions, options);
        var $self = this;
        var actionButton = opts.actionButton;
        var moduleId = opts.moduleId;
        var tabId = opts.tabId;
        var adminActions = opts.adminActions;
        var adminCount = adminActions.length;
        var customActions = opts.customActions;
        var customCount = customActions.length;
        var panes = opts.panes;
        var supportsMove = opts.supportsMove;
        var count = adminCount + customCount;
        var isShared = opts.isShared;
        var supportsQuickSettings = opts.supportsQuickSettings;
        var displayQuickSettings = opts.displayQuickSettings;
        var sharedText = opts.sharedText;
        var moduleTitle = opts.moduleTitle;

        function completeMove(targetPane, moduleOrder) {
            //remove empty pane class
            $("#dnn_" + targetPane).removeClass("DNNEmptyPane");

            var dataVar = {
                TabId: tabId,
                ModuleId: moduleId,
                Pane: targetPane,
                ModuleOrder: moduleOrder
            };

            var service = $.dnnSF();
            var serviceUrl = $.dnnSF().getServiceRoot("InternalServices") + "ModuleService/";
            $.ajax({
                url: serviceUrl + 'MoveModule',
                type: 'POST',
                data: dataVar,
                beforeSend: service.setModuleHeaders,
                success: function () {
                    window.location.reload();
                },
                error: function () {
                }
            });

            //fire window resize to reposition action menus
            $(window).resize();
        }

        function getModuleId(module) {
            var $anchor = $(module).children("a");
            if ($anchor.length === 0) {
                $anchor = $(module).children("div.dnnDraggableContent").children("a");
            }
            return $anchor.attr("name");
        }

        function isEnabled(action) {
            return action.ClientScript || action.Url || action.CommandArgument;
        }

        function moveDown(targetPane, moduleIndex) {
            var container = $(".DnnModule-" + moduleId);

            //move module to target pane
            container.fadeOut("slow", function () {
                $(this).detach()
                    .insertAfter($("#dnn_" + targetPane).children()[moduleIndex])
                    .fadeIn("slow", function () {

                        //update server
                        completeMove(targetPane, (2 * moduleIndex + 4));
                    });
            });
        }

        function moveTop(targetPane) {
            var container = $(".DnnModule-" + moduleId);

            //move module to target pane
            container.fadeOut("slow", function () {
                $(this).detach()
                    .prependTo($("#dnn_" + targetPane))
                    .fadeIn("slow", function () {
                        //update server
                        completeMove(targetPane, 0);
                    });
            });
        }

        function moveToPane(targetPane) {
            var container = $(".DnnModule-" + moduleId);

            //move module to target pane
            container.fadeOut("slow", function () {
                $(this).detach()
                    .appendTo("#dnn_" + targetPane)
                    .fadeIn("slow", function () {

                        //update server
                        completeMove(targetPane, -1);
                    });
            });
        }

        function moveBottom(targetPane) {
            moveToPane(targetPane);
        }

        function moveUp(targetPane, moduleIndex) {
            var container = $(".DnnModule-" + moduleId);

            //move module to target pane
            container.fadeOut("slow", function () {
                $(this).detach()
                    .insertBefore($("#dnn_" + targetPane).children()[moduleIndex - 1])
                    .fadeIn("slow", function () {
                        //update server
                        completeMove(targetPane, (2 * moduleIndex - 2));
                    });
            });
        }

        function closeMenu(ul) {
            var $menuroot = $('#moduleActions-' + moduleId + ' ul.dnn_mact');
            $menuroot.removeClass('showhover').data('displayQuickSettings', false);
            if (ul && ul.position()) {
                if (ul.position().top > 0) {
                    ul.hide('slide', { direction: 'up' }, 80, function () {
                        dnn.removeIframeMask(ul[0]);
                    });
                } else {
                    ul.hide('slide', { direction: 'down' }, 80, function () {
                        dnn.removeIframeMask(ul[0]);
                    });
                }
            }
        }

        function showMenu(ul) {
            // detect position
            var $self = ul.parent();
            var windowHeight = $(window).height();
            var windowScroll = $(window).scrollTop();
            var thisTop = $self.offset().top;
            var atViewPortTop = (thisTop - windowScroll) < windowHeight / 2;

            var ulHeight = ul.height();

            if (!atViewPortTop) {
                ul.css({
                    top: -ulHeight,
                    right: 0
                }).show('slide', { direction: 'down' }, 80, function () {
                    if ($(this).parent().hasClass('actionMenuMove')) {
                        $(this).jScrollPane();
                    }
                    dnn.addIframeMask(ul[0]);
                });
            }
            else {
                ul.css({
                    top: 20,
                    right: 0
                }).show('slide', { direction: 'up' }, 80, function () {
                    if ($(this).parent().hasClass('actionMenuMove')) {
                        $(this).jScrollPane();
                    }
                    dnn.addIframeMask(ul[0]);
                });
            }
        }

        function buildMenuRoot(root, rootText, rootClass, rootIcon) {
            root.append("<li class=\"" + rootClass + "\"><a href='javascript:void(0)' aria-label=\"" + rootText + "\"><i class='dnni dnni-" + rootIcon + "' /></a><ul></ul>");
            var parent = root.find("li." + rootClass + " > ul");

            return parent;
        }

        function buildMenu(root, rootText, rootClass, rootIcon, actions, actionCount) {
            var $parent = buildMenuRoot(root, rootText, rootClass, rootIcon);

            for (var i = 0; i < actionCount; i++) {
                var action = actions[i];

                if (action.Title !== "~") {
                    if (!action.Url) {
                        action.Url = "javascript: __doPostBack('" + actionButton + "', '" + action.ID + "')";
                    } else {
                        action.Url = decodeURIComponent(action.Url);
                    }

                    var htmlString = "<li>";

                    switch (action.CommandName) {
                        case "DeleteModule.Action":
                            htmlString = "<li id=\"moduleActions-" + moduleId + "-Delete\">";
                            break;
                        case "ModuleSettings.Action":
                            htmlString = "<li id=\"moduleActions-" + moduleId + "-Settings\">";
                            break;
                        case "ImportModule.Action":
                            htmlString = "<li id=\"moduleActions-" + moduleId + "-Import\">";
                            break;
                        case "ExportModule.Action":
                            htmlString = "<li id=\"moduleActions-" + moduleId + "-Export\">";
                            break;
                        case "ModuleHelp.Action":
                            htmlString = "<li id=\"moduleActions-" + moduleId + "-Help\">";
                            break;
                    }

                    if (isEnabled(action)) {
                        htmlString += "<a href=\"" + action.Url + "\"" + (action.NewWindow ? " target=\"_blank\"" : "") + "><img src=\"" + action.Icon + "\" alt=\"" + action.Title + "\"><span>" + action.Title + "</span></a>";
                    } else {
                        htmlString += "<img src=\"" + action.Icon + "\" alt=\"" + action.Title + "\"><span>" + action.Title + "</span>";
                    }

                    $parent.append(htmlString);
                }
            }

            $parent.find("#moduleActions-" + moduleId + "-Delete a").dnnConfirm({
                text: opts.deleteText,
                yesText: opts.yesText,
                noText: opts.noText,
                title: opts.confirmTitle
            });
        }

        function buildMoveMenu(root, rootText, rootClass, rootIcon) {
            var parent = buildMenuRoot(root, rootText, rootClass, rootIcon);
            var modulePane = $(".DnnModule-" + moduleId).parent();
            var paneName = modulePane.attr("id").replace("dnn_", "");

            var htmlString;
            var moduleIndex = -1;
            var id = paneName + moduleId;
            var modules = modulePane.children();
            var moduleCount = modules.length;
            var i;

            for (i = 0; i < moduleCount; i++) {
                var module = modules[i];
                var mid = getModuleId(module);

                if (moduleId === parseInt(mid)) {
                    moduleIndex = i;
                    break;
                }
            }

            //Add Top/Up actions
            if (moduleIndex > 0) {
                htmlString = "<li id=\"" + id + "-top\" class=\"common\">" + opts.topText;
                parent.append(htmlString);

                //Add click event handler to just added element
                parent.find("li#" + id + "-top").click(function () {
                    moveTop(paneName);
                });

                htmlString = "<li id=\"" + id + "-up\" class=\"common\">" + opts.upText;
                parent.append(htmlString);

                //Add click event handler to just added element
                parent.find("li#" + id + "-up").click(function () {
                    moveUp(paneName, moduleIndex);
                });
            }

            //Add Bottom/Down actions
            if (moduleIndex < moduleCount - 1) {
                htmlString = "<li id=\"" + id + "-down\" class=\"common\">" + opts.downText;
                parent.append(htmlString);

                //Add click event handler to just added element
                parent.find("li#" + id + "-down").click(function () {
                    moveDown(paneName, moduleIndex);
                });

                htmlString = "<li id=\"" + id + "-bottom\" class=\"common\">" + opts.bottomText;
                parent.append(htmlString);

                //Add click event handler to just added element
                parent.find("li#" + id + "-bottom").click(function () {
                    moveBottom(paneName);
                });
            }

            var htmlStringContainer = "";

            //Add move to pane entries
            for (i = 0; i < panes.length; i++) {
                var pane = panes[i];
                if (paneName !== pane) {
                    id = pane + moduleId;
                    htmlStringContainer += "<li id=\"" + id + "\">" + opts.movePaneText.replace("{0}", pane);
                }
            }

            if (htmlStringContainer) {
                // loop is done, append the HTML and add moveToPane function on click event
                parent.append(htmlStringContainer);
                parent.find("li").not('.common').click(function () {
                    moveToPane($(this).attr("id").replace(moduleId, ""));
                });
            }
        }

        function buildMenuLabel(root, rootText, rootClass) {
            if (!rootText || rootText.length == 0) {
                return;                
            }
            root.append("<li class=\"" + rootClass + "\"><div>" + rootText + "</div>");
        }

        function buildQuickSettings(root, rootText, rootClass, rootIcon) {
            var $parent = buildMenuRoot(root, rootText, rootClass, rootIcon);

            var $quickSettings = $("#moduleActions-" + moduleId + "-QuickSettings");
            $quickSettings.show();
            root.addClass('showhover');

            $parent.append($quickSettings);
        }

        function position(mId) {
            var container = $(".DnnModule-" + mId);
            var root = $("#moduleActions-" + mId + " > ul");
            var containerPosition = container.offset();
            var containerWidth = container.width();

            root.css({
                position: "absolute",
                marginLeft: 0,
                marginTop: 0,
                top: containerPosition.top,
                left: containerPosition.left + containerWidth - 65
            });
        }

        function watchResize(mId) {
            var container = $(".DnnModule-" + mId);
            container.data("o-size", { w: container.width(), h: container.height() });
            var resizeThrottle;

            var loopyFunc = function () {
                var data = container.data("o-size");
                if (data.w !== container.width() || data.h !== container.height()) {
                    container.data("o-size", { w: container.width(), h: container.height() });
                    container.trigger("resize");
                }

                if (resizeThrottle) {
                    clearTimeout(resizeThrottle);
                    resizeThrottle = null;
                }

                resizeThrottle = setTimeout(loopyFunc, 250);
            };

            container.trigger("resize", function () {
                position(mId);
            });

            loopyFunc();
        };

        if (count > 0 || supportsMove) {
            var $form = $("form#Form");
            if ($form.find("div#moduleActions-" + moduleId).length === 0) {
                $form.append("<div id=\"moduleActions-" + moduleId + "\" class=\"actionMenu\"><ul class=\"dnn_mact\"></ul></div>");
                var menu = $form.find("div:last");
                var menuRoot = menu.find("ul");
                var menuLabel = moduleTitle;
                if (customCount > 0) {
                    buildMenu(menuRoot, "Edit", "actionMenuEdit", "pencil",  customActions, customCount);
                }
                if (adminCount > 0) {
                    buildMenu(menuRoot, "Admin", "actionMenuAdmin", "cog", adminActions, adminCount);
                }
                if (supportsMove) {
                    buildMoveMenu(menuRoot, "Move", "actionMenuMove", "arrows");
                }
                if (supportsQuickSettings) {
                    buildQuickSettings(menuRoot, "Quick", "actionQuickSettings", "caret-down");
                    menuRoot.data('displayQuickSettings', displayQuickSettings);
                }

                if (isShared) {
                    menuLabel = menuLabel && menuLabel.length > 0 ? sharedText + ': ' + menuLabel : sharedText;
                }
                buildMenuLabel(menuRoot, menuLabel, "dnn_menu_label");
                watchResize(moduleId);
            }
        }

        $("#moduleActions-" + moduleId + " .dnn_mact > li.actionMenuMove > ul").jScrollPane();

        $("#moduleActions-" + moduleId + " .dnn_mact > li").hoverIntent({
            over: function () {
                showMenu($(this).find("ul").first());
            },
            out: function () {
                if (!($(this).hasClass("actionQuickSettings") && $(this).data('displayQuickSettings'))) {
                    closeMenu($(this).find("ul").first());
                }
            },
            timeout: 400,
            interval: 200
        });

        var $container = $('#moduleActions-' + moduleId + '-QuickSettings');
        $container.find('select').mouseout(function(e) {
            e.stopPropagation();
        });

        return $self;
    };

    $.fn.dnnModuleActions.defaultOptions = {
        customText: "CustomText",
        adminText: "AdminText",
        moveText: "MoveText",
        topText: "Top",
        upText: "Up",
        downText: "Down",
        bottomText: "Bottom",
        movePaneText: "To {0}",
        supportsQuickSettings: false
    };
})(jQuery);