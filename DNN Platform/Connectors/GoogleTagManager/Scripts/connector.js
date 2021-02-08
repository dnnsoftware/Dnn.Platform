"use strict";
define(["jquery", "knockout", "templatePath/scripts/config", "templatePath/scripts/PersonaBarDialog"], function ($, ko, cf) {
    var helper,
        bindViewModel,
        rootFolder,
        utility,
        appId,
        appSecret,
        container;

    var wasDeactivated = false;

    var init = function (koObject, connectionHelper, pluginFolder, util) {

        helper = connectionHelper;
        bindViewModel = koObject;
        rootFolder = pluginFolder;
        utility = util;
        koObject.container = ko.observable("");

        if (typeof dnn === "undefined") {
            window.Sys = !window.Sys ? window.top.Sys : window.Sys;
            window.dnn = !window.dnn ? window.top.dnn : window.dnn;
            window.String = !window.String ? window.top.String : window.String;
            window.String.format = !window.String.format ? window.top.String.format : window.String.format;
        }

    };

    var onSave = function (conn) {

        // Convert boolean to string as the API requires a dictionary of string values
        conn.configurations[1].value(conn.configurations[1].value().toString());
    };

    var onSaveComplete = function (conn, id) {

        // Deactivation / delete handled through the save since the delete function as
        // of v9.2.2 only works with multiple connector support. This should be
        // considered a temporary workaround until core behaviour updated.

        if (wasDeactivated) {

            // Was this just deactivated?  clear the fields
            conn.onDelete(conn, null, null);
            wasDeactivated = false;
            utility.notify(utility.resx.Connectors.txt_Deleted, true);
        }
        else {

            conn.connected("true");

            if (bindViewModel.buttons().length === 1) {

                activateDeleteButton(conn);
            }
            utility.notify((utility.resx.Connectors || utility.resx.PersonaBar).txt_Saved, true);
        }
    };

    var activateDeleteButton = function (conn) {

        if (conn.buttons().length === 1) {

            conn.buttons.push({
                className: "secondarybtn",
                text: bindViewModel.resx.btnDelete,
                action: function (conn, e) {

                    // Set the isDeactivating flag to true to override the default save behaviour
                    // Temporary workaround until delete functionality on connectors is improved
                    conn.configurations[2].value("true");
                    wasDeactivated = true;
                    conn.save(conn, e, onSaveComplete.bind(this, conn, conn.id));
                }
            });

        }

    };

    var getActionButtons = function () {


        if (bindViewModel.connected()) {

            return [
                {
                    className: "primarybtn",
                    text: utility.resx.Connectors.btn_Save,
                    action: function (conn, e) {
                        conn.save(conn, e, onSaveComplete.bind(this, conn, conn.id));
                    }
                },

                {
                    className: "secondarybtn",
                    text: bindViewModel.resx.btnDelete,
                    action: function (conn, e) {

                        // Set the isDeactivating flag to true to override the default save behaviour
                        // Temporary workaround until delete functionality on connectors is improved
                        conn.configurations[2].value("true");
                        wasDeactivated = true;
                        conn.save(conn, e, onSaveComplete.bind(this, conn, conn.id));
                    }
                }
            ];

        } else {

            return [
                {
                    className: "primarybtn",
                    text: utility.resx.Connectors.btn_Save,
                    action: function (conn, e) {
                        conn.save(conn, e, onSaveComplete.bind(this, conn, conn.id));
                    }
                }
            ];
        }
    };

    return {
        init: init,
        onSave: onSave,
        getActionButtons: getActionButtons
    };

});