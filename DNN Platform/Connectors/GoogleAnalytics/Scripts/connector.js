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

        if (typeof dnn === "undefined") {
            window.Sys = !window.Sys ? window.top.Sys : window.Sys;
            window.dnn = !window.dnn ? window.top.dnn : window.dnn;
            window.String = !window.String ? window.top.String : window.String;
            window.String.format = !window.String.format ? window.top.String.format : window.String.format;
        }


    }

    var onSave = function (conn) {
			

		// Convert boolean to string as the API requires a dictionary of string values
		conn.configurations[2].value(conn.configurations[2].value().toString());

    }

    var onSaveComplete = function (conn, id) {
		

		
		
    }
	
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
    }

    return {
		
        init: init,
        onSave: onSave,
        getActionButtons: getActionButtons
		
    }

});