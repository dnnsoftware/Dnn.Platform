// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

/*
* Module responsible to Sql Console
*/
define(['jquery',
  'knockout',
  'knockout.mapping',
  './sort',
  './exportData',
  './clipboard.min',
  './html2canvas',
  './FileSaver.min',
  'dnn.jquery',
  'main/koBindingHandlers/jScrollPane'],
  function ($, ko, koMapping, sort, exportData, Clipboard) {
    'use strict';

    var utility, resx, $panel, viewModel, sqlConsole, sqlContent, jsPdf;

    var pagesCount = 7;

    var requestService = function (type, method, params, callback, error) {
      utility.sf.moduleRoot = "personaBar";
      utility.sf.controller = "SqlConsole";

      utility.sf[type].call(utility.sf, method, params, callback, error);
    }

    var getSavedQueries = function (selectedId) {
      requestService('get', 'GetSavedQueries', {}, function (data) {
        viewModel.connections(data.connections);
        viewModel.connection(data.connections[0]);

        var newQuery = {
          id: -1,
          name: resx.NewQuery,
          connection: viewModel.connection(),
          query: ''
        };

        viewModel.savedQueries.removeAll();
        viewModel.savedQueries.push(newQuery);
        for (var i = 0; i < data.queries.length; i++) {
          viewModel.savedQueries.push(data.queries[i]);
        }

        viewModel.selectedQuery(selectedId);

        if (!viewModel.name()) {
          viewModel.name(newQuery.name);
        }
      });
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
        var textType = /text.*/;
        if (file.type.match(textType) || file.name.toLowerCase().split('.').pop() === 'sql') {
          var reader = new FileReader();

          reader.onload = function (e) {
            viewModel.query(reader.result);
          }

          reader.readAsText(file);
        } else {
          utility.notifyError('File not supported');
        }

        $uploadControl.val('');
      });
    }

    var changeTab = function (data) {
      var index = data.index;

      $panel.find('.tables-container > div').hide();
      $panel.find('.tables-container > div#result' + index).show();
      var $tableContainer = $panel.find('.tables-container > div#result' + index).find('> div.table-container');
      if ($tableContainer.data('jsp')) {
        $tableContainer.data('jsp').destroy();
        $tableContainer = $panel.find('.tables-container > div#result' + index).find('> div.table-container');
      }
      $tableContainer.jScrollPane();

      $panel.find('.result-tabs ul li').removeClass('selected').eq(index).addClass('selected');
    }

    var scrollPaneInitialised = function (table, e) {
      var $table = $panel.find('.tables-container > div#result' + table.index).find('table');
      if (!$table.attr('width') || $table.attr('width') === '0') {
        $table.attr('width', $table.width());
      }
    }

    var savedQueryChanged = function (val) {
      var id = viewModel.selectedQuery();
      var query;
      var savedQueries = viewModel.savedQueries();
      for (var i = 0; i < savedQueries.length; i++) {
        if (savedQueries[i].id === id) {
          query = savedQueries[i];
          break;
        }
      }

      if (!query || !query.name) {
        return;
      }

      viewModel.query(query.query || '');
      viewModel.name(query.name);
      viewModel.connection(query.connection);
      viewModel.id(query.id || -1);
    }

    var startSaveQuery = function () {
      viewModel.query(sqlContent.getValue());

      if (!viewModel.query()) {
        return;
      }

      viewModel.saving(true);

      if (viewModel.id() <= 0) {
        viewModel.name('');
      }

      $('#query-name').focus();
    }

    var saveQuery = function () {
      viewModel.query(sqlContent.getValue());

      if (!viewModel.query()) {
        return;
      }

      if (!viewModel.name()) {
        utility.notify(resx.EmptyName);
        return;
      }

      var query = {
        id: viewModel.id(),
        name: viewModel.name(),
        query: viewModel.query(),
        connection: viewModel.connection()
      }

      viewModel.loading(true);

      requestService('post', 'SaveQuery', query, function (data) {
        getSavedQueries(data.id);
        viewModel.saving(false);
        viewModel.loading(false);
      });
    }

    var cancelSave = function () {
      viewModel.saving(false);
    }

    var deleteQuery = function () {
      var confirmText = resx.DeleteConfirm;
      var deleteText = resx.Delete;
      var cancelText = resx.Cancel;

      utility.confirm(confirmText, deleteText, cancelText, function () {
        viewModel.loading(true);
        requestService('post', 'DeleteQuery', { id: viewModel.id() }, function () {
          getSavedQueries();
          viewModel.loading(false);
        });
      });
    }

    var clone = function (data) {
      var cloneData = {};
      for (var name in data) {
        if (data.hasOwnProperty(name)) {
          cloneData[name] = data[name];
        }
      }

      return cloneData;
    }

    var encode = function (item) {
      for (var name in item) {
        if (item.hasOwnProperty(name) && item[name]) {
          item[name] = $('<div/>').text(item[name]).html();
        }
      }

      return item;
    };

    var matchKeyword = function (data, keyword) {
      if (!keyword) {
        return true;
      }

      var keywords = keyword.toLowerCase().split(' ');

      var allEmptyKey = true;
      for (var i = 0; i < keywords.length; i++) {
        var key = keywords[i].trim();
        if (key) {
          allEmptyKey = false;
          break;
        }
      }

      if (allEmptyKey) {
        return true;
      }

      var matched = false;
      for (var name in data) {
        if (data.hasOwnProperty(name) && data[name]) {
          for (var i = 0; i < keywords.length; i++) {
            var key = keywords[i].trim();
            if (key) {
              var regexVal = key.replace(/[|\\{}()[\]^$+*?.]/g, '\\$&');
              var regex = new RegExp('(' + regexVal + ')', 'gi');
              if (regex.test(data[name].toString())) {
                data[name] = data[name].toString().replace(regex, '<span class="highlight">$1</span>');
                matched = true;
              }
            }
          }
        }
      }

      return matched;
    }

    var normalizeType = function (value) {
      if (value === "" || value === null || typeof value === "undefined") {
        return "";
      }
      var type = typeof value;
      switch (type) {
        case 'boolean':
          return value;
        case 'number':
          return value;
        case 'string':
          var lcValue = value.toLowerCase();
          if (isFinite(lcValue)) {
            return parseFloat(lcValue);
          } else if (lcValue == "true") {
            return true;
          } else if (lcValue == "false") {
            return false;
          }
          return lcValue;
        default:
          return value;

      }
    };

    var createDataTable = function (data) {
      var table = { header: [], rows: ko.observableArray(data) };
      //try to find headers
      var row = data[0];
      for (var name in row) {
        if (row.hasOwnProperty(name)) {
          table.header.push(name);
        }
      }

      table.sortColumn = ko.observable(table.header[0]);
      table.sortType = ko.observable(0);

      table.keywords = ko.observable('');
      table.keywords.subscribe(function () {
        table.currentPage(1);
      });
      table.keywords.extend({ rateLimit: 200 });

      //try to get page info
      table.pageSizes = ko.observableArray([
        { name: resx.AllEntries, value: 0 },
        { name: resx.PageSize.replace('{0}', 10), value: 10 },
        { name: resx.PageSize.replace('{0}', 25), value: 25 },
        { name: resx.PageSize.replace('{0}', 50), value: 50 },
        { name: resx.PageSize.replace('{0}', 100), value: 100 }
      ]);

      table.pageSize = ko.observable(10);
      table.showPageSizesList = ko.observable(false);

      table.pageSizesLabel = ko.computed(function () {
        var sizes = table.pageSizes();
        for (var i = 0; i < sizes.length; i++) {
          if (sizes[i].value === table.pageSize()) {
            return sizes[i].name;
          }
        }

        return '';
      });

      table.changePageSize = function () {
        table.pageSize(this.value);
        table.currentPage(1);

        var $currentTable = $('.tables-container > div:visible').find('div.table-container');
        $currentTable.data('jsp').destroy();

        $currentTable = $('.tables-container > div:visible').find('div.table-container');
        $currentTable.jScrollPane();

        table.showPageSizesList(false);
      }

      table.currentPage = ko.observable(1);
      table.currentPage.subscribe(function () {
        setTimeout(function () {
          var $currentTable = $('.tables-container > div:visible').find('div.table-container');
          if ($currentTable.data('jsp')) {
            $currentTable.data('jsp').destroy();
          }
        }, 0);
        setTimeout(function () {
          var $currentTable = $('.tables-container > div:visible').find('div.table-container');

          if (!$currentTable.data('jsp')) {
            $currentTable.jScrollPane();
          }
        }, 10);

      });

      table.startIndex = ko.computed(function () {
        return (table.currentPage() - 1) * table.pageSize();
      });

      table.filterData = ko.computed(function () {
        var filterData = ko.observableArray();
        var source = table.rows();
        for (var i = 0; i < source.length; i++) {
          var keywords = table.keywords();
          var item = encode(clone(source[i]));
          if (matchKeyword(item, keywords)) {
            filterData.push(item);
          }
        }

        var sortType = table.sortType();
        if (sortType !== 0) {
          filterData.sort(function (left, right) {
            var sortColumn = table.sortColumn();
            var leftData = normalizeType(left[sortColumn]);
            var rightData = normalizeType(right[sortColumn]);

            if (leftData === rightData) {
              return 0;
            } else if (leftData === "" || leftData < rightData) {
              return sortType === 1 ? -1 : 1;
            } else if (leftData > rightData || rightData === "") {
              return sortType === 1 ? 1 : -1;
            }
          });
        }

        return filterData();
      });

      table.endIndex = ko.computed(function () {
        var endIndex = table.startIndex() + table.pageSize();

        if (table.pageSize() === 0 || endIndex > table.filterData().length) {
          endIndex = table.filterData().length;
        }
        return endIndex;
      });

      table.totalPages = ko.computed(function () {
        if (table.pageSize() === 0) {
          return 0;
        }
        var totalPages = parseInt(table.filterData().length / table.pageSize());
        if (table.filterData().length % table.pageSize() !== 0) {
          totalPages++;
        }
        return totalPages;
      });

      table.statistics = ko.computed(function () {
        var args = [table.startIndex() + 1, table.endIndex(), table.filterData().length];
        return viewModel.resx.PageInfo.replace(/\{(\d+)\}/gi, function (i) { var index = parseInt(arguments[1]); return args[index]; });
      });

      table.pagesNumber = ko.computed(function () {
        var pages = [];
        var totalPages = table.totalPages();
        if (totalPages <= pagesCount) {
          for (var i = 1; i <= totalPages; i++) {
            pages.push(i);
          }
        } else {
          var currentPage = table.currentPage();
          var split = parseInt(pagesCount / 2);
          var start = currentPage > split ? currentPage - split : 1;
          var end = totalPages - currentPage < split ? totalPages : start + pagesCount - 1;

          if (end - start !== pagesCount - 1) {
            start = end - pagesCount + 1;
          }

          for (var i = start; i <= end; i++) {
            pages.push(i);
          }
        }

        return pages;
      });

      table.prev = function (e) {
        if (table.currentPage() > 1) {
          table.currentPage(table.currentPage() - 1);
        }
      }

      table.next = function (e) {
        if (table.currentPage() < table.totalPages()) {
          table.currentPage(table.currentPage() + 1);
        }
      }

      table.changePage = function (page, e) {
        table.currentPage(page);
      };

      table.pageData = ko.computed(function () {
        var data = [];
        var filterData = table.filterData();
        for (var i = table.startIndex(); i < table.endIndex(); i++) {
          data.push(filterData[i]);
        }
        return data;
      });

      table.hasData = ko.computed(function () {
        return table.pageData().length > 0 || table.keywords().length > 0;
      });

      var exportFile = function (name, data) {
        var blob = new Blob([data], {
          type: "application/vnd.ms-excel;charset=utf-8"
        });
        saveAs(blob, name);
      }

      var exportExcel = function () {
        viewModel.loading(true);
        var pageSize = table.pageSize();
        var currentPage = table.currentPage();
        table.currentPage(1);
        table.pageSize(table.filterData().length);

        exportFile(table.title + '.xls', exportData.excel($('#result' + table.index + ' table')[0]));

        table.pageSize(pageSize);
        table.currentPage(currentPage);
        table.showExportList(false);
        viewModel.loading(false);
      }

      var exportCsv = function () {
        viewModel.loading(true);
        var pageSize = table.pageSize();
        var currentPage = table.currentPage();
        table.currentPage(1);
        table.pageSize(table.filterData().length);

        exportFile(table.title + '.csv', exportData.csv($('#result' + table.index + ' table')[0]));

        table.pageSize(pageSize);
        table.currentPage(currentPage);
        table.showExportList(false);
        viewModel.loading(false);
      }

      var exportPdf = function () {
        viewModel.loading(true);
        var pageSize = table.pageSize();
        var currentPage = table.currentPage();
        table.currentPage(1);
        table.pageSize(table.filterData().length);
        var selector = '#result' + table.index + ' .table-container';
        var $container = $(selector);

        if ($container.data('jsp')) {
          $container.data('jsp').destroy();
          $container = $(selector);
        }

        var $table = $container.find('table');
        var $line = $('<div />').css({
          height: '1px',
          width: $table.outerWidth(),
          opacity: 0
        });
        $('.sqlconsolePanel .results-container').after($line);

        html2canvas($table[0], {
          onrendered: function (canvas) {
            table.pageSize(pageSize);
            table.currentPage(currentPage);
            table.showExportList(false);
            viewModel.loading(false);

            var $wrapper = $('<div style="background-color: #fff;" />');
            $wrapper.width(canvas.width).height(canvas.height);
            $wrapper.append(canvas);
            $(document.body).append($wrapper);

            var generatePdf = function () {
              var layout = canvas.height > canvas.width ? 'p' : 'l';
              var pdf = new jsPdf(layout, 'pt', [canvas.width, canvas.height]);
              pdf.addHTML($wrapper[0], function () {
                pdf.save(table.title + '.pdf');
                $wrapper.remove();
                $line.remove();

                if (!$container.data('jsp')) {
                  $container.jScrollPane();
                }
              });
            }

            if (typeof jsPdf === "undefined") {
              require(['main/../modules/dnn.sqlconsole/scripts/jspdf'], function (pdf) {
                jsPdf = pdf;
                generatePdf();
              });
            } else {
              generatePdf();
            }
          }
        });
      }

      var exportClipboard = function () {
        viewModel.loading(true);
        var pageSize = table.pageSize();
        var currentPage = table.currentPage();
        table.currentPage(1);
        table.pageSize(table.filterData().length);

        var $download = $('<a href="#"></a>').appendTo(document.body);
        $download.click(function (e) {
          e.preventDefault();
        });

        var clipboard = new Clipboard($download[0], {
          text: function (trigger) {
            return exportData.csv($('#result' + table.index + ' table')[0], '\t');
          }
        });

        clipboard.on('success', function (e) {
          clipboard.destroy();
          $download.remove();

          utility.notify(resx.ExportClipboardSuccessful);

          table.pageSize(pageSize);
          table.currentPage(currentPage);
          table.showExportList(false);
          viewModel.loading(false);
        });

        clipboard.on('error', function (e) {
          e.clearSelection();

          clipboard.destroy();
          $download.remove();

          utility.notify(resx.ExportClipboardFailed);

          table.pageSize(pageSize);
          table.currentPage(currentPage);
          table.showExportList(false);
          viewModel.loading(false);
        });

        $download[0].click();
      }

      var getExportMethods = function () {
        return [
          {
            name: resx.ExportExcel,
            onExport: exportExcel
          },
          {
            name: resx.ExportCSV,
            onExport: exportCsv
          }, {
            name: resx.ExportPDF,
            onExport: exportPdf
          }, {
            name: resx.ExportClipboard,
            onExport: exportClipboard
          }];
      }

      table.showExportList = ko.observable(false);
      table.exportMethods = getExportMethods();

      return table;
    }

    var renderData = function (data) {
      viewModel.tables.removeAll();
      for (var i = 0; i < data.length; i++) {
        var table = createDataTable(data[i]);
        if (table != null) {
          table.title = resx.QueryTabTitle.replace('{0}', (i + 1));
          table.index = i;
          viewModel.tables.push(table);
        }
      }
    }

    var runQuery = function () {
      var newQuery = sqlContent.getValue();
      viewModel.query(newQuery);

      if (!viewModel.query()) {
        return;
      }

      var params = {
        connection: viewModel.connection(),
        query: viewModel.query()
      }
      viewModel.tables.removeAll();
      viewModel.loading(true);

      requestService('post', 'RunQuery', params, function (data) {
        if (data.Data) {
          renderData(data.Data);

          var $animateControl = $('div.CodeMirror');
          if ($animateControl.height() > 100) {
            $animateControl.animate({ height: '-=250' }, 'fast').on('click.sqlconsole', function () {
              $animateControl.animate({ height: '+=250' }, 'fast');
              $animateControl.off('click.sqlconsole');
            });
          }
        } else {
          utility.notify(resx.QuerySuccessful);
        }

        viewModel.loading(false);
      }, function (xhr) {
        var message = resx.QueryFailed;
        if (xhr.responseText) {
          var response = JSON.parse(xhr.responseText);
          if (response['Error'] || response['Message']) {
            message = response['Error'] || response['Message'];
          }
        }
        viewModel.loading(false);
        utility.notifyError(htmlDecode(message), { clickToClose: true });
      });
    }
    function htmlDecode(textToDecode) {
      return $('<div />').text(textToDecode).html();
    }
    var queryChanged = function (data) {
      sqlContent.setValue(data);
    }

    var initViewModel = function () {
      viewModel = {
        resx: utility.resx.SqlConsole,
        savedQueries: ko.observableArray([]),
        selectedQuery: ko.observable(),
        connections: ko.observableArray([]),
        id: ko.observable(0),
        name: ko.observable(''),
        connection: ko.observable(''),
        query: ko.observable(''),
        loading: ko.observable(false),
        runQuery: runQuery,
        tables: ko.observableArray([]),
        changeTab: changeTab,
        scrollPaneInitialised: scrollPaneInitialised,
        saving: ko.observable(false),
        startSaveQuery: startSaveQuery,
        saveQuery: saveQuery,
        deleteQuery: deleteQuery,
        cancelSave: cancelSave,
        savedQueryChanged: savedQueryChanged,
      };

      viewModel.selectedQuery.subscribe(savedQueryChanged);
    }


    var init = function (wrapper, util, params, callback) {
      utility = util;
      resx = utility.resx.SqlConsole;
      $panel = wrapper;

      initViewModel();

      viewModel.query.subscribe(queryChanged);

      ko.applyBindings(viewModel, $panel[0]);

      getSavedQueries(-1);
      initUpload();

      initSqlConsole();

      if (typeof callback === 'function') {
        callback();
      }
    };

    var initSqlConsole = function () {
      let siteRoot = dnn ? dnn.getVar("sf_siteRoot") : '/';
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

        sqlConsole = monaco.editor;
        sqlContent = sqlConsole.createModel("", "sql");
        initMonacoEditor();
      });

    }

    var initMonacoEditor = function () {
      var theme = "vs-light";
      if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        theme = "vs-dark";
      }
      window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
        theme = e.matches ? "vs-dark" : "vs-light";
      });

      var monacoEditor = sqlConsole.create(document.getElementById("monaco-editor"), {
        model: sqlContent,
        language: "sql",
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
