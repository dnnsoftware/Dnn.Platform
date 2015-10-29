var persistent = {
    init: function(cfg, sf) {
        var defaultSettings = {
            pageSize: 10
        };
        console.log(cfg);

        var settings = null;
        var serviceFramework = sf;

        return {
            load: function() {
                if (!settings) settings = $.extend(defaultSettings, cfg.userSettings);
                return settings;
            },
            save: function (s, callback) {
                if (!s) return;
                settings = $.extend(defaultSettings, s);

                ////this doesn't exist right now.
                serviceFramework.controller = "UserSettings";
                serviceFramework.post('UpdateUserSettings', settings, function() {
                    // Update settings in all locations
                    $.extend(window.__userSettings, settings);

                    $.extend(cfg.userSettings, settings);

                    if (typeof callback === 'function') {
                        callback();
                    }
                });
            }
        };
    }
};