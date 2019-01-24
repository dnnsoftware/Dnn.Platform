module.exports = ['$scope', 'EventLogDataService',
    function ($scope, EventLogDataService) {

        // Defaults.
        var options = {
            pageIndex: 0,
            pageSize: 20,
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
        function getPages(pageCount, currentPage) {

            // Check there are pages to paginate.
            if (pageCount < 1) {
                return [1];
            }

            // Number of selectable pages to show at any given time. This should
            // be an odd number so the current page can be centralised.
            var pageOptionCount = 9;

            // This is the number of selectable pages that appear on either side
            // of the current page. It's derived from the pageOptionCount.
            var bufferSize = (pageOptionCount - 1) / 2;

            // Initialise array.
            var numArray = [];

            // Work out what the bounds will be.
            var lowPage = 0;
            var highPage = 0;

            if (pageCount < pageOptionCount) {

                lowPage = 1;
                highPage = pageCount;

            } else {

                lowPage = currentPage - bufferSize;
                highPage = currentPage + bufferSize;

                // Gone too low?
                if (lowPage < 1) {

                    // How far?
                    highPage = highPage + Math.abs(lowPage) + 1;

                    lowPage = 1;
                }

                // Gone too high?
                if (highPage > pageCount) {

                    // How far?
                    lowPage = lowPage - Math.abs(highPage - pageCount);

                    highPage = pageCount;
                }
            }

            // Build array.
            for (var i = lowPage; i <= highPage; i++) {
                numArray.push(i);
            }

            // Clipping at the bottom?
            if (numArray[0] !== 1) {

                // Yes.
                numArray[0] = 1;
                numArray[1] = '...';
            }

            // Clipping at the top?
            if (numArray[numArray.length - 1] !== pageCount) {

                // Yes.
                numArray[numArray.length - 1] = pageCount;
                numArray[numArray.length - 2] = '...';
            }

            return numArray;
        }

    }];
