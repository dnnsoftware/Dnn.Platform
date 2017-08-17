(function () {

    // Get module.
    var module = angular.module('cantarus.poly-deploy');

    // Create controller.
    module.controller('UploadController', ['$scope', 'FileUploader', 'SessionService', 'apiUrl',
        function ($scope, FileUploader, SessionService, apiUrl) {

            // Wait for session guid.
            SessionService.session.then(function (sessionGuid) {

                // Construct upload url.
                var uploadUrl = apiUrl + 'Package/Add?guid=' + sessionGuid;

                console.log('uploadUrl: ' + uploadUrl);

                // Create uploader.
                $scope.uploader = new FileUploader({
                    url: uploadUrl
                });
            });

        }]);
})();
