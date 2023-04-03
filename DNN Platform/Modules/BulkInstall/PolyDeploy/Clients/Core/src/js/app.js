var angular = require('angular');

// Create Angular module.
var angularModule = angular.module('cantarus.poly-deploy', []);

// Values
angularModule.value('baseUrl', '/');
angularModule.value('apiUrl', '/DesktopModules/PolyDeploy/API/');

// Boostrap.
require('./bootstrap')(angularModule);

module.exports = 'cantarus.poly-deploy';
