// DotNetNuke® - http://www.dotnetnuke.com
//
// Copyright (c) 2002-2013 DotNetNuke Corporation
// All rights reserved.

(function (dnn) {
    'use strict';

    if (typeof dnn === 'undefined') dnn = {};
    if (typeof dnn.social === 'undefined') dnn.social = {};

    dnn.social.LocalizationController = function LocalizationController ($, settings, social) {
        var that = this;

        this.showMissingKeys = settings.showMissingKeys || false;

        this.service = social.getService('Social');

        // Actual table of localized strings and their values.    
        this.stringTable = {};

        // Load a localization string table for the specified view.
        this.loadTable = function () {
            var worked = false;

            var success = function (m) {
                $.each(m.Table,
                    function (index, entry) {
                        if (that.stringTable.hasOwnProperty(index) == false) {
                            that.stringTable[index] = entry;
                        }
                    });

                worked = true;
            };

            var failure = function (m) {
                console.log('Unable to load string translations: ' + (m.ErrorMessage || 'Unknown error'));
            };

            // This is a synchronous call instead of async so that we can guarantee the translations are loaded
            var params = {
                culture: settings.culture
            };

            that.service.getsync('GetLocalizationTable', params, success, failure);

            return worked;
        };

        this.getString = function (key) {
            var value = that.stringTable[key];

            if ((value || '').length == 0) {
                if (that.showMissingKeys) {
                    return '[L]{0}'.format(key);
                }

                return key;
            }

            return value;
        };

        this.loadTable();
    };
})(window.dnn);