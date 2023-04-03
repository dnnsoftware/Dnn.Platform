var controllers = require('./controllers'),
    directives = require('./directives'),
    services = require('./services'),
    config = require('./config');

module.exports = function (mod) {

    // Register controllers.
    controllers(mod);

    // Register directives.
    directives(mod);

    // Register services.
    services(mod);

    // Configure.
    mod.config(config);
};
