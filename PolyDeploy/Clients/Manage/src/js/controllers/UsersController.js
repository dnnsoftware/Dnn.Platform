module.exports = ['$scope', 'APIUserDataService',
    function ($scope, APIUserDataService) {

        // Load users.
        refreshUsers();

        $scope.newUser = {
            name: '',
            bypassIPWhitelist: false
        };

        // Create user.
        $scope.createUser = function (newUser) {

            // Create the new user and append it, if you call for a refresh
            // the API key and encryption keys will be obfuscated.
            APIUserDataService.createUser(newUser.name, newUser.bypassIPWhitelist).then(
                function (createdUser) {

                    // Push on to users.
                    $scope.users.push(createdUser);
                });
        };

        // Delete user.
        $scope.deleteUser = function (apiUser) {

            // Delete the user and then call for a refresh from the server.
            APIUserDataService.deleteUser(apiUser).then(refreshUsers);
        };

        // Fetch API users.
        function refreshUsers() {
            APIUserDataService.getUsers().then(
                function (apiUsers) {

                    // Pop them on the scope.
                    $scope.users = apiUsers;

                });
        }

    }];
