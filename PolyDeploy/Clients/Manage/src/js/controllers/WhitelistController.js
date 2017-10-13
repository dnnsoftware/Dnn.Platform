module.exports = ['$scope', 'IPSpecDataService',
    function ($scope, IPSpecDataService) {

        // Load specs.
        refreshSpecs();

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