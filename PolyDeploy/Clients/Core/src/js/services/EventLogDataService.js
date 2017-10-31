module.exports = ['$http', 'apiUrl',
    function ($http, apiUrl) {

        var controllerUrl = apiUrl + 'EventLog/';

        // GET
        // Browse Events.
        function browseEvents(pageIndex, pageSize, eventType, severity) {

            var queryParameters = {};

            if (pageIndex) {
                queryParameters['pageIndex'] = pageIndex;
            }

            if (pageSize) {
                queryParameters['pageSize'] = pageSize;
            }

            if (eventType) {
                queryParameters['eventType'] = eventType;
            }

            if (severity) {
                queryParameters['severity'] = severity;
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
        // Get event types..
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
