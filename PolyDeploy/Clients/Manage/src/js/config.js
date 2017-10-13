module.exports = function (mod) {
    mod.config(['$stateProvider', '$urlRouterProvider', '$httpProvider',
        function ($stateProvider, $urlRouterProvider, $httpProvider) {

            // Default route.
            $urlRouterProvider.otherwise('/');

            // States.
            $stateProvider
                .state('index', {
                    url: '/',
                    template: require('./templates/index.html'),
                    controller: 'IndexController'
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
        }]);
};