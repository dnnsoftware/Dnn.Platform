(function () {

    // Get module.
    var module = angular.module('cantarus.poly-deploy');

    // Create controller.
    module.controller('ManageUsersController', ['$scope', 'DataService',
        function ($scope, DataService) {

            // Load users.
            refreshUsers();

            // Create user.
            $scope.createUser = function (name) {

                // Create the new user and append it, if you call for a refresh
                // the API key and encryption keys will be obfuscated.
                DataService.apiUser.createUser(name).then(
                    function (createdUser) {

                        // Push on to users.
                        $scope.users.push(createdUser);
                    });
            };

            // Delete user.
            $scope.deleteUser = function (apiUser) {

                // Delete the user and then call for a refresh from the server.
                DataService.apiUser.deleteUser(apiUser).then(refreshUsers);
            };

            // Fetch API users.
            function refreshUsers() {
                DataService.apiUser.getUsers().then(
                    function (apiUsers) {

                        // Pop them on the scope.
                        $scope.users = apiUsers;

                    });
            }

        }]);

})();
