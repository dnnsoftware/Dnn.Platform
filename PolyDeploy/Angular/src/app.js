(function () {

    var appRoot = '/DesktopModules/Cantarus/PolyDeploy/Angular/src/'

    // Create Angular module.
    var module = angular.module('cantarus.poly-deploy', [
        'ui.router'
    ]);

    // Set up values.
    module.value('baseUrl', '/');
    module.value('appRoot', appRoot);
    module.value('apiUrl', '/DesktopModules/PolyDeploy/API/');

    // Configure routing.
    module.config(function ($stateProvider, $urlRouterProvider) {

        // Default route.
        $urlRouterProvider.otherwise('/install/upload');

        // States.
        $stateProvider
            .state('install', {
                url: '/install',
                templateUrl: appRoot + 'templates/install.html',
                controller: 'InstallController'
            })
            .state('install.upload', {
                url: '/upload',
                templateUrl: appRoot + 'templates/upload.html',
                controller: 'UploadController'
            })
            .state('install.summary', {
                url: '/summary',
                templateUrl: appRoot + 'templates/summary.html',
                controller: 'SummaryController'
            });
    });

})();
