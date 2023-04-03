var controllers = require('./controllers'),
    services = require('./services');

module.exports = function (mod) {

    // Register controllers.
    controllers(mod);

    // Register services.
    services(mod);
};
