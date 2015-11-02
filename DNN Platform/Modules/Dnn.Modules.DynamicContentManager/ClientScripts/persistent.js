var persistent = {
    init: function(cfg, sf) {
        var serviceFramework = sf;
        
        return {
            load: function() {
                return cfg.userSettings;
            },
            save: function (settings, callback) {
                if (!settings) return;

                serviceFramework.serviceController = "Settings";
                serviceFramework.post('Save', settings,
                    function successCallback () {
                        // Update settings in all locations
                        $.extend(window.__userSettings, settings);

                        $.extend(cfg.userSettings, settings);

                        if (typeof callback === 'function') {
                            callback();
                        }
                    }
                );
            }
        };
    }
};