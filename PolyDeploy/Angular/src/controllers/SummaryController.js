(function () {

    // Get module.
    var module = angular.module('cantarus.poly-deploy');

    // Create controller.
    module.controller('SummaryController', ['$scope', 'SessionService',
        function ($scope, SessionService) {

            SessionService.summary().then(function (summaryData) {
                console.log(summaryData);
                $scope.summaryData = summaryData;
            });

        }]);

})();
