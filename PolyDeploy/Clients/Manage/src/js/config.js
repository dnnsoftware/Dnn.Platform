module.exports = function (mod) {
    mod.config(['$stateProvider', '$urlRouterProvider', '$httpProvider',
        function ($stateProvider, $urlRouterProvider, $httpProvider) {

            // Default route.
            $urlRouterProvider.otherwise('/');

            // States.
            $stateProvider
                .state('menu', {
                    template: require('./templates/menu.html'),
                })
                .state('menu.welcome', {
                    url: '/',
                    template: require('./templates/welcome.html')
                })
                .state('menu.users', {
                    url: '/users',
                    template: require('./templates/users.html'),
                    controller: 'UsersController'
                })
                .state('menu.whitelist', {
                    url: '/whitelist',
                    template: require('./templates/whitelist.html'),
                    controller: 'WhitelistController'
                })
                .state('menu.events', {
                    url: '/events',
                    template: require('./templates/events.html'),
                    controller: 'EventsController'
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