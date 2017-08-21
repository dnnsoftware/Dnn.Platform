(function () {

    // Get module.
    var module = angular.module('cantarus.poly-deploy');

    // Create service.
    module.factory('SessionService', ['$interval', 'DataService',
        function ($interval, DataService) {

            // Session storage.
            var sessionObject = {
                nope: 'what?'
            };
            var sessionPromise;
            var sessionRefreshInterval;

            // Get a session when the service is first used.
            newSession();
            
            // Gets a new session from the API.
            function newSession() {

                // Get a session from the API.
                sessionPromise = DataService.session.create().then(
                    function (session) {

                        // Replace currently stored session.
                        replace(sessionObject, session);

                        return sessionObject;
                    });

                return sessionPromise;
            }

            // Refresh session from server.
            function refreshSession() {

                // Use session promise to ensure we have a session object.
                return sessionPromise.then(
                    function (session) {

                        // Return promise from data service.
                        return DataService.session.get(session.Guid).then(
                            function (sessionUpdate) {

                                replace(sessionObject, sessionUpdate);

                                return sessionObject;
                            });
                    });
            }

            // Get session summary.
            function summary() {

                // Get the session summary from the API.
                return sessionPromise.then(
                    function (session) {

                        // Get session summary.
                        console.log('GetSummary: ' + session.Guid);
                        return DataService.session.summary(session.Guid);
                    });
            }

            // Start install.
            function install() {

                // Only when we have a session.
                return sessionPromise.then(
                    function (session) {

                        // Perform install.
                        DataService.session.install(session.Guid);

                        // Start refresh interval.
                        sessionRefreshInterval = $interval(refreshSession, 1000);
                    });
            }

            // Replace.
            function replace(dest, source) {

                // Delete all properties in destination.
                for (prop in dest) {
                    delete dest[prop];
                }

                // Copy properties from source to destination.
                for (prop in source) {
                    dest[prop] = source[prop];
                }
            }

            return {
                session: sessionObject,
                sessionPromise: sessionPromise,
                newSession: newSession,
                summary: summary,
                install: install
            };

        }]);

})();
