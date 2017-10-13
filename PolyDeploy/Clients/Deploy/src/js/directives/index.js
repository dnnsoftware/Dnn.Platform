/*
    Directives
    This file will register all your directives for you.
*/

// Build require context of directives.
var directivesContext = require.context('./', true, /\.js$/);

module.exports = function (mod) {

    // For each directive.
    directivesContext.keys().forEach(function (directive) {

        // Strip path.
        var directiveName = directive.substring(directive.lastIndexOf('/') + 1);

        // Strip extension.
        directiveName = directiveName.substring(0, directiveName.lastIndexOf('.js'));

        // Lowercase first letter.
        directiveName = directiveName.charAt(0).toLowerCase() + directiveName.slice(1);

        // Register directive with module.
        mod.directive(directiveName, directivesContext(directive));
    });
};
