module.exports = ['$scope', 'IPSpecDataService',
    function ($scope, IPSpecDataService) {

        // Load specs.
        refreshSpecs();

        $scope.newIp = {
            name: '',
            ipv4Address: ''
        };

        $scope.errorMessage = null;

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