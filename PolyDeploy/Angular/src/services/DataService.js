(function () {

    // Get module.
    var module = angular.module('cantarus.poly-deploy');

    // Create service.
    module.factory('DataService', ['SessionDataService', 'APIUserDataService',
        function (SessionDataService, APIUserDataService) {

            // All this service really does is pipe each data service in so they're easy to access.

            return {
                session: SessionDataService,
                apiUser: APIUserDataService
            };

        }]);

})();
