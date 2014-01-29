
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
        
        if (count > 0 || supportsMove) {
            var $form = $("body form");
            if ($form.find("div#moduleActions-" + moduleId).length === 0) {
                $form.append("<div id=\"moduleActions-" + moduleId + "\" class=\"actionMenu\"><ul class=\"dnn_mact\"></ul></div>");
                var menu = $form.find("div:last");
                var menuRoot = menu.find("ul");
                if (customCount > 0) {
                    buildMenu(menuRoot, "Edit", "actionMenuEdit", customActions, customCount);
                }
                if (adminCount > 0) {
                    buildMenu(menuRoot, "Admin", "actionMenuAdmin", adminActions, adminCount);
                }
                if (supportsMove) {
                    buildMoveMenu(menuRoot, "Move", "actionMenuMove");
                }
                
	            watchResize(moduleId);
            }
        }

        function buildMoveMenu(root, rootText, rootClass) {
            var parent = buildMenuRoot(root, rootText, rootClass);
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

                if (moduleId == parseInt(mid)) {
                    moduleIndex = i;
                    break;
                }
            }

            //Add Top/Up actions
            if (moduleIndex > 0) {
                //htmlString = "<li id=\"" + id + "-top\"><a href=\"#\"><img src=\"" + rootFolder + "images/action_top.gif\"><span>" + opts.topText + "</span></a>";
                htmlString = "<li id=\"" + id + "-top\">" + opts.topText;
                parent.append(htmlString);

                //Add click event handler to just added element
                parent.find("li#" + id + "-top").click(function () {
                    moveTop(paneName);
                });

                //htmlString = "<li id=\"" + id + "-up\"><a href=\"#\"><img src=\"" + rootFolder + "images/action_up.gif\"><span>" + opts.upText + "</span></a>";
                htmlString = "<li id=\"" + id + "-up\">" + opts.upText;
                parent.append(htmlString);

                //Add click event handler to just added element
                parent.find("li#" + id + "-up").click(function () {
                    moveUp(paneName, moduleIndex);
                });
            }

            //Add Bottom/Down actions
            if (moduleIndex < moduleCount - 1) {
                //htmlString = "<li id=\"" + id + "-down\"><a href=\"#\"><img src=\"" + rootFolder + "images/action_down.gif\"><span>" + opts.downText + "</span></a>";
                htmlString = "<li id=\"" + id + "-down\">" + opts.downText;
                parent.append(htmlString);

                //Add click event handler to just added element
                parent.find("li#" + id + "-down").click(function () {
                    moveDown(paneName, moduleIndex);
                });

                //htmlString = "<li id=\"" + id + "-bottom\"><a href=\"#\"><img src=\"" + rootFolder + "images/action_bottom.gif\"><span>" + opts.bottomText + "</span></a>";
                htmlString = "<li id=\"" + id + "-bottom\">" + opts.bottomText;
                parent.append(htmlString);

                //Add click event handler to just added element
                parent.find("li#" + id + "-bottom").click(function () {
                    moveBottom(paneName);
                });
            }

            //Add move to pane entries
            for (i = 0; i < panes.length; i++) {
                var pane = panes[i];
                if (paneName !== pane) {
                    id = pane + moduleId;
                    //htmlString = "<li id=\"" + id + "\"><a href=\"#\"><img src=\"" + rootFolder + "images/action_move.gif\"><span>" + opts.movePaneText.replace("{0}", pane) + "</span></a>";
                    htmlString = "<li id=\"" + id + "\">" + opts.movePaneText.replace("{0}", pane);
                    parent.append(htmlString);

                    //Add click event handler to just added element
                    parent.find("li#" + id).click(function () {
                        moveToPane($(this).attr("id").replace(moduleId, ""));
                    });
                }
            }
        }

        function buildMenu(root, rootText, rootClass, actions, actionCount) {
            var $parent = buildMenuRoot(root, rootText, rootClass);

            for (var i = 0; i < actionCount; i++) {
                var action = actions[i];

                if (action.Title !== "~") {
                    if (!action.Url) {
	                    action.Url = "javascript: __doPostBack('" + actionButton + "', '" + action.ID + "')";
                    } else {
	                    action.Url = decodeURIComponent(action.Url);
                    }

                    var htmlString = "<li>";
                    if (action.CommandName === "DeleteModule.Action") {
                        htmlString = "<li id=\"moduleActions-" + moduleId + "-Delete\">";
                    }
                    if (isEnabled(action)) {
                        htmlString += "<a href=\"" + action.Url + "\"><img src=\"" + action.Icon + "\"><span>" + action.Title + "</span></a>";
                    } else {
                        htmlString += "<img src=\"" + action.Icon + "\"><span>" + action.Title + "</span>";
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

        function buildMenuRoot(root, rootText, rootClass) {
            root.append("<li class=\"" + rootClass + "\"><a href='javascript:void(0)' /><ul></ul>");
            var parent = root.find("li." + rootClass + " > ul");

            return parent;
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

        function moveBottom(targetPane) {
            moveToPane(targetPane);
        }

        function moveDown(targetPane, moduleIndex) {
            var container = $(".DnnModule-" + moduleId);

            //move module to target pane
            container.fadeOut("slow", function () {
                $(this).detach()
                    .insertAfter($("#dnn_" + targetPane).children()[moduleIndex])
                    .fadeIn("slow", function () {

                        //update server
                        completeMove(targetPane, ( 2 * moduleIndex + 2));
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

        function moveUp(targetPane, moduleIndex) {
            var container = $(".DnnModule-" + moduleId);

            //move module to target pane
            container.fadeOut("slow", function () {
                $(this).detach()
                    .insertBefore($("#dnn_" + targetPane).children()[moduleIndex - 1])
                    .fadeIn("slow", function () {
                        //update server
                        completeMove(targetPane, (2 * moduleIndex - 4));
                    });
            });
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
            var loopyFunc = function () {
                var data = container.data("o-size");
                if (data.w != container.width() || data.h != container.height()) {
                    container.data("o-size", { w: container.width(), h: container.height() });
                    container.trigger("resize");
                }

                setTimeout(loopyFunc, 250);
            };

            container.trigger("resize", function () {
                position(mId);
            });

            loopyFunc();
        };

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
        movePaneText: "To {0}"
    };
    
    // register window resize on ajaxComplete to reposition action menus - only in edit mode
    // after page fully load
    var resizeThrottle;
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
                            left: containerPosition.left + containerWidth - 65
                        });
                    }
                });
                resizeThrottle = null;
            },
            100
        );
    });
    $(window).load(function () {
        $(document).ajaxComplete(function () {
            $(window).resize();
        });
        $(window).resize();
    });

})(jQuery);