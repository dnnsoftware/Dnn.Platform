/*
    Services
    This file will register all your services for you.
*/

// Build require context of controllers.
var servicesContext = require.context('./', true, /\Service.js$/);

module.exports = function (mod) {

    // For each service.
    servicesContext.keys().forEach(function (service) {

        // Strip path.
        var serviceName = service.substring(service.lastIndexOf('/') + 1);

        // Strip extension.
        serviceName = serviceName.substring(0, serviceName.lastIndexOf('.js'));

        // Register service with module.
        mod.factory(serviceName, servicesContext(service));
    });
};
