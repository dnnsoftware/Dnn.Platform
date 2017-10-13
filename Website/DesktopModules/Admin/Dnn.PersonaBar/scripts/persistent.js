'use strict';
define(['jquery'], function ($) {
    return {
        init: function (cfg, sf) {
            var defaultSettings = {
                period: 'Week',
                comparativeTerm: '1 w',
                startDate: '',
                endDate: '',
                expandPersonaBar: true,
                expandTasksPane: true,
                legends: []
            };

            var settings = null;
            var serviceFramework = sf.init(cfg.siteRoot, cfg.tabId, cfg.antiForgeryToken);
            var inIframe = window !== window.top;

            return {
                load: function () {
                    if (!settings) settings = $.extend(defaultSettings, cfg.userSettings);
                    return settings;
                },
                save: function (s, callback) {
                    if (!s) return;
                    settings = $.extend(defaultSettings, s);

                    serviceFramework.controller = "UserSettings";
                    serviceFramework.postsilence('UpdateUserSettings', settings, function () {
                        // Update settings in all locations
                        if (inIframe) {
                            if (window.parent['personaBarSettings']['personaBarUserSettings']) {
                                $.extend(window.parent['personaBarSettings']['personaBarUserSettings'], settings);
                            }
                        } else {
                            if (window['personaBarSettings']['personaBarUserSettings']) {
                                $.extend(window['personaBarSettings']['personaBarUserSettings'], settings);
                            }
                        }

                        $.extend(cfg.userSettings, settings);

                        if (typeof callback === 'function') {
                            callback();
                        }
                    });
                }
            };
        }
    };
});