// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.


(function ($) {

    window.LocalizationController = function (settings, root) {
        var that = this;

        this.showMissingKeys = settings.showMissingKeys || false;

        this.servicesFramework = $.ServicesFramework(settings.moduleId);

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
            
            $.ajax({
                type: 'get',
                url: root.servicesFramework.getServiceRoot('CoreMessaging') + 'Subscriptions/GetLocalizationTable',
                beforeSend: root.servicesFramework.setModuleHeaders,
                data: params,
                cache: false,
                async: false
            }).done(success).fail(failure);

            return worked;
        };

        this.getString = function (key) {
            var value = that.stringTable[key];

            if ((value || '').length == 0) {
                if (that.showMissingKeys) {
                    return '[L]{0}'.replace("{0}", key);
                }

                return key;
            }

            return value;
        };
	    
		this.loadTable();
    };
})(window.jQuery);