'use strict';
define(['jquery', 'knockout', 'main/pager', 'main/validator', 'main/config', 'main/PersonaBarDialog', 'jquery.easydropdown.min', 'dnn.jquery'],
    function ($, ko, pager, validator, cf, personaBarDialog) {
        var utility = null;
        var config = cf.init();
        var viewModel = {
            connections: ko.observableArray([])
        };

        ko.protectedObservable = function (initialValue) {
            var actualValue = ko.observable(initialValue),
                tempValue = ko.observable(initialValue);

            var result = tempValue;
            result.commit = function () {
                if (tempValue() !== actualValue()) {
                    actualValue(tempValue().trim());
                }
            };
            result.reset = function () {
                actualValue.valueHasMutated();
                tempValue(actualValue());
            };

            return result;
        };

        var wrapConnection = function (conn) {
            var koConfigurations = [];
            var secureSettingsValue = conn.configurations["SecureSettings"];
            var secureSettings = secureSettingsValue ? secureSettingsValue.split(";") : new Array();
            $.each(conn.configurations, function (key, value) {
                if (key === "SecureSettings") {
                    return;
                }
                var o = {};
                o.name = key;
                o.type = secureSettings.indexOf(key) > -1 ? "password" : "text";
                o.localizedName = key;
                o.value = ko.protectedObservable(value);
                koConfigurations.push(o);
            });

            var koConnectionObject = {
                name: conn.name,
                displayName: ko.observable(conn.displayName),
                previousDisplayName: ko.observable(conn.displayName),
                icon: conn.icon,
                supportsMultiple: conn.supportsMultiple,
                previousConnectionState: ko.observable(conn.connected),
                connected: ko.observable(conn.connected),
                id: conn.id,
                uniqueNumber: Math.random() * Date.now(),
                faviconCss: ko.computed(function () {
                    var css = 'socialnetwork-favicon ' + conn.name;
                    return css;
                }),
                initialObject: conn,
                onOpenEdit: onOpenEdit,
                onCancel: onCancel,
                onToggleEditeMode: onToggleEditeMode,
                onDelete: onDelete,
                open: ko.observable(false),
                inEdit: ko.observable(false),
                editMode: ko.observable(false),
                componentInitialized: ko.observable(false),
                editLayout: ko.observable(''),
                connector: ko.observable(),
                buttons: ko.observableArray([]),

                edit: function (item, e) {
                    e.preventDefault();
                    var btn = $(e.target);
                    var editRow = btn.parent().parent().next().find('td > div');
                    var showEditRow = function () {
                        $.each(viewModel.connections(), function (i, v) {
                            v.isOpen(false);
                            $.each(v.connections(), function (i, item) {
                                item.inEdit(false);
                                item.open(false);
                            });
                        });
                        item.inEdit(true);

                        editRow.slideDown(200, 'linear', function () {
                            $(this).find('input:first').select();
                        });
                    };
                    var collapsedEditRow = $('#connectionstbl tr.edit-row > td > div.edit-form:visible');
                    if (collapsedEditRow.length > 0) {
                        collapsedEditRow.slideUp(200, 'linear', showEditRow);
                    } else {
                        showEditRow();
                    }
                },

                cancel: function (item, e) {
                    if (e) {
                        e.preventDefault();
                    }
                    $.each(item.configurations, function (i, v) {
                        var name = v.name.toLowerCase();
                        v.value.reset();
                        if (item[name] && typeof item[name] === "object" && typeof item[name].reset === "function") {
                            item[name].reset();
                        }
                    });
                    var connected = item.previousConnectionState() && (item.supportsMultiple ? !!item.id : true);
                    item.connected(connected);
                    item.displayName(item.previousDisplayName());
                    item.inEdit(false);
                    viewModel.connections().forEach(function (v) {
                        v.connections().forEach(function (t) {
                            t.open(false);
                        });

                    });
                    if (e) {
                        $(e.target).parents('div.edit-form').slideUp(200, 'linear', function () {
                            viewModel.connections().forEach(function (v) {
                                if (v.connections().length === 1) {
                                    v.isOpen(false);
                                }
                            })
                        });
                    }
                },
                save: function (item, e, cb) {
                    e.preventDefault();
                    item.connected(false);
                    var container = $(e.target).parents('div.edit-form');
                    saveConnection(item, function (success, validated, id) {
                        utility.notify(utility.resx.Connectors.txt_Saved, true);
                        var connected = isConnectionConnected(item);
                        item.connected(connected);
                        if (success) {
                            item.previousConnectionState(connected);
                            item.previousDisplayName(item.displayName());
                            if (id) {
                                item.id = id;
                            }
                            $.each(item.configurations, function (i, v) {
                                v.value.commit();
                            });
                        }

                        if (typeof cb == "function") {
                            cb(item, true);
                        }
                    }, function (xhr, status) {
                        utility.notifyError(status || 'Failed...');
                        if (typeof cb == "function") {
                            cb(item, false);
                        }
                    });
                },
                configurations: koConfigurations
            };

            var pluginFolder = conn.pluginFolder;
            var pluginJs = 'rootPath' + pluginFolder + 'scripts/connector';
            var pluginCss = 'css!rootPath' + pluginFolder + 'connector.css';
            var pluginHtml = 'text!rootPath' + pluginFolder + 'connector.htm';
            require([pluginJs, pluginHtml, pluginCss], function (connector, layout) {
                loadLocalizedStrings(conn.name, function (data) {
                    for (var i = 0; i < koConfigurations.length; i++) {
                        if (typeof data[koConfigurations[i].name] != "undefined") {
                            koConfigurations[i].localizedName = data[koConfigurations[i].name];
                        }
                    }

                    koConnectionObject.resx = data;
                    koConnectionObject.connector(connector);
                    connector.init(koConnectionObject, connectionHelper, pluginFolder, utility);
                    if (!ko.components.isRegistered(koConnectionObject.uniqueNumber + "component")) {
                        ko.components.register(koConnectionObject.uniqueNumber + "component", { template: layout });
                        koConnectionObject.componentInitialized(true);

                        (function (connection) {
                            setTimeout(function () {
                                var $root = $('#connector-' + connection.displayName().replace(/ /g, ''));
                                $root.find('input, select').each(function () {
                                    var $this = $(this);
                                    if (!$this.attr('id') || !$root.find('label[for="' + $this.attr('id') + '"]').length) {
                                        $this.attr('aria-label', 'Value');
                                    }
                                });

                                $root.find('a').each(function () {
                                    var $this = $(this);
                                    if (!$this.text()) {
                                        $this.attr('aria-label', 'Link');
                                    }
                                });
                            }, 100);
                        })(koConnectionObject);
                    }
                    // koConnectionObject.editLayout(layout);

                    //custom buttons
                    var buttons = getDefaultButtons();
                    if (typeof connector.getActionButtons == "function") {
                        buttons = connector.getActionButtons();
                    }
                    for (var i = 0; i < buttons.length; i++) {
                        koConnectionObject.buttons.push(buttons[i]);
                    }


                    $('#connector-' + koConnectionObject.displayName()).find('.edit-fields').children().each(function () {
                        ko.applyBindings(koConnectionObject, this);
                    });
                });
            });

            return koConnectionObject;
        };

        var isConnectionConnected = function (item) {
            if (item.name === "Mailchimp" || item.name === "GoogleTagManager" || item.name === "GoogleAnalytics") {
                return !!item.configurations[0].value();
            }
            if (item.name === "Zendesk") {
                return item.configurations[0].value() && item.configurations[1].value() && item.configurations[2].value();
            }
            return item.configurations.every(function (config) {
                return !!config.value() || config.name == "Id" || config.name == "Connected";
            });
        };

        var getDefaultButtons = function () {
            return [
                {
                    className: 'primarybtn',
                    text: viewModel.resx.btn_Save,
                    action: function (conn, e) {
                        conn.save(conn, e);
                    }
                }
            ];
        }

        var validateDisplayName = function (conn) {
            var displayName = conn.displayName();
            var noSpecialChar = /^[a-zA-Z0-9- ]*$/.test(displayName);
            var isNotEmpty = !!displayName.trim();
            return isNotEmpty && noSpecialChar;
        };

        var isDisplayNameUnique = function (conn) {
            var displayName = conn.displayName();
            var isUnique = viewModel.connections().every(function (v) {
                return v.connections().every(function (t) {
                    return !(t.displayName().toLowerCase() == displayName.toLowerCase() && t.uniqueNumber != conn.uniqueNumber);
                });
            });
            return isUnique;
        };

        var saveConnection = function (conn, success, forceSave) {
            viewModel.errorMessage('');

            if (conn.connector()) {
                conn.connector().onSave(conn);
            }
            if (conn.supportsMultiple) {
                var isDisplayNameValid = validateDisplayName(conn);
                var isUniqueName = isDisplayNameUnique(conn);
                if (!isDisplayNameValid) {
                    return utility.notifyError(viewModel.resx.txt_DisplayNameIsNotValid);
                }
                if (!isUniqueName) {
                    return utility.notifyError(viewModel.resx.txt_DisplayNameIsNotUnique);
                }
            }

            utility.sf.moduleRoot = 'personaBar';
            utility.sf.controller = 'Connectors';
            var postData = {
                name: conn.name,
                displayName: conn.displayName(),
                id: conn.id,
                configurations: []
            };

            //if fields not all empty or all filled, then disable button and not save
            var allEmpty = true;
            var allFilled = true;

            for (var i = 0; i < conn.configurations.length; i++) {
                var c = conn.configurations[i];
                var value = c.value();
                postData.configurations.push({
                    name: c.name,
                    value: value
                });

                if (value != '') {
                    allEmpty = false;
                } else {
                    allFilled = false;
                }
            }

            var primaryButtons = $('#connector-' + conn.displayName()).find('a.primarybtn');
            if (!allEmpty && !allFilled && !forceSave) {
                primaryButtons.addClass('disabledbtn');
                return;
            } else {
                primaryButtons.removeClass('disabledbtn');
            }

            viewModel.errorMessage('');
            primaryButtons.html(viewModel.resx.btn_Checking);
            if (postData.name === "UNC" && !postData.configurations[0].value) {
                return onDelete(conn);
            }
            if (conn.supportsMultiple && !postData.configurations[0].value && (!postData.configurations[1] || !postData.configurations[1].value)) {
                return onDelete(conn);
            }
            utility.sf.post('SaveConnection', postData, function (d) {
                primaryButtons.html(viewModel.resx.btn_Save);
                if (d) {
                    if (d.Success) {
                        success(d.Success, d.Validated, d.Id);
                    } else if (d.Message) {
                        utility.notifyError(d.Message || 'Failed...');
                    } else {
                        utility.notifyError('Failed...');
                    }
                } else {
                    utility.notifyError('Failed...');
                }
            }, onSaveFailed.bind(this, conn));
        };

        var onSaveFailed = function (conn, xhr, status) {
            var primaryButtons = $('#connector-' + conn.displayName()).find('a.primarybtn');
            primaryButtons.html(viewModel.resx.btn_Save);
            if (xhr.responseJSON) {
                utility.notifyError(xhr.responseJSON.Message || status || 'Failed...');
            } else {
                utility.notifyError(status || 'Failed...');
            }
        };

        var onAddNew = function (item, e) {
            item.isOpen(true);
            var connections = item.connections();
            var newConnector = connections.find(function (conn) { return !conn.id });

            if (newConnector && connections.length > 1) {
                closeAllSubConnectors();
                return newConnector.open(true);
            }
            var configurationKeys = Object.keys(connections[0].initialObject.configurations);
            var newConfigurations = {};
            configurationKeys.forEach(function (key) {
                if (key === "AuthPage") {
                    newConfigurations[key] = connections[0].initialObject.configurations[key];
                } else {
                    newConfigurations[key] = "";
                }
            });
            var displayName = e === true ? item.displayName : getNewName(item);
            closeAllSubConnectors();
            var newConnection = Object.assign(connections[0].initialObject, { id: null, configurations: newConfigurations, displayName: displayName, open: true });
            var newConn = wrapConnection(newConnection);
            newConn.connected(false);
            newConn.previousConnectionState(false);
            newConn.open(true);
            connections.push(newConn);
            item.connections(connections);
            return newConn;
        };

        var getNewName = function (item) {
            var connections = item.connections();
            var index = connections.length;
            var newName = item.displayName + " - " + index;
            while (connections.some(function (conn) { return conn.displayName() === newName })) {
                newName = item.displayName + " - " + ++index;
            }
            return newName;
        }

        var closeAllSubConnectors = function () {
            viewModel.connections().forEach(function (v) {
                v.connections().forEach(function (t) {
                    t.open(false);
                });
            });
        }

        var onOpen = function (item, e) {
            var isOpen = item.isOpen();
            viewModel.connections().forEach(function (v) {
                if (v.name !== item.name) {
                    v.isOpen(false);
                } else {
                    v.height(40 + v.connections().length * 53 + 'px');
                    v.isOpen(!isOpen);
                }
            });
            $.each(viewModel.connections(), function (i, v) {
                $.each(v.connections(), function (i, item) {
                    item.inEdit(false);
                });
            });
            var collapsedEditRow = $('#connectionstbl tr.edit-row > td > div.edit-form:visible');
            if (collapsedEditRow.length > 0) {
                collapsedEditRow.slideUp(200, 'linear');
            }
            viewModel.connections().forEach(function (v) {
                v.connections().forEach(function (t) {
                    t.open(false);
                });
            });
        };

        var onOpenEdit = function (item, e) {
            var open = !item.open();
            viewModel.connections().forEach(function (v) {
                v.connections().forEach(function (t) {
                    t.open(t.uniqueNumber === item.uniqueNumber ? open : false);
                });
            });
        };

        var onCancel = function (item, e) {
            item.onOpenEdit(item);
            item.cancel(item, e);
        }

        var onToggleEditeMode = function (item, e) {
            item.editMode(!item.editMode);
        }

        var onDelete = function (item, e) {
            if (!item.id) {
                return removeItem(item, e);
            }
            utility.sf.moduleRoot = 'personaBar';
            utility.sf.controller = 'Connectors';
            var postData = { id: item.id, name: item.name };
            utility.sf.post('DeleteConnection', postData, function (data) {
                utility.notify(utility.resx.Connectors.txt_Deleted, true);
                removeItem(item, e);
            });
        };

        var removeItem = function (item, e) {
            var connectionCategory = viewModel.connections().find(function (conn) {
                return conn.connections().some(function (test) {
                    return test.uniqueNumber == item.uniqueNumber;
                });
            });
            if (ko.components.isRegistered(item.uniqueNumber + "component")) {
                ko.components.unregister(item.uniqueNumber + "component");
            }
            var connections = connectionCategory.connections().filter(function (conn) {
                return conn.uniqueNumber != item.uniqueNumber;
            });
            if (!connections.length) {
                var newConn = onAddNew(connectionCategory, true);
                connections.push(newConn);
                connectionCategory.connections(connections);
                $('#connector-' + newConn.name.replace(/ /g, '')).slideUp(200, 'linear', function () {
                    viewModel.connections().forEach(function (v) {
                        if (v.connections().length === 1) {
                            v.isOpen(false);
                        }
                    })
                });
            }
            connectionCategory.connections(connections);
        };

        var categorizeConnections = function (conn) {
            var connectionNames = conn.map(function (connection) {
                return connection.name;
            });
            connectionNames = connectionNames.filter(function (name, index, names) {
                return names.indexOf(name) === index;
            });
            var connections = connectionNames.map(function (name) {
                var _connections = conn.filter(function (connection) {
                    return name === connection.name;
                });
                var connection = {
                    connections: ko.observable(_connections),
                    supportsMultiple: ko.observable(_connections[0].supportsMultiple),
                    name: _connections[0].name,
                    displayName: _connections[0].displayName(),
                    isOpen: ko.observable(false),
                    height: ko.observable(0),

                    icon: _connections[0].icon || "",
                    onOpen: onOpen,
                    onAddNew: onAddNew
                };
                connection.connected = ko.computed(isConnected.bind(this, connection));
                return connection;
            });
            return connections;
        };

        var isConnected = function (connection) {
            var connections = connection.connections();
            return connections.some(function (connection) {
                return connection.connected();
            });
        };

        var loadConnections = function (cb) {
            utility.sf.moduleRoot = "personaBar";
            utility.sf.controller = 'Connectors';
            utility.sf.get('GetConnections', null, function (data) {
                var conn = [];
                $.each(data, function (i, v) {
                    conn.push(wrapConnection(v));
                });
                viewModel.connections(categorizeConnections(conn));
            }, function (xhr, status) {
                utility.notifyError(status || 'Failed...');
            });

            if (typeof cb === 'function') cb();
        };

        var connectionHelper = {
            loadConnections: loadConnections,
            saveConnection: saveConnection
        }

        var localStorageAllowed = function () {
            var mod = 'DNN_localStorageTEST';
            try {
                window.localStorage.setItem(mod, mod);
                window.localStorage.removeItem(mod);
                return true;
            } catch (e) {
                return false;
            }
        };

        var loadLocalizedStrings = function (name, cb) {
            var allowLocalStorage = localStorageAllowed();
            var storageName = 'Connections.' + name + '.' + config.culture + '.Table';

            if (allowLocalStorage) {
                if (window.localStorage[storageName]) {
                    var table = JSON.parse(window.localStorage[storageName]);
                    var time = new Date().getTime() - table.timestamp;
                    if (time > 60 * 60 * 1000) { //expire the cache after 1 hour
                        window.localStorage.removeItem(storageName);
                    } else {
                        cb(table);
                        return;
                    }
                }
            }

            utility.sf.moduleRoot = 'personaBar';
            utility.sf.controller = 'Connectors';
            utility.sf.get('GetConnectionLocalizedString', { name: name, culture: config.culture }, function (data) {
                if (allowLocalStorage) {
                    try {
                        data.timestamp = new Date().getTime();
                        window.localStorage[storageName] = JSON.stringify(data);
                    } catch (ex) {
                        if (ex.name === "QuotaExceededError") {
                            console.log("Local Storage might be full.");
                        }
                        console.log(ex);
                        if (window.localStorage[storageName]) {
                            window.localStorage.removeItem(storageName);
                        }
                    }
                }
                cb(data);
            });
        };

        var copyUtility = function (util) {
            var shadow = {};
            for (var prop in util) {
                if (prop != 'notify') {
                    shadow[prop] = util[prop];
                }
            }
            shadow['notify'] = notifyMessage;
            shadow['popupNotify'] = util.notify;
            return shadow;
        };

        var notifyMessage = function (text, success) {
            if (success) {
                viewModel.errorMessage('');
                utility.popupNotify(text, { type: success.type });
            } else {
                viewModel.errorMessage(text);
            }
        };

        return {
            init: function (wrapper, util, params, callback) {
                utility = copyUtility(util);
                utility.localizeErrMessages(validator);
                utility.asyncParallel([
                    function (cb1) {
                        loadConnections(cb1);
                    }
                ],
                    function () {
                        var container = wrapper[0];
                        viewModel.resx = utility.resx.Connectors;
                        viewModel.errorMessage = ko.observable('');
                        ko.applyBindings(viewModel, container);
                        if (typeof callback === 'function') callback();
                    });

            },

            initMobile: function (wrapper, util, params, callback) {
                this.init(wrapper, util, params, callback);
            },

            load: function (params, callback) {
                if (typeof callback === 'function') callback();
            },

            loadMobile: function (params, callback) {
                this.load(params, callback);
            }
        };
    });