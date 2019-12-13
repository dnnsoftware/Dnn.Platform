// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

"use strict";
define(["jquery", "knockout", "templatePath/scripts/config", "templatePath/scripts/PersonaBarDialog"], function ($, ko, cf) {
    var helper,
        bindViewModel,
        rootFolder,
        utility,
        appId,
        appSecret,
        container;

    var init = function (koObject, connectionHelper, pluginFolder, util) {
        helper = connectionHelper;
        bindViewModel = koObject;
        rootFolder = pluginFolder;
        utility = util;
        koObject.container = ko.observable("");
        koObject.loadingFolders = ko.observable(false);
        koObject.foldersData = ko.observable([]);

        if (typeof dnn === "undefined") {
            window.Sys = !window.Sys ? window.top.Sys : window.Sys;
            window.dnn = !window.dnn ? window.top.dnn : window.dnn;
            window.String = !window.String ? window.top.String : window.String;
            window.String.format = !window.String.format ? window.top.String.format : window.String.format;
        }
        processData(koObject);

    };

    var fieldsHaveValues = function (conn) {
        return conn.configurations[0].value() !== "" &&
            conn.configurations[1].value() !== "";
    };

    var onSave = function (conn) {
        if (typeof conn.container !== "undefined") {
            conn.configurations.push({
                name: "Container",
                value: ko.protectedObservable(conn.container())
            });
        }
    };

    var processData = function (conn, id) {
        conn.id = id || conn.id;
        for (var i = conn.configurations.length - 1; i >= 0; i--) {
            var config = conn.configurations[i];
            if (config.name !== "AccountName" && config.name !== "AccountKey" && config.name != "Id") {
                switch (config.name.toLowerCase()) {
                    case "connected":
                        conn.connected(config.value() === "true");
                        conn.configurations.splice(i, 1);
                        break;
                    case "container":
                        conn.container(conn.container() || config.value());
                        conn.configurations.splice(i, 1);
                        break;
                }
            } else if (config.name === "AccountName") {
                appId = config.value();
            } else if (config.name === "AccountKey") {
                appSecret = config.value();
            } else if (config.name === "Id") {
                id = conn.id;
            }
        }

        conn.connected(!(typeof conn.container() === "undefined" || conn.container() === ""));
        if (conn.id && conn.foldersData().length === 0)
            loadFoldersData(conn);
    };

    var loadFoldersData = function (conn) {
        utility.sf.moduleRoot = "AzureConnector";
        utility.sf.controller = "Services";
        utility.sf.get("GetAllContainers?id=" + conn.id, {}, foldersLoaded.bind(this, conn), folderLoadFailed.bind(this, conn));
    };

    var foldersLoaded = function (conn, data) {
        conn.loadingFolders(false);
        var foldersData = [conn.resx.SelectContainer];
        for (var i = 0; i < data.length; i++) {
            foldersData.push(data[i]);
        }
        conn.foldersData(foldersData);
        if (typeof conn.container() === "undefined" || conn.container() === "") {
            conn.container("/");
        }

        $('#connector-Azure').find('select').easyDropDown({ wrapperClass: 'pb-dropdown', cutOff: 10, inFocus: true });
    };

    var folderLoadFailed = function (conn, xhr) {
        conn.loadingFolders(false);
        var message = xhr.responseJSON.Message;
        if (message.indexOf("{") === 0) {
            message = JSON.parse(message)["error"];
        }
        utility.notifyError(message);
    };

    var onSaveComplete = function (conn, id) {
        if (!fieldsHaveValues(conn)) {
            conn.foldersData([]);
            return;
        }
        processData(conn, id);
        utility.notify((utility.resx.Connectors || utility.resx.PersonaBar).txt_Saved, true);
    };


    var authenticate = function (conn, e) {
        e.preventDefault();
        $.each(conn.configurations, function (i, v) {
            v.value.commit();
        });
        saveConnection(conn, onSaveComplete.bind(this, conn));
    };

    //copy the save connection method to save sync.
    var onSaveConnection = false;
    var saveConnection = function (conn, success) {
        if (onSaveConnection) return;
        onSaveConnection = true;
        if (conn.connector()) {
            conn.connector().onSave(conn);
        }

        utility.sf.moduleRoot = "personaBar";
        utility.sf.controller = "Connectors";
        var postData = {
            name: conn.name,
            displayName: conn.displayName(),
            id: conn.id,
            configurations: []
        };

        for (var i = 0; i < conn.configurations.length; i++) {
            var c = conn.configurations[i];
            var value = c.value();
            postData.configurations.push({
                name: c.name,
                value: c.value()
            });

        }

        if (!postData.configurations[0].value && !postData.configurations[1].value) {
            return conn.onDelete(conn);
        }

        utility.sf.call("POST", "SaveConnection", postData, function (d) {
            if (d && d.Success) {
                success(d.Id);
            } else {
                var message = d.responseJSON.Message;
                utility.notifyError(message || "Failed...");
            }
            onSaveConnection = false;
        }, saveFailed.bind(this, conn), null, null, true, false);
    };

    var saveFailed = function (conn, d) {
        var message = d.responseJSON.Message;
        utility.notifyError(message || "Failed...");
        onSaveConnection = false;
        conn.cancel();
    };

    var getActionButtons = function () {
        return [
            {
                className: "primarybtn",
                text: "Save",
                action: function (conn, e) {
                    conn.save(conn, e, onSaveComplete.bind(this, conn, conn.id));
                }
            }
        ];
    };

    return {
        init: init,
        onSave: onSave,
        getActionButtons: getActionButtons
    };

});