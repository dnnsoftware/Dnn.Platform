module.exports = ['$scope', 'WarnService',
    function ($scope, WarnService) {

        $scope.warnings = WarnService.warnings;

    }];
