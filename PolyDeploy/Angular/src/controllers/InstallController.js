(function () {

    // Get module.
    var module = angular.module('cantarus.poly-deploy');

    // Create controller.
    module.controller('InstallController', ['$state',
        function ($state) {

            // Nothing here, go to upload.
            $state.go('install.upload');
            
        }]);

})();
