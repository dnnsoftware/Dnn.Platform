(function () {

    // Get module.
    var module = angular.module('cantarus.poly-deploy');

    // Create service.
    module.factory('SessionService', ['DataService',
        function (DataService) {

            // Session storage.
            var currentSession;

            // Get a session when the service is first used.
            getSession();
            
            // Gets a new session from the API.
            function getSession() {

                // Get a session from the API.
                DataService.session.create().then(
                    function (data) {

                        // Store it.
                        currentSession = data;

                        console.log(currentSession);
                    });
            }

            return {
                session: currentSession
            };

        }]);

})();
