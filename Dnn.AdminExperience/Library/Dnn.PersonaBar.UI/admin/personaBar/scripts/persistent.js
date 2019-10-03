'use strict';
define(['jquery'], function ($) {
    return {
        init: function (cfg, sf) {
            var defaultSettings = {
                expandPersonaBar: true
            };

            var settings = null;
            var serviceFramework = sf.init(cfg.siteRoot, cfg.tabId, cfg.antiForgeryToken);
            var inIframe = window !== window.top;

            return {
                load: function () {
                    if (!settings) settings = $.extend(defaultSettings, cfg.userSettings);
                    return settings;
                },
                save: function (s, success, error) {
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

                        if (typeof success === 'function') {
                            success();
                        }
                    }, error);
                }
            };
        }
    };
});
