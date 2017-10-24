module.exports = ['$scope', 'SessionService',
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

            // Get session response.
            var response = session.Response;

            // Get counts.
            var installed = 0;
            var failed = 0;

            response.forEach(function (modPackage) {

                // Attempted install?
                if (modPackage.Attempted) {

                    // Success?
                    modPackage.Success ? installed++ : failed++;
                }
            });

            // Create start of string.
            var installStatus = 'Installation ';

            // Enumeration for session status.
            // Could consider moving this in to the
            // SessionDataService and having the correct enumeration
            // applied to the data as it comes in.
            var sessionStatusEnum = {
                '0': 'Not Started',
                '1': 'in Progress',
                '2': 'Complete'
            };

            // Append status.
            installStatus = installStatus + sessionStatusEnum[session.Status.toString()];

            // Append success and failure.
            installStatus = installStatus + ': ' + installed + ' successful installs and ' + failed + ' failures.';

            return installStatus;
        };

        // Get CSS class to apply to module panel.
        $scope.panelStatus = function (modPackage) {

            // Default class.
            var panelClass = 'panel-default';

            // Attempted install?
            if (modPackage.Attempted) {

                // Success?
                panelClass = modPackage.Success ? 'panel-success' : 'panel-danger';
            }

            console.log(panelClass);

            return panelClass;
        };

    }];