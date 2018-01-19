module.exports = ['$scope', 'EventLogDataService',
    function ($scope, EventLogDataService) {

        // Defaults.
        var options = {
            pageIndex: 0,
            pageSize: 10,
            eventType: undefined,
            severity: undefined
        };

        // Initialise scope variables.
        $scope.currentPage = 1;
        $scope.pageCount = 1;
        $scope.eventLogs = [];

        // Add functions to scope.
        $scope.hasNextPage = hasNextPage;
        $scope.hasPrevPage = hasPrevPage;
        $scope.nextPage = nextPage;
        $scope.prevPage = prevPage;
        $scope.loadPage = loadPage;
        $scope.getPages = getPages;

        // Fetch first page.
        fetchEvents(options);

        // Fetch events.
        function fetchEvents(opts) {

            // Load events based on options.
            EventLogDataService.browseEvents(opts).then(
                function (response) {

                    // Place the events on the scope.
                    $scope.eventLogs = response.Data;

                    // Place page count on scope.
                    $scope.pageCount = response.Pagination.Pages;

                });
        }

        // Is there a next page?
        function hasNextPage() {
            return $scope.currentPage < $scope.pageCount;
        }

        // Is there a previous page?
        function hasPrevPage() {
            return $scope.currentPage > 1;
        }

        // Load the next page.
        function nextPage() {

            // Is there a next page?
            if (hasNextPage()) {

                // Yes, increment and fetch events.
                $scope.currentPage++;

                options.pageIndex = $scope.currentPage - 1;

                fetchEvents(options);
            }
        }

        // Load the next page.
        function prevPage() {

            // Is there a previous page?
            if (hasPrevPage()) {

                // Yes, decrement and fetch events.
                $scope.currentPage--;

                options.pageIndex = $scope.currentPage - 1;

                fetchEvents(options);
            }
        }

        // Load specified page.
        function loadPage(pageNo) {

            // Page exists?
            if (pageNo > 0 && pageNo <= $scope.pageCount) {

                $scope.currentPage = pageNo;

                options.pageIndex = pageNo - 1;

                fetchEvents(options);
            }
        }

        // Get array for pagination.
        function getPages(number) {
            return new Array(number);
        }

    }];
