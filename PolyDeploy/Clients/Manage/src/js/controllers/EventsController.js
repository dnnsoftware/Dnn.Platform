module.exports = ['$scope', 'EventLogDataService',
    function ($scope, EventLogDataService) {

        // Load events.
        loadEvents();

        // Fetch events.
        function loadEvents() {

            // No pagination yet.
            EventLogDataService.browseEvents(0, 9999).then(
                function (eventLogs) {

                    // Pop them on the scope.
                    $scope.eventLogs = eventLogs;

                });
        }

    }];
