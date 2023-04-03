module.exports = ['$scope', 'IPSpecDataService', 'SettingDataService',
    function ($scope, IPSpecDataService, SettingDataService) {

        $scope.newIp = {
            name: '',
            ipv4Address: ''
        };

        $scope.errorMessage = null;

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

        // Dismiss error message.
        $scope.dismissError = function () {

            $scope.errorMessage = null;
        };

        // Create spec.
        $scope.createSpec = function (ipSpec) {

            // Create the new spec and append it.
            IPSpecDataService.createSpec(ipSpec.name, ipSpec.ipv4Address).then(
                function (resp) {

                    if (resp.err) {
                        $scope.errorMessage = resp.err;
                        return;
                    }

                    // Push on to specs.
                    $scope.specs.push(resp.ipSpec);
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