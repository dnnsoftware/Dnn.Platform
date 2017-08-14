(function () {

    // Get module.
    var module = angular.module('cantarus.poly-deploy');

    // Create service.
    module.factory('DataService', ['SessionDataService',
        function (SessionDataService) {

            return {
                session: SessionDataService
            };

        }]);

})();
