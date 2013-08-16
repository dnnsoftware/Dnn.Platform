(function ($) {
    $.fn.dnnModuleDragDrop = function (options) {

        var isEditMode = $('body').hasClass('dnnEditState');
        if (!isEditMode) {
            $(function () {
                $('.DNNEmptyPane').each(function () {
                    $(this).removeClass('DNNEmptyPane').addClass('dnnDropEmptyPanes');
                });
                $('.contentPane').each(function () {
                    // this special code is for you -- IE8
                    this.className = this.className;
                });
            });

            return this;
        }

        //Default settings
        var settings = {
            actionMenu: "div.actionMenu",
            cursor: "move",
            draggingHintText: "DraggingHintText",
            dragHintText: "DragHintText",
            dropOnEmpty: true,
            dropHintText: "DropHintText",
            dropTargetText: "DropModuleText"
        };

        settings = $.extend(settings, options || {});
        var $self = this;
        var paneModuleIndex;
        var modulePaneName;
        var tabId = settings.tabId;
        var mid;

        var $modules = $('.DnnModule');
        var $module;

        var getModuleId = function ($mod) {
            return $mod.find("a").first().attr("name");
        };

        var getModuleIndex = function (moduleId, $pane) {
            var index = -1;
            var modules = $pane.children(".DnnModule");
            for (var i = 0; i < modules.length; i++) {
                var module = modules[i];
                mid = getModuleId($(module));

                if (moduleId == parseInt(mid)) {
                    index = i;
                    break;
                }
            }
            return index;
        };

        var updateServer = function (moduleId, $pane) {
            var order;
            var paneName = $pane.attr("id").substring(4);
            var index = getModuleIndex(moduleId, $pane);

            if (paneName !== modulePaneName) {
                //Moved to new Pane
                order = index * 2;
            } else {
                //Module moved within Pane
                if (index > paneModuleIndex) {
                    //Module moved down
                    order = (index + 1) * 2;
                } else {
                    //Module moved up
                    order = index * 2;
                }
            }

            var dataVar = {
                TabId: tabId,
                ModuleId: moduleId,
                Pane: paneName,
                ModuleOrder: order
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
        };

        for (var moduleNo = 0; moduleNo < $modules.length; moduleNo++) {
            $module = $($modules[moduleNo]);
            mid = getModuleId($module);

            //Add a drag handle
            if ($module.find(".dnnDragHint").length === 0) {
                $module.prepend("<div class=\"dnnDragHint\"></div>");
            }

            //Add a drag hint
            $module.find(".dnnDragHint").dnnHelperTip({
                helpContent: settings.dragHintText,
                holderId: "ModuleDragToolTip-" + mid
            });
        }

        //call jQuery UI Sortable plugin
        var originalPane = null;
        var allDnnSortableEmpty = true;
        $('.dnnSortable').each(function (n, v) {
            if (!$.trim($(v).html()).length) {
                return true;
            }
            else {
                allDnnSortableEmpty = false;
                return false;
            }
        });
        if (allDnnSortableEmpty) {

            $(function () {
                $('.DNNEmptyPane').each(function () {
                    $(this).removeClass('DNNEmptyPane').addClass('dnnDropEmptyPanes');
                });
                $('.contentPane').each(function () {
                    // this special code is for you -- IE8
                    this.className = this.className;
                });
            });


            $self.droppable({
                tolerance: "pointer",
                /*hoverClass: 'dnnDropTarget',*/
                over: function (event, ui) {
                    $(this).append("<div class='dnnDropTarget'><p>" + settings.dropTargetText + "(" + $(this).attr("id").substring(4) + ")" + "</p></div>");
                },
                out: function (event, ui) {
                    $(this).empty();
                },
                drop: function (event, ui) {
                    // add module
                    var dropItem = ui.draggable;
                    var pane = $(this).empty();
                    var order = 0;
                    var paneName = pane.attr("id").substring(4);
                    dropItem.remove();
                    if (dnn.controlBar) {
                        dnn.controlBar.addModule(dnn.controlBar.dragdropModule + '',
                            dnn.controlBar.dragdropPage,
                            paneName,
                            '-1',
                            order + '',
                            dnn.controlBar.dragdropVisibility + '',
                            dnn.controlBar.dragdropAddExistingModule + '',
                            dnn.controlBar.dragdropCopyModule + '');
                    }
                }
            });
            return $self;
        }

        $('div.dnnDragHint').mousedown(function () {
            $('.DNNEmptyPane').each(function () {
                $(this).removeClass('DNNEmptyPane').addClass('dnnDropEmptyPanes');
            });
            $('.contentPane').each(function () {
                // this special code is for you -- IE8
                this.className = this.className;
            });

        }).mouseup(function () {
            $('.dnnDropEmptyPanes').each(function () {
                $(this).removeClass('dnnDropEmptyPanes').addClass('DNNEmptyPane');
            });
            $('.contentPane').each(function () {
                // this special code is for you -- IE8
                this.className = this.className;
            });
        });

        $self.sortable({
            connectWith: ".dnnSortable",
            dropOnEmpty: settings.dropOnEmpty,
            cursor: settings.cursor,
            cursorAt: { left: 10, top: 30 },
            handle: "div.dnnDragHint",
            placeholder: "dnnDropTarget",
            tolerance: "pointer",
            helper: function (event, ui) {

                var dragTip = $('<div class="dnnDragdropTip ControlBar_DragdropModule"></div>');
                var title = $('span.Head', ui).html();
                if (!title)
                    title = "The Dragging Module";

                dragTip.html(title);
                $('body').append(dragTip);
                return dragTip;
            },

            start: function (event, ui) {
                var $pane = ui.item.parent();
                originalPane = $pane;

                modulePaneName = $pane.attr("id").substring(4);
                mid = getModuleId(ui.item);
                paneModuleIndex = getModuleIndex(mid, $pane);

                //Add drop target text
                var $dropTarget = $(".dnnDropTarget");
                $dropTarget.append("<p>" + settings.dropTargetText + "(" + modulePaneName + ")" + "</p>");

                $(settings.actionMenu).hide();
            },

            over: function (event, ui) {
                //Add drop target text
                var $dropTarget = $(".dnnDropTarget");
                $dropTarget.empty().append("<p>" + settings.dropTargetText + "(" + $(this).attr("id").substring(4) + ")" + "</p>");
            },

            stop: function (event, ui) {
                var dropItem = ui.item;
                if (dnn.controlBar && dropItem.hasClass('ControlBar_ModuleDiv')) {
                    // add module
                	var pane = ui.item.parent();
                    var order = -1;
                    var paneName = pane.attr("id").substring(4);
	                if ($('div.DnnModule', pane).length > 0) {
		                var modules = $('div.DnnModule, div.ControlBar_ModuleDiv', pane);
		                for (var i = 0; i < modules.length; i++) {
			                var module = modules.get(i);
			                if ($(module).hasClass('ControlBar_ModuleDiv')) {
				                order = i;
			                }
		                }
	                }
	                dropItem.remove();
                    dnn.controlBar.addModule(dnn.controlBar.dragdropModule + '',
                        dnn.controlBar.dragdropPage,
                        paneName,
                        '-1',
                        order + '',
                        dnn.controlBar.dragdropVisibility + '',
                        dnn.controlBar.dragdropAddExistingModule + '',
                        dnn.controlBar.dragdropCopyModule + '');

                } else {
                    // move module
                    mid = getModuleId(dropItem);
                    updateServer(mid, dropItem.parent());
                    $(settings.actionMenu).show();

                    //remove the empty pane holder class for current pane
                    dropItem.parent().removeClass("dnnDropEmptyPanes");
                    // if original pane is empty, add dnnemptypane class
                    if (originalPane && $('div', originalPane).length === 0)
                        originalPane.addClass('dnnDropEmptyPanes');

                    // show animation
                    dropItem.css('background-color', '#fffacd');
                    setTimeout(function () {
                        dropItem.css('background', '#fffff0');
                        setTimeout(function () {
                            dropItem.css('background', 'transparent');
                        }, 300);
                    }, 2500);

                    $("div[data-tipholder=\"" + "ModuleDragToolTip-" + mid + "\"] .dnnHelpText").text(settings.dragHintText);
                   
                    $('.dnnDropEmptyPanes').each(function () {
                        $(this).removeClass('dnnDropEmptyPanes').addClass('DNNEmptyPane');
                    });
                    $('.contentPane').each(function () {
                        // this special code is for you -- IE8
                        this.className = this.className;
                    });

                    //fire window resize to reposition action menus
                    $(window).resize();
                }
            }
        });

        return $self;
    };
})(jQuery);