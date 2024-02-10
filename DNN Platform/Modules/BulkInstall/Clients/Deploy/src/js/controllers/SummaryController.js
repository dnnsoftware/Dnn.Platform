module.exports = ['$scope', 'SessionService',
    function ($scope, SessionService) {

        SessionService.summary().then(function (summaryData) {
            console.log(summaryData);
            $scope.summaryData = summaryData;
        });

        $scope.dependenciesString = function (deps) {

            var depString = '';

            angular.forEach(deps, function (dep) {

                if (depString.length !== 0) {
                    depString = depString + ', ';
                }

                depString = depString + dep;
            });

            return depString;
        };

    }];