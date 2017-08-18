(function () {

    // Get module.
    var module = angular.module('cantarus.poly-deploy');

    // Create controller.
    module.controller('UploadController', ['$scope', 'FileUploader', 'SessionService', 'apiUrl',
        function ($scope, FileUploader, SessionService, apiUrl) {

            // Store for errors.
            $scope.errors = [];

            // Wait for session guid.
            SessionService.sessionPromise.then(setupUploader);

            // Setup uploader.
            function setupUploader(session) {

                // Construct upload url.
                var uploadUrl = apiUrl + 'Session/AddPackage?guid=' + session.Guid;

                // Create uploader.
                var uploader = new FileUploader({
                    url: uploadUrl
                });

                // Place on scope.
                $scope.uploader = uploader;

                // Add .zip filter.
                uploader.filters.push({
                    name: 'zipOnly',
                    rejectionMessage: 'is not a .zip file.',
                    fn: function (item, options) {

                        // Is there a name?
                        if (!item.name) {
                            return false;
                        }

                        // Very rudimentary check to see if there is a .zip extension.
                        var name = item.name;

                        // Get extension.
                        var ext = name.substring(name.lastIndexOf('.'));

                        // Is .zip?
                        if (ext.toLowerCase() !== '.zip') {
                            return false;
                        }

                        return true;
                    }
                });

                // Add handling for failed file add.
                uploader.onWhenAddingFileFailed = function (item, filter, options) {
                    $scope.errors.push(item.name + ' ' + filter.rejectionMessage);
                };
            }

        }]);
})();
