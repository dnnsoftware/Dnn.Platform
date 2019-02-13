module.exports = ['$interval', 'SessionDataService',
    function ($interval, SessionDataService) {

        // Session storage.
        var sessionObject = {};
        var sessionPromise;
        var sessionRefreshInterval;

        // Get a session when the service is first used.
        newSession();

        // Gets a new session from the API.
        function newSession() {

            // Is there an interval promise?
            if (sessionRefreshInterval) {

                // Cancel it.
                $interval.cancel(sessionRefreshInterval);
            }

            // Get a session from the API.
            sessionPromise = SessionDataService.create().then(
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
                    return SessionDataService.get(session.Guid).then(
                        function (sessionUpdate) {

                            // Do we need to stop refreshing?
                            if (sessionUpdate
                                && sessionUpdate.Status
                                && sessionUpdate.Status === 2) {

                                // Complete, no need to continue refreshing.
                                $interval.cancel(sessionRefreshInterval);
                            }

                            // Replace current session data.
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
                    return SessionDataService.summary(session.Guid);
                });
        }

        // Start install.
        function install() {

            // Only when we have a session.
            return sessionPromise.then(
                function (session) {

                    // Perform install.
                    SessionDataService.install(session.Guid);

                    // Start refresh interval.
                    sessionRefreshInterval = $interval(refreshSession, 1000);
                });
        }

        // Replace.
        function replace(dest, source) {

            // Delete all properties in destination.
            for (var prop in dest) {
                delete dest[prop];
            }

            // Copy properties from source to destination.
            for (var prop in source) {
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

    }];