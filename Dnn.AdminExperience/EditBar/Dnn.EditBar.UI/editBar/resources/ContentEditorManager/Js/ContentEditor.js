// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

if (typeof dnn === "undefined" || dnn === null) { dnn = {}; };
if (typeof dnn.ContentEditorManager === "undefined" || dnn.ContentEditorManager === null) { dnn.ContentEditorManager = {}; };

dnn.ContentEditorManager = $.extend({}, dnn.ContentEditorManager, {
    init: function (options) {
        if (options.type === "moduleManager" && options.panesClientIds && options.panes) {
            var panesClientIds = options.panesClientIds.split(';');
            var panes = options.panes.split(',');
            for (var i = 0; i < panes.length; i++) {
                (function (paneName, paneClientIds) {
                    var parentPaneClientIds = paneClientIds.split(',');
                    for (var j = 0; j < parentPaneClientIds.length; j++) {
                        (function (paneClientId) {
                            var pane = paneClientId.length > 0 ? $('[id=' + paneClientId + '][class~="dnnSortable"]') : null;
                            if (pane && pane.length > 0) {
                                pane.dnnModuleManager({
                                    pane: paneName,
                                    syncHandler: options.syncHandler,
                                    supportAjax: options.supportAjax
                                });
                            }
                        })(parentPaneClientIds[j]);
                    }
                })(panes[i], panesClientIds[i]);
            }
        }
    }
});

(function ($) {
	//update jquery to add custom logic
	var originaljQueryReady = $.fn.ready;
	var readyFunctionsInAjaxMode;

	var runReadyFunctionsInAjaxMode = function() {
		setTimeout(function () {
			var $this = $(document);
			for (var i = 0; i < readyFunctionsInAjaxMode.length; i++) {
			    (function(readyFunction) {
			        originaljQueryReady.call($this, function () {
			            try {
			                readyFunction.call($this);
			            } catch (ex) {
			                console.log('function execute failed: "' + ex.message + '" on: ' + readyFunction);
			            };
			        });
			    })(readyFunctionsInAjaxMode[i]);
			}

			readyFunctionsInAjaxMode = undefined;
		}, 100);
	}
	$.fn.ready = function(fn) {
		var $this = this;
		if (typeof window.dnnLoadScriptsInAjaxMode === "undefined") {
			originaljQueryReady.call($this, fn);
		} else {
			if (typeof readyFunctionsInAjaxMode == "undefined") {
				readyFunctionsInAjaxMode = [];
				if (window.dnnLoadScriptsInAjaxMode.length == 0) {
					runReadyFunctionsInAjaxMode();
				} else {
					$(window).one('dnnScriptLoadComplete', function() {
						runReadyFunctionsInAjaxMode();
					});
				}
				
			}

			readyFunctionsInAjaxMode.push(fn);
		}
	};

    dnn.ContentEditorManager.triggerChangeOnPageContentEvent = function () {
        $(document).trigger("changeOnPageContent");
    };

	dnn.ContentEditorManager.catchSortEvents = function (callback) {
    	var $allPanes = $('.dnnSortable');
    	var allPanesCount = $allPanes.length;
    	var catchedPanes = 0;

        $allPanes.each(function () {
        	var instance = this;
	        var catchSortEvents = function() {
		        if (!$(instance).data('ui-sortable')) {
		        	setTimeout(catchSortEvents, 100);
			        return;
		        }

		        catchedPanes++;

		        $(instance).sortable('option', 'helper', function(event, element) {
			        var helper = element.clone();
			        helper.addClass('floating forDrag').removeClass('CatchDragState');

			        var $dragHint = helper.find('> div.dnnDragHint');
			        var $dragContent = $('<div />');
			        $dragHint.append($dragContent);

			        var title = helper.find('span[id$="titleLabel"]:eq(0)').html();
			        $('<span class="title" />').appendTo($dragContent).html(title);

		            var $body = $(document.body);
		            var positionCss = $body.css('position');
		            var marginLeft = parseInt($body.css('margin-left'));
			        if (positionCss === "relative" && marginLeft) {
			            helper.css('margin-left', (0 - marginLeft) + "px");
			        }

			        $body.append(helper);
			        return helper;
		        });
		        $(instance).sortable('option', 'cursorAt', { left: 0, top: 0 });

		        //catch stop event
		        if ($(instance).sortable('option', 'start') === dnn.ContentEditorManager.sortStart) {
					if (catchedPanes === allPanesCount && typeof callback == "function") {
						callback();
					}

			        return;
		        }

		        $(instance).data('sortStartEvent', $(instance).sortable('option', 'start'));
		        $(instance).sortable('option', 'start', dnn.ContentEditorManager.sortStart);

		        if (!$(instance).data('eventCatched')) {
			        $(instance).on('sortbeforestop', function(event, ui) {
				        //catch stop event
				        var $newPane = ui.item.parent();
				        if ($newPane.sortable('option', 'stop') == dnn.ContentEditorManager.sortStop) {
					        return;
				        }

				        $newPane.data('sortStopEvent', $newPane.sortable('option', 'stop'));
				        $newPane.sortable('option', 'stop', dnn.ContentEditorManager.sortStop);

				        //catch stop event on original pane
				        var $oldPane = $(event.target);
				        if ($oldPane.sortable('option', 'stop') == dnn.ContentEditorManager.sortStop) {
					        return;
				        }

				        $oldPane.data('sortStopEvent', $oldPane.sortable('option', 'stop'));
				        $oldPane.sortable('option', 'stop', dnn.ContentEditorManager.sortStop);
			        });

			        $(instance).find('div.dnnDragHint').on('mousedown', dnn.ContentEditorManager.dragHandlerCheck);

			        $(instance).data('eventCatched', true);
		        }

				if (catchedPanes === allPanesCount && typeof callback == "function") {
					callback();
				}
	        };

	        catchSortEvents();
        });
    };

    dnn.ContentEditorManager.refreshContent = function (pane) {
        setTimeout(function() {
        	dnn.ContentEditorManager.catchSortEvents(function () {
	            $('div.actionMenu').show();

				//try to catch new add module and trigger addmodule event.
				var moduleDialog = dnn.ContentEditorManager.getModuleDialog();
				var cookieNewModuleId = moduleDialog.getModuleId();
				if (cookieNewModuleId) {
					var newModule = pane.find('div.DnnModule-' + cookieNewModuleId);
					if (newModule.length > 0) {
						moduleDialog.setModuleId(-1);
						newModule.trigger('editmodule');
					}
				}
            });
        }, 100);
    };

    dnn.ContentEditorManager.sortStart = function(event, ui) {
        window['cem_dragging'] = true; //add a global status when dragging module/layout.
        $(document.body).addClass('dnnModuleSorting');
	    $(this).data('sortStartEvent').call(this, event, ui);
    };

    dnn.ContentEditorManager.sortStop = function (event, ui) {
        var newPane = ui.item.parent(), oldPane = $(this);
        var instance = this;
        var $item = ui.item;
        $(document.body).removeClass('dnnModuleSorting');
		$('div.DnnModule.CatchDragState').removeClass('dragging').trigger('dragend');
        $item.removeClass('floating').css({left: '', top: '', position: '', zIndex: ''});
        $('.actionMenu').removeClass('floating');

        newPane.find('div.handlerContainer').hide(); // hide quick add module handler.

        $item.addClass('highlight').removeClass('drift forDrag');
        setTimeout(function() {
            $item.addClass('animate');
        }, 1000);
        setTimeout(function() {
            $item.removeClass('highlight animate');
        }, 1500);

        $('div.dnnDragHint').removeClass('dnnDragDisabled');

        window['cem_dragging'] = false;

        var relatedModules = $item.data('relatedModules');
        if (relatedModules && relatedModules.length > 0) {
            var serviceController = new dnn.dnnModuleService({
                service: 'InternalServices',
                controller: 'ModuleService',
                async: false
            });
            var tabId = serviceController.getTabId();
            for (var i = 0; i < relatedModules.length; i++) {
                var moduleId = relatedModules[i];
                var dataVar = {
                    TabId: tabId,
                    ModuleId: moduleId,
                    Pane: newPane.attr('id').replace('dnn_', ''),
                    ModuleOrder: -1
                };

                var onSuccess = function() {
                    dnn.ContentEditorManager.triggerChangeOnPageContentEvent();
                }
                serviceController.request('MoveModule', 'POST', dataVar, onSuccess);
            }
        }

        var moduleDialog = dnn.ContentEditorManager.getModuleDialog();

		var getPanesId = function() {
			var oldPaneId = oldPane.attr('id');
            var newPaneId = newPane.attr('id');

            if (oldPane.data('parentpane')) {
                oldPaneId = $('[id$=' + oldPane.data('parentpane') + '][class~="dnnSortable"]').attr('id');
            }

            if (newPane.data('parentpane')) {
                newPaneId = $('[id$=' + newPane.data('parentpane') + '][class~="dnnSortable"]').attr('id');
            }

            var panes = [oldPaneId];
			if (newPaneId !== oldPaneId) {
				panes.push(newPaneId);
			}

			return panes;
		}

		var refreshPane = function () {
			var panesId = getPanesId();
            moduleDialog.refreshPane(panesId[0], '', function() {
                dnn.ContentEditorManager.refreshContent(this);

                if (panesId.length > 1) {
	                setTimeout(function() {
		                moduleDialog.refreshPane(panesId[1], '', function() {
			                dnn.ContentEditorManager.refreshContent(this);
		                });
	                }, 100);
                }
            });
		}

		var showLoading = function (paneId) {
			var $pane = $('#' + paneId);
			 var loading = $("<div class=\"dnnLoading\"></div>");
            loading.css({
                width: $pane.outerWidth(),
                height: $pane.outerHeight()
            });
            $pane.before(loading);
		}

        var refreshContent = function() {
        	moduleDialog._resetPending();

            var panesId = getPanesId();
			for (var i = 0; i < panesId.length; i++) {
				showLoading(panesId[i]);
			}

            $(instance).data('sortStopEvent').call(instance, event, ui, function () {
                dnn.ContentEditorManager.triggerChangeOnPageContentEvent();
                refreshPane();
            });
        }

        refreshContent();
    };

	dnn.ContentEditorManager.dragHandlerCheck = function(e) {
        if ($(this).hasClass('dnnDragDisabled')) {
            e.stopImmediatePropagation();
        }

		if ($(this).parent().hasClass('CatchDragState')) {
		    $('div.DnnModule.CatchDragState').find('div.pane').trigger('dragstart');
	    }
    };

	$(document).ready(function() {
        dnn.ContentEditorManager.catchSortEvents();
    });

	$(window).on('load', function handlerNewModuleFromCookie() {
        //handle the floating module from cookie
        var handleNewModuleFromCookie = function handleNewModuleFromCookie() {
            var moduleDialog = dnn.ContentEditorManager.getModuleDialog();

            var cookieModuleId = moduleDialog._getCookie('CEM_CallbackData');
            if (cookieModuleId && cookieModuleId.indexOf('module-') > -1) {
                var moduleId = cookieModuleId.substr(7);
                var existingModule = moduleDialog._getCookie('CEM_ExistingModule');
                if (!existingModule) existingModule = false;

                var module = $('div.DnnModule-' + moduleId);
                var moduleManager = module.parent().data('dnnModuleManager');

                moduleDialog.apply(moduleManager, existingModule);
                moduleDialog._processModuleForDrag(module);
                $('#moduleActions-' + moduleId).addClass('floating');
                moduleDialog._removeCookie('CEM_CallbackData');
            } else {
                var cookieNewModuleId = moduleDialog.getModuleId();
                if (cookieNewModuleId) {
                    var newModule = $('div.DnnModule-' + cookieNewModuleId);
                    moduleDialog.setModuleId(-1);

                    dnn.ContentEditorManager.triggerChangeOnPageContentEvent();
                    newModule.trigger('editmodule');
                }
            }
        }

        setTimeout(handleNewModuleFromCookie, 500);
    });
}(jQuery));
