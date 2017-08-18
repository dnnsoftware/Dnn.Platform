(function () {

    // Get module.
    var module = angular.module('cantarus.poly-deploy');

    // Create controller.
    module.controller('ResultController', ['$scope', 'SessionService',
        function ($scope, SessionService) {

            // Wait for session.
            SessionService.sessionPromise.then(
                function (sess) {

                    // Add session to scope.
                    var session = SessionService.session;

                    $scope.session = session;

                    // Install not started?
                    if (session.Status === 0) {

                        // No, start install.
                        SessionService.install();
                    }
                });

            // Current status.
            $scope.currentStatus = function (session) {

                // Got a response?
                if (!session.Response) {

                    // No.
                    return 'Starting install...';
                }

                var response = session.Response;

                var installed = response.Installed.length;
                var failed = response.Failed.length;
                var installStatus = 'Installation ';

                if (session.Status === 1) {
                    installStatus = installStatus + 'in Progress';
                }

                if (session.Status === 2) {
                    installStatus = installStatus + 'Complete';
                }

                return installStatus + ': ' + installed + ' successful installs and ' + failed + ' failures.';
            };

        }]);
})();
