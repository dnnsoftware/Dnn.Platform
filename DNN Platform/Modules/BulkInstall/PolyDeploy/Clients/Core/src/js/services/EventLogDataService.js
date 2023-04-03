module.exports = ['$http', 'apiUrl',
    function ($http, apiUrl) {

        var controllerUrl = apiUrl + 'EventLog/';

        // GET
        // Browse Events.
        function browseEvents(options) {

            var queryParameters = {};

            if (options) {
                if (options.pageIndex) {
                    queryParameters['pageIndex'] = options.pageIndex;
                }

                if (options.pageSize) {
                    queryParameters['pageSize'] = options.pageSize;
                }

                if (options.eventType) {
                    queryParameters['eventType'] = options.eventType;
                }

                if (options.severity) {
                    queryParameters['severity'] = options.severity;
                }
            }

            var queryString = buildQueryString(queryParameters);

            // Make request.
            return $http.get(controllerUrl + 'Browse' + queryString).then(
                function (response) {

                    // Return unpacked data.
                    return response.data;
                });
        }

        // GET
        // Get event types.
        function getEventTypes() {

            // Make request.
            return $http.get(controllerUrl + 'EventTypes').then(
                function (response) {

                    // Return unpacked data.
                    return response.data;
                });
        }

        // Builds a query string from the passed object.
        function buildQueryString(queryParams) {

            var queryString = '';

            // Loop properties.
            for (var paramName in queryParams) {

                // Is it the first parameter?
                if (queryString.length === 0) {

                    // Yes, use '?'.
                    queryString = queryString + '?';
                } else {

                    // No, use '&'.
                    queryString = queryString + '&';
                }

                // Add parameter.
                queryString = queryString + paramName + '=' + queryParams[paramName];
            }

            return queryString;
        }

        return {
            browseEvents: browseEvents,
            getEventTypes: getEventTypes
        };

    }];
