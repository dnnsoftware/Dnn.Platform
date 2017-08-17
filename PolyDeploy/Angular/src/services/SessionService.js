(function () {

    // Get module.
    var module = angular.module('cantarus.poly-deploy');

    // Create service.
    module.factory('SessionService', ['DataService',
        function (DataService) {

            // Session storage.
            var sessionPromise;

            // Get a session when the service is first used.
            getSession();
            
            // Gets a new session from the API.
            function getSession() {

                // Get a session from the API.
                sessionPromise = DataService.session.create().then(
                    function (data) {

                        // Store it.
                        return data.Guid;
                    });
            }

            return {
                session: sessionPromise
            };

        }]);

})();
