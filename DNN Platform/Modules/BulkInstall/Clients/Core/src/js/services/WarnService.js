module.exports = ['SettingDataService',
    function (SettingDataService) {

        var warnings = {

            // IP Whitelist Disabled.
            whitelistDisabled: {
                key: 'WHITELIST_STATE',
                active: false,
                message: 'IP Whitelisting is currently disabled.',
                test: function () {
                    var self = this;

                    // Retrive state.
                    SettingDataService.whitelist.getState()
                        .then(function (result) {
                            self.active = !result;
                        });
                }
            }
        };

        // For each warning.
        angular.forEach(warnings, function (warning) {

            // Is there a test available?
            if (warning.test) {

                // Perform test.
                warning.test();

                // Is there a key?
                if (warning.key) {

                    // Subscribe for updates.
                    SettingDataService.subscribe(warning.key,
                        function () {
                            warning.test();
                        });
                }
            }
        });

        // Update the state of the passed warning key.
        function updateWarnState(warn, state) {

            // Key exists?
            if (warnings[warn]) {
                warnings[warn].state = state;
            }
        }

        return {
            warnings: warnings
        };
    }];
