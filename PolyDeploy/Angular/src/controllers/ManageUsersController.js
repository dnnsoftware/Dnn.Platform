(function () {

    // Get module.
    var module = angular.module('cantarus.poly-deploy');

    // Create controller.
    module.controller('ManageUsersController', ['$scope', 'DataService',
        function ($scope, DataService) {

            // Fetch API users.
            DataService.apiUser.getUsers().then(
                function (apiUsers) {

                    // Pop them on the scope.
                    $scope.users = apiUsers;

                });

        }]);

})();
