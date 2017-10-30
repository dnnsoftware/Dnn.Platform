module.exports = ['$stateProvider', '$urlRouterProvider', '$httpProvider',
    function ($stateProvider, $urlRouterProvider, $httpProvider) {

        // Default route.
        $urlRouterProvider.otherwise('/install/upload');

        // States.
        $stateProvider
            .state('install', {
                url: '/install',
                template: require('./templates/install.html'),
                controller: 'InstallController'
            })
            .state('install.upload', {
                url: '/upload',
                template: require('./templates/upload.html'),
                controller: require('./controllers/UploadController')
            })
            .state('install.summary', {
                url: '/summary',
                template: require('./templates/summary.html'),
                controller: 'SummaryController'
            })
            .state('install.result', {
                url: '/result',
                template: require('./templates/result.html'),
                controller: 'ResultController'
            })
            .state('manage-users', {
                url: '/manage-users',
                template: require('./templates/manage-users.html'),
                controller: 'ManageUsersController'
            });

        // Add $http interceptor for DNN Services Framework.
        $httpProvider.interceptors.push(['DnnService',
            function (DnnService) {
                return {
                    request: function (config) {

                        var securityHeaders = DnnService.getSecurityHeaders();

                        Object.keys(securityHeaders).forEach(function (key) {
                            config.headers[key] = securityHeaders[key];
                        });

                        return config;
                    }
                };
            }]);
    }];