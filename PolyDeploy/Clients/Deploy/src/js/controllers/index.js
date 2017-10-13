/*
    Controllers
    This file will register all your controllers for you.
*/

// Build require context of controllers.
var controllersContext = require.context('./', true, /\Controller.js$/);

module.exports = function (mod) {

    // For each controller.
    controllersContext.keys().forEach(function (controller) {

        // Strip path.
        var controllerName = controller.substring(controller.lastIndexOf('/') + 1);

        // Strip extension.
        controllerName = controllerName.substring(0, controllerName.lastIndexOf('.js'));

        // Register controller with module.
        mod.controller(controllerName, controllersContext(controller));
    });
};
