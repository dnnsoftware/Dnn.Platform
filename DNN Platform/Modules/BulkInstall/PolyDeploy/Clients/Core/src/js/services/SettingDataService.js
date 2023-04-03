module.exports = ['$http', 'apiUrl',
    function ($http, apiUrl) {

        var controllerUrl = apiUrl + 'Setting/';

        var subscribers = {};

        // GET
        // Get a Setting.
        function getSetting(group, key) {

            // Make request.
            return $http.get(controllerUrl + `Get?group=${group}&key=${key}`).then(
                function (response) {

                    // Return unpacked data.
                    return response.data;
                });
        }

        // POST
        // Update or create a Setting.
        function setSetting(group, key, value) {

            // Make request.
            return $http.post(controllerUrl + `Set?group=${group}&key=${key}&value=${value}`).then(
                function (response) {

                    // Return unpacked data.
                    return response.data;
                });
        }

        function getWhitelistState() {

            var group = 'WHITELIST',
                setting = 'STATE';

            return getSetting(group, setting)
                .then(function (result) {
                    return result.Value.toLowerCase() === 'true';
                });
        }

        function setWhitelistState(value) {

            var group = 'WHITELIST',
                setting = 'STATE';

            return setSetting(group, setting, value)
                .then(function () {

                    // Notify subscribers of change.
                    notify(`${group}_${setting}`);
                });
        }

        // Allow other services to subscribe to changes to particular settings.
        function subscribe(key, callback) {

            // Do we have already have a subscribers array with this key?
            if (!subscribers[key]) {

                // No, create it.
                subscribers[key] = [];
            }

            // Add subscriber.
            subscribers[key].push(callback);
        }

        // Notify subscribers.
        function notify(key, value) {

            // Any subscribers?
            if (subscribers[key] && subscribers[key].length > 0) {

                // Notify each.
                angular.forEach(subscribers[key],
                    function (subscriber) {
                        subscriber(value);
                    });
            }
        }

        return {
            subscribe: subscribe,
            whitelist: {
                getState: getWhitelistState,
                setState: setWhitelistState
            }
        };

    }];