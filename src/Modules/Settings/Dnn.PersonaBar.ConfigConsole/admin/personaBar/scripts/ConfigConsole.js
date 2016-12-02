/*
DotNetNuke® - http://www.dotnetnuke.com
Copyright (c) 2002-2016
by DotNetNuke Corporation
All Rights Reserved
*/

/*
* Module responsible to Config Console
*/
'use strict';
define(['jquery',
        'knockout',
        'knockout.mapping',
		'jquery-ui.min',
        'main/codeEditor',
		'main/config',
        'dnn.jquery',
        'main/koBindingHandlers/jScrollPane'],
    function ($, ko, koMapping, jqueryUI, codeEditor, cf, jScrollPane) {
        var config = cf.init();

        var identifier, utility, $panel, viewModel, configEditor, mergeEditor, curConfigName;

        var requestService = function (type, method, params, callback, failure) {
            utility.sf.moduleRoot = "personaBar";
            utility.sf.controller = "ConfigConsole";

            utility.sf[type].call(utility.sf, method, params, callback, failure);
        }

        var getConfigs = function () {
            requestService('get', 'GetConfigFilesList', {}, function (data) {
                viewModel.configs(data.Results);
            }, function () {
                // failed
                utility.notifyError('Failed...');
            });
        }

        var getConfigFile = function () {
            requestService('get', 'GetConfigFile', { 'fileName': curConfigName }, function (data) {
                configEditor.setValue(data.FileContent);
            }, function () {
                // failed
                utility.notifyError('Failed...');
            });
        }

        var saveConfigFile = function () {
            utility.confirm(utility.resx.ConfigConsole.SaveConfirm, utility.resx.ConfigConsole.SaveButton, utility.resx.ConfigConsole.CancelButton, function () {
                requestService('post', 'UpdateConfigFile', { 'fileName': curConfigName, 'fileContent': configEditor.getValue() }, function (data) {
                    utility.notify(utility.resx.ConfigConsole.Success);
                }, function () {
                    // failed
                    utility.notifyError(utility.resx.ConfigConsole.ERROR_ConfigurationFormat);
                });
            });
        }

        var mergeConfigFile = function () {
            var confirmText = utility.resx.ConfigConsole.MergeConfirm;
            if (curConfigName != null && curConfigName == 'web.config') {
                confirmText = utility.resx.ConfigConsole.SaveWarning;
            }
            utility.confirm(confirmText, utility.resx.ConfigConsole.SaveButton, utility.resx.ConfigConsole.CancelButton, function () {
                requestService('post', 'MergeConfigFile', { 'fileName': '', 'fileContent': mergeEditor.getValue() }, function (data) {
                    utility.notify(utility.resx.ConfigConsole.Success);
                }, function () {
                    // failed
                    utility.notifyError(utility.resx.ConfigConsole.ERROR_Merge);
                });
            });
        }

        var initViewModel = function () {
            viewModel = {
                resx: utility.resx.ConfigConsole,
                configxml: ko.observable(''),
                mergexml: ko.observable(''),
                saveConfig: saveConfigFile,
                mergeConfig: mergeConfigFile,
                configs: ko.observableArray([]),
                config: ko.observable('')
            };

            viewModel.caption = ko.dependentObservable(function () {
                if (!this.config()) {
                    return utility.resx.ConfigConsole.plConfigHelp;
                }
            }, viewModel);
        }

        var configSelectionChanged = function (data) {
            if (data != null && data != curConfigName) {
                curConfigName = data;
                getConfigFile();
            }
        }

        var initUpload = function () {
            var $uploadContainer = $panel.find('.fileupload-wrapper');
            var $uploadControl = $uploadContainer.find('input');

            if (typeof FileReader === "undefined") {
                $uploadContainer.hide();
                return;
            }

            $uploadControl.on('change', function (e) {
                var file = $uploadControl[0].files[0];
                var textType = /text|xml.*/;

                if (file.type.match(textType) || file.name.toLowerCase().split('.').pop() === 'config') {
                    var reader = new FileReader();

                    reader.onload = function (e) {
                        mergeEditor.setValue(reader.result);
                    }

                    reader.readAsText(file);
                } else {
                    utility.notifyError('File not supported');
                }
            });
        }

        var init = function (wrapper, util, params, callback) {
            identifier = params.identifier;
            utility = util;
            $panel = wrapper;

            initViewModel();

            ko.applyBindings(viewModel, $panel[0]);

            curConfigName = config.portalId;

            getConfigs();

            configEditor = codeEditor.init($panel.find('.config-xml'), { mode: 'xml' });
            mergeEditor = codeEditor.init($panel.find('.script-xml'), { mode: 'xml' });
            configEditor.setSize("100%", 500);
            mergeEditor.setSize("100%", 500);

            configEditor.on("blur", function (cm) {
                cm.save();
                return true;
            });

            mergeEditor.on("blur", function (cm) {
                cm.save();
                return true;
            });

            viewModel.config.subscribe(configSelectionChanged);

            initUpload();

            $('.configConsolePanel .body').dnnTabs({ selected: 0 });

            if (typeof callback === 'function') {
                callback();
            }
        };

        var load = function (params, callback) {
            if (typeof callback === 'function') {
                callback();
            }
        };

        return {
            init: init,
            load: load
        };
    });
