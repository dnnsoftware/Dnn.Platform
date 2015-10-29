if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.templatesViewModel = function (rootViewModel, config) {
    var self = this;
    var resx = config.resx;
    var settings = config.settings;
    var util = config.util;
    var $rootElement = config.$rootElement;
    var ko = config.ko;

    self.rootViewModel = rootViewModel;

    self.contentTypes = ko.observableArray([]);

    self.mode = config.mode;
    self.isSystemUser = settings.isSystemUser;
    self.searchText = ko.observable("").extend({ throttle: 500 });
    self.results = ko.observableArray([]);
    self.totalResults = ko.observable(0);
    self.pageSize = ko.observable(settings.pageSize);
    self.pager_PageDesc = resx.pager_PageDesc;
    self.pager_PagerFormat = resx.templates_PagerFormat;
    self.pager_NoPagerFormat = resx.templates_NoPagerFormat;
    // ReSharper disable once InconsistentNaming
    self.selectedTemplate = new dcc.templateViewModel(self, config);

    var findTemplates = function () {
        self.pageIndex(0);
        self.getTemplates();
    };

    var getContentTypes = function () {
        var params = {
            searchTerm: '',
            pageIndex: 0,
            pageSize: 1000
        };

        util.contentTypeService().getEntities("GetContentTypes",
            params,
            self.contentTypes,
            function () {
                // ReSharper disable once InconsistentNaming
                return new dcc.contentTypeViewModel(self, config);
            }
            );
    };

    self.addTemplate = function (event, ui) {
        var tbody = $rootElement.find("#templates-addbody");

        $(ui.target).fadeOut(200);
        util.asyncParallel([
            function (cb1) {
                self.selectedTemplate.init();
                self.selectedTemplate.isEditMode(true);
                self.selectedTemplate.bindCodeEditor();
                self.mode("editTemplate");
                cb1();
            },
            function (cb2) {
                $rootElement.find('#templates-editrow > td > div').slideUp(200, 'linear', function () {
                    cb2();
                });
            }
        ], function () {
            $rootElement.find('#templates-editrow').appendTo(tbody);
            $rootElement.find('#templates-editrow > td > div').slideDown(400, 'linear');
        });
    };

    var collapseDetailRow = function (cb) {
        $rootElement.find("tr.in-edit-row").removeClass('in-edit-row');
        $rootElement.find('a.dccButton').fadeIn(200);

        $rootElement.find('#templates-editrow > td > div').stop(true, false).slideUp(600, 'linear', function () {
            $rootElement.find('#templates-editrow').appendTo('#templates-editbody');
            if (typeof cb === 'function') cb();
        });
    };

    self.closeEdit = function () {
        self.mode("listTemplates");
        collapseDetailRow(function() {
            self.refresh();
        });
        self.selectedTemplate.isEditMode(false);
    }



    self.editTemplate = function (data, e) {
        self.selectedTemplate.init();
        self.selectedTemplate.isEditMode(true);
        $rootElement.find('a.dccButton').fadeIn(200);

        var row = $rootElement.find(e.target);

        if (row.is("tr") === false) {
            row = row.closest('tr');
        }

        if (row.hasClass('in-edit-row')) {
            row.removeClass('in-edit-row');
            $rootElement.find('#templates-editrow > td > div').stop(true, false).slideUp(600, 'linear', function () {
                $rootElement.find('#templates-editrow').appendTo('#templates-editbody');
            });
            return;
        }

        var tbody = row.parent();
        $rootElement.find('tr', tbody).removeClass('in-edit-row');
        row.addClass('in-edit-row');

        util.asyncParallel([
            function (cb1) {
                self.getTemplate(data.templateId(), cb1);
            },
            function (cb2) {
                $rootElement.find('#templates-editrow > td > div').stop(true, false).slideUp(200, 'linear', function () {
                    cb2();
                });
            }
        ], function () {
            self.mode("editTemplate");
            $rootElement.find('#templates-editrow').insertAfter(row);
            $rootElement.find('#templates-editrow > td > div').stop(true, false).slideDown(400, 'linear');
        });
    };

    self.getTemplate = function (templateId, cb) {
        var params = {
            templateId: templateId
        };
        util.templateService().getEntity("GetTemplate",
            params,
            self.selectedTemplate,
            function () {
                self.selectedTemplate.bindCodeEditor();
            }
        );

        if (typeof cb === 'function') cb();
    };

    self.getTemplates = function () {

        getContentTypes();

        var params = {
            searchTerm: self.searchText(),
            pageIndex: self.pageIndex(),
            pageSize: self.pageSize()
        };

        util.templateService().getEntities("GetTemplates",
            params,
            self.results,
            function () {
                // ReSharper disable once InconsistentNaming
                return new dcc.templateViewModel(self, config);
            },
            self.totalResults
            );
    };

    self.init = function () {
        // ReSharper disable once UseOfImplicitGlobalInFunctionScope
        dnn.koPager().init(self, config);
        self.searchText.subscribe(function () {
            findTemplates();
        });
        self.pageSize.subscribe(function () {
            findTemplates();
        });
        $rootElement.find("#templates-editView").css("display", "none");
    };

    self.refresh = function () {
        self.getTemplates();
    };
};

dcc.templateViewModel = function (parentViewModel, config) {
    var self = this;
    var util = config.util;
    var resx = config.resx;
    var codeEditor = config.codeEditor;
    var ko = config.ko;
    var invalidCharsRegEx = /[|"<>]/g;
    var replacementCharsRegEx = /[\s|"<>/]/g;
    var $rootElement = config.$rootElement;
    var $contextMenu = $rootElement.find("#templateEditorContextMenu");

    self.parentViewModel = parentViewModel;
    self.rootViewModel = parentViewModel.rootViewModel;

    self.canEdit = ko.observable(false);
    self.canSelectGlobal = ko.observable(false);
    self.templateId = ko.observable(-1);
    self.localizedNames = ko.observableArray([]);
    self.contentTypeId = ko.observable(-1);
    self.filePath = ko.observable('');
    self.isSystem = ko.observable(false);
    self.content = ko.observable('');
    self.selected = ko.observable(false);
    self.isEditMode = ko.observable(false);
    self.isEditTemplate = ko.observable(false);

    self.contentTypes = parentViewModel.contentTypes;
    var codeSnippets = [];

    self.isAddMode = ko.computed(function () {
        return self.templateId() === -1;
    });

    self.name = ko.computed({
        read: function () {
            return util.getLocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedNames());
        },
        write: function (value) {
            util.setlocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedNames(), value);
        }
    });

    self.name.subscribe(function (newValue) {
        if (self.filePath() === "" && newValue !== "") {
            self.filePath("Content Templates/" + newValue.replace(replacementCharsRegEx, "") + ".cshtml");
        }
    });

    var getCodeSnippets = function (onSuccess) {
        if (codeSnippets.length === 0) {
            var params = {};

            util.templateService().get("GetSnippets", params,
                function(data) {
                    if (typeof data !== "undefined" && data != null) {
                        //Success
                        codeSnippets.splice(0, codeSnippets.length);
                        for (var i = 0; i < data.results.length; i++) {
                            var result = data.results[i];
                            codeSnippets.push({
                                name: result.name,
                                snippet: result.snippet
                            });
                        }
                    }
                    if (typeof onSuccess === 'function') onSuccess();
                },
                function() {
                    //Failure
                }
            );
        } else {
            if (typeof onSuccess === 'function') onSuccess();
        }
    };

    var $element = function (element, props) {
        var $e = $(document.createElement(element));
        props && $e.attr(props);
        return $e;
    };

    var insertField = function (event) {
        var doc = codeEditor.doc;
        doc.replaceSelection("@Dnn.DisplayFor(\"" + event.data.field + "\")");
        $contextMenu.hide();
    };

    var insertSnippet = function (event) {
        var doc = codeEditor.doc;
        doc.replaceSelection(event.data.snippet);
        $contextMenu.hide();
    };

    var getContentFields = function (onSuccess) {
        if (self.contentTypeId() !== "undefined" && self.contentTypeId() > 0 && self.contentTypeId() !== self.previousContentTypeId) {
            var params = {
                contentTypeId: self.contentTypeId()
            };

            util.contentTypeService().get("GetAllContentFields", params,
                function (data) {
                    if (typeof data !== "undefined" && data != null) {
                        //Success
                        if (typeof onSuccess === 'function') onSuccess(data.results);
                    }
                },

                function () {
                    //Failure
                }
            );

            self.previousContentTypeId = self.contentTypeId();
        }
    };

    var configureSubMenu = function(fields) {
        var $contentFields = $element("ul");
        var field, name, childFields;
        for (var i = 0; i < fields.length; i++) {
            if (fields[i].fields === undefined) {
                field = fields[i].fieldName;
                name = fields[i].name;
                $contentFields.append(
                    $element("li").append(
                        $element("div").append(
                            $element("span").text(name)
                        )
                    ).on("click", { field: field }, insertField)
                );
            } else {
                childFields = fields[i].fields;
                name = fields[i].name;
                $contentFields.append(
                    $element("li").append(
                        $element("div").append(
                            $element("span").text(name),
                            $element("i", { "class": "fa fa-caret-right" })
                        ),
                        configureSubMenu(childFields)
                    )
                );
            }
        }

        return $contentFields;
    }

    var configureContextMenu = function () {
        if (self.isEditMode()) {
            //remove exisiting context menu items
            $contextMenu.children().remove();

            //Build Framework
            $contextMenu.append(
                $element("li", { "class": "contentField" }).append(
                    $element("div").append(
                        $element("span").text(resx.insertField),
                        $element("i", { "class": "fa fa-caret-right" })
                    )
                ),
                $element("li", { "class": "codeSnippet" }).append(
                    $element("div").append(
                        $element("span").text(resx.insertHelper),
                        $element("i", { "class": "fa fa-caret-right" })
                    )
                )
            );

            //Add content Fields
            getContentFields(function(data) {
                $contextMenu.find(".contentField").append(configureSubMenu(data));
            });

            //Add code snippets
            getCodeSnippets(function() {
                var $codeSnippets = $element("ul");
                for (var i = 0; i < codeSnippets.length; i++) {
                    var snippet = codeSnippets[i].snippet;
                    var name = codeSnippets[i].name;
                    $codeSnippets.append(
                        $element("li").append(
                            $element("div").text(name)
                        ).on("click", { snippet: snippet }, insertSnippet)
                    );
                }
                $contextMenu.find(".codeSnippet").append($codeSnippets);
            });
        }
    }

    self.contentTypeId.subscribe(function (newValue) {
        if (newValue === undefined) {
            return;
        }
        var isSystemType = false;
        var contentTypes = self.contentTypes();
        var contentTypeId = self.contentTypeId();
        for (var i = 0; i < contentTypes.length; i++) {
            var contentType = contentTypes[i];
            if (contentType.contentTypeId() === contentTypeId) {
                isSystemType = contentType.isSystem();
                break;
            }
        }

        self.canSelectGlobal(self.parentViewModel.isSystemUser && self.isAddMode() && isSystemType);
        self.isSystem(false);

        configureContextMenu();
    });

    self.contentType = ko.computed(function () {
        var value = "";
        if (self.contentTypes !== undefined) {
            var entity = util.getEntity(
                self.contentTypes(),
                function (contentType) {
                    return (self.contentTypeId() === contentType.contentTypeId());
                });
            if (entity != null) {
                value = entity.name;
            }
        }
        return value;
    });

    var validate = function (data) {
        var jsObject = ko.toJS(data);
        if (invalidCharsRegEx.test(jsObject.filePath)) {
            return {
                isValid: false,
                validationErrorMessage: resx.invalidCharsMessage
            }; //File path contains invalid characters
        }
        if (!util.hasDefaultValue(self.rootViewModel.defaultLanguage, self.localizedNames())) {
            return {
                isValid: false,
                validationErrorMessage: resx.invalidTemplateMessage
            };
        }
        return {
            isValid: true,
            validationErrorMessage: ''
        };
    };
    
    self.bindCodeEditor = function () {
        configureContextMenu();
        codeEditor.setValue(self.content());

        var target = document.querySelector('#templates-editView');
        var refreshEditor = function () {
            // ReSharper disable once UseOfImplicitGlobalInFunctionScope
            if (jQuery(target).css('display') !== 'none') {
                codeEditor.refresh();
            };
        };
        if (window.MutationObserver || window.WebKitMutationObserver || window.MozMutationObserver) {
            var observer = new MutationObserver(refreshEditor);
            observer.observe(target, { attributes: true, attributeFilter: ["style"] });
        }
        else {
            util.onVisible($rootElement.find(".CodeMirror"), 250, refreshEditor);
        }
    };

    self.cancel = function () {
        self.rootViewModel.closeEdit();
    };

    self.deleteTemplate = function (data) {
        util.confirm(resx.deleteTemplateConfirmMessage, resx.yes, resx.no, function () {
            var params = {
                templateId: data.templateId(),
                name: data.name(),
                contentTypeId: data.contentTypeId(),
                isSystem: data.isSystem(),
                filePath: data.filePath(),
                content: codeEditor.getValue(),
                isEditTemplate: data.isEditTemplate()
        };

            util.templateService().post("DeleteTemplate", params,
                function () {
                    //Success
                    parentViewModel.refresh();
                },

                function (xhr, status, err) {
                    //Failure
                    util.alert(status + ":" + err, resx.ok);
                }
                );
        });
    };

    self.init = function () {
        self.canEdit(true);
        self.templateId(-1);
        self.contentTypeId(-1);
        self.filePath('');
        self.isSystem(false);
        self.isEditTemplate(false);
        self.content('');

        util.initializeLocalizedValues(self.localizedNames, self.rootViewModel.languages());
    };

    self.load = function (data) {
        self.canEdit(data.canEdit);
        self.templateId(data.templateId);
        self.contentTypeId(data.contentTypeId);
        self.isSystem(data.isSystem);
        self.filePath(data.filePath);
        self.isEditTemplate(data.isEditTemplate);
        self.content(data.content);

        util.loadLocalizedValues(self.localizedNames, data.localizedNames);
    };

    self.saveTemplate = function (data) {
        var validationData = validate(data);
        if (!validationData.isValid) {
            util.alert(validationData.validationErrorMessage, resx.ok);

        }
        else {
            var jsObject = ko.toJS(data);
            var params = {
                templateId: jsObject.templateId,
                localizedNames: jsObject.localizedNames,
                contentTypeId: jsObject.contentTypeId,
                isSystem: jsObject.isSystem,
                filePath: jsObject.filePath,
                content: codeEditor.getValue(),
                isEditTemplate: jsObject.isEditTemplate
            };

            util.templateService().post("SaveTemplate", params,
                function () {
                    self.cancel();
                },
                function (xhr, status, err) {
                    //Failure
                    util.handleServiceError(xhr, status, err);
                }
            );
        }
    };

    self.toggleSelected = function () {
        self.selected(!self.selected());
    };

    var $codeEditor = $rootElement.find(".CodeMirror");
    $codeEditor.bind("contextmenu", function (event) {
        event.preventDefault();

        var cursorLocation = codeEditor.cursorCoords();

        $contextMenu.show();
        $contextMenu.offset({ top: cursorLocation.top, left: cursorLocation.left });

        return false;
    });

    codeEditor.on("mousedown", function () {
        $contextMenu.hide();
    });
}