var angular = require('angular'),
    uirouter = require('angular-ui-router'),

    // Angular File Upload (GitHub: https://github.com/nervgh/angular-file-upload/)
    fileUpload = require('angular-file-upload'),

    // Doesn't follow convention and return its app name in module.exports.
    fileUpload = 'angularFileUpload';

var polyCore = require('../../../Core/src/js/app');

// Create Angular module.
var angularModule = angular.module('cantarus.poly-deploy.manage', [
    uirouter,
    fileUpload,
    polyCore
]);

// Values
angularModule.value('appRoot', '/DesktopModules/Cantarus/PolyDeploy/Angular/src/');

// Boostrap.
require('./bootstrap')(angularModule);

// Configure.
require('./config')(angularModule);

module.exports = 'cantarus.poly-deploy.manage';