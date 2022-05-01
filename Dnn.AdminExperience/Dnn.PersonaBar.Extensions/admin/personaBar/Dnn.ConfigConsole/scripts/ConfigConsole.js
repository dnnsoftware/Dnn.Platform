// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

/*
* Module responsible to Config Console
*/
'use strict';

define(['jquery',
  'knockout',
  'knockout.mapping',
  'jquery-ui.min',
  'main/config',
  'jquery.easydropdown.min',
  'dnn.jquery',
  'main/koBindingHandlers/jScrollPane'],
  function ($, ko, koMapping, jqueryUI, cf) {
    var config = cf.init();

    var utility, $panel, viewModel, monacoConfigFilesEditor, configFilesEditor, configFilesModel, configFilesContent, monacoMergeScriptsEditor, mergeScriptsEditor, mergeScriptsModel, mergeScriptsContent, curConfigName;

    var requestService = function (type, method, params, callback, failure) {
      utility.sf.moduleRoot = "personaBar";
      utility.sf.controller = "ConfigConsole";

      utility.sf[type].call(utility.sf, method, params, callback, failure);
    }

    var getConfigs = function () {
      requestService('get', 'GetConfigFilesList', {}, function (data) {
        // add the caption as first option
        data.Results.unshift(viewModel.resx.plConfigHelp);

        viewModel.configs(data.Results);

        $('.configConsolePanel select').easyDropDown({ wrapperClass: 'pb-dropdown', cutOff: 10, inFocus: true });
      }, function () {
        // failed
        utility.notifyError('Failed...');
      });
    }

    var getConfigFile = function () {
      if (curConfigName === viewModel.resx.plConfigHelp) {
        // it's the caption, so empty the editor
        configFilesContent.setValue('');
      } else {
        requestService('get', 'GetConfigFile', { 'fileName': curConfigName }, function (data) {
          if (curConfigName.endsWith("config")) {
            configFilesEditor.setModelLanguage(configFilesModel, "xml");
          }
          else if (curConfigName.endsWith("txt")) {
            configFilesEditor.setModelLanguage(configFilesModel, "plain/text");
          }
          configFilesContent.setValue(data.FileContent);
        }, function () {
          // failed
          utility.notifyError('Failed...');
        });
      }
    }

    var saveConfigFile = function () {
      utility.confirm(
        utility.resx.ConfigConsole.SaveConfirm,
        utility.resx.ConfigConsole.SaveButton,
        utility.resx.ConfigConsole.CancelButton,
        function () {
          validate({
            'fileName': curConfigName,
            'fileContent': configFilesContent.getValue()
          });
        }
      );
    }

    var validate = function (configFile) {
      var callback = function (data) {
        if (data && data.ValidationErrors && data.ValidationErrors.length) {
          confirmValidationErrors(data.ValidationErrors, configFile);
        } else {
          update(configFile);
        }
      };

      requestService('post', 'ValidateConfigFile', configFile, callback, fail);
    }

    var confirmValidationErrors = function (validationErrors, configFile) {
      const BR = '<br/>';
      const TOP = 5;

      var question = utility.resx.ConfigConsole.ValidationErrorsConfirm.replace('{0}', BR);

      // remove duplicates and take top N
      var errors = validationErrors
        .filter((value, index, self) => self.indexOf(value) === index)
        .slice(0, TOP)
        .concat(validationErrors.length > TOP ? ['...'] : []);

      utility.confirm(
        question + BR + BR + errors.join(BR),
        utility.resx.ConfigConsole.SaveButton,
        utility.resx.ConfigConsole.CancelButton,
        function () {
          update(configFile);
        }
      );
    }

    var update = function (configFile) {
      requestService('post', 'UpdateConfigFile', configFile, succeed, fail);
    }

    var succeed = () => {
      utility.notify(utility.resx.ConfigConsole.Success);
    }

    var fail = () => {
      utility.notifyError(utility.resx.ConfigConsole.ERROR_ConfigurationFormat);
    }

    var mergeConfigFile = function () {
      var confirmText = utility.resx.ConfigConsole.MergeConfirm;
      if (curConfigName != null && curConfigName == 'web.config') {
        confirmText = utility.resx.ConfigConsole.SaveWarning;
      }
      utility.confirm(confirmText, utility.resx.ConfigConsole.SaveButton, utility.resx.ConfigConsole.CancelButton, function () {
        requestService('post', 'MergeConfigFile', { 'fileName': '', 'fileContent': mergeScriptsContent.getValue() }, function (data) {
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
            mergeScriptsContent.setValue(reader.result);
          }

          reader.readAsText(file);
        } else {
          utility.notifyError('File not supported');
        }
      });
    }

    var init = function (wrapper, util, params, callback) {
      utility = util;
      $panel = wrapper;

      initViewModel();

      ko.applyBindings(viewModel, $panel[0]);

      curConfigName = config.portalId;

      getConfigs();

      initConfigConsole();

      initUpload();

      $('.configConsolePanel .body').dnnTabs({ selected: 0, activate: initTabsClickEvent() });
      setEditorHeight();

      if (typeof callback === 'function') {
        callback();
      }
    };

    var initTabsClickEvent = function () {
      const tabs = document.querySelectorAll('.configConsolePanel .tabs-nav li');
      tabs.forEach(tab => {
        tab.addEventListener('click', setEditorHeight);
      });
    }

    var setEditorHeight = function () {
      setTimeout(() => {
        var panelHeight = $('#ConfigConsole-panel').height();
        if (panelHeight > 400) {

          if (document.querySelector('#configConsole-files').style.display !== 'none') {
            $('#monaco-editor-config-files').height(panelHeight - 400);
          }

          if (document.querySelector('#configConsole-merge').style.display !== 'none') {
            $('#monaco-editor-merge-scripts').height(panelHeight - 400);
          }
        }
      }, 300);
    }

    var initConfigConsole = function () {
      let siteRoot = dnn.getVar("sf_siteRoot");
      if (!siteRoot) siteRoot = '/';
      var monacoEditorLoaderScript = document.createElement('script');
      monacoEditorLoaderScript.type = 'text/javascript';
      monacoEditorLoaderScript.src = siteRoot + 'Resources/Shared/components/MonacoEditor/loader.js';
      document.body.appendChild(monacoEditorLoaderScript);

      require.config({ paths: { 'vs': siteRoot + 'Resources/Shared/components/MonacoEditor' } });
      require(['vs/editor/editor.main'], function (monaco) {

        self.MonacoEnvironment = {
          getWorkerUrl: function (moduleId, label) {
            if (label === 'typescript' || label === 'javascript') {
              return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
                            importScripts('${process.env.ASSET_PATH}/typescript.worker.js');`
              )}`;
            }

            return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
                        importScripts('${process.env.ASSET_PATH}/editor.worker.js');`
            )}`;
          }
        }

        configFilesEditor = monaco.editor;
        mergeScriptsEditor = monaco.editor;

        configFilesContent = configFilesEditor.createModel("", "xml");
        mergeScriptsContent = mergeScriptsEditor.createModel("", "xml");

        initMonacoEditors();

        configFilesModel = monacoConfigFilesEditor.getModel();
        mergeScriptsModel = monacoMergeScriptsEditor.getModel();

        viewModel.config.subscribe(configSelectionChanged);
      });

    }

    var initMonacoEditors = function () {
      var theme = "vs-light";
      if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        theme = "vs-dark";
      }
      window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
        theme = e.matches ? "vs-dark" : "vs-light";
      });

      monacoConfigFilesEditor = configFilesEditor.create(document.getElementById("monaco-editor-config-files"), {
        model: configFilesContent,
        language: "xml",
        wordWrap: 'wordWrapColumn',
        wordWrapColumn: 80,
        wordWrapMinified: true,
        wrappingIndent: "indent",
        lineNumbers: "on",
        roundedSelection: false,
        scrollBeyondLastLine: false,
        readOnly: false,
        theme: theme,
        automaticLayout: true
      });

      monacoMergeScriptsEditor = mergeScriptsEditor.create(document.getElementById("monaco-editor-merge-scripts"), {
        model: mergeScriptsContent,
        language: "xml",
        wordWrap: 'wordWrapColumn',
        wordWrapColumn: 80,
        wordWrapMinified: true,
        wrappingIndent: "indent",
        lineNumbers: "on",
        roundedSelection: false,
        scrollBeyondLastLine: false,
        readOnly: false,
        theme: theme,
        automaticLayout: true
      });
    }

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
