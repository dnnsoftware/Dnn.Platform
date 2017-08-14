(function () {

    // Get module.
    var module = angular.module('cantarus.poly-deploy');

    // Create service.
    module.factory('SessionDataService', ['$http', 'apiUrl',
        function ($http, apiUrl) {

            var controllerUrl = apiUrl + 'Session/';

            // POST
            // Create a new session.
            function create() {

                // Make request.
                return $http.post(controllerUrl + 'Create').then(
                    function (response) {

                        // Return unpacked data.
                        return response.data;
                    });
            }

            // GET
            // Get session by guid.
            function get(guid) {

                // Make request.
                return $http.get(controllerUrl + 'Get?guid=' + guid).then(
                    function (response) {

                        // Return unpacked data.
                        return response.data;
                    });
            }

            return {
                create: create
            };

        }]);

})();
