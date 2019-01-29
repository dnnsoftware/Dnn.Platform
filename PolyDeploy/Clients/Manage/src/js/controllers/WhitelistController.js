module.exports = ['$scope', 'IPSpecDataService', 'SettingDataService',
    function ($scope, IPSpecDataService, SettingDataService) {

        $scope.whitelistStates = [
            {
                name: 'Enabled',
                value: true
            },
            {
                name: 'Disabled',
                value: false
            }
        ];

        $scope.whitelistState = false;

        // Load specs.
        refreshSpecs();

        // Retrieve whitelist state.
        SettingDataService.whitelist.getState()
            .then(function (setting) {

                // Selected option.
                var selected = undefined;

                // Loop options.
                angular.forEach($scope.whitelistStates, function (state) {

                    // Is this the selected option?
                    if (state.value === setting) {
                        selected = state;
                    }
                });

                // Set on scope.
                $scope.whitelistState = selected;
            });

        // Update whitelist state.
        $scope.updateWhitelistState = function (state) {

            // Save value.
            SettingDataService.whitelist.setState(state.value);
        };

        // Create spec.
        $scope.createSpec = function (ipAddress) {

            // Create the new spec and append it.
            IPSpecDataService.createSpec(ipAddress).then(
                function (createdSpec) {

                    // Push on to specs.
                    $scope.specs.push(createdSpec);
                });
        };

        // Delete spec.
        $scope.deleteSpec = function (ipSpec) {

            // Delete the spec and then call for a refresh from the server.
            IPSpecDataService.deleteSpec(ipSpec).then(refreshSpecs);
        };

        // Fetch IP specs.
        function refreshSpecs() {
            IPSpecDataService.getSpecs().then(
                function (ipSpecs) {

                    // Pop them on the scope.
                    $scope.specs = ipSpecs;

                });
        }

    }];