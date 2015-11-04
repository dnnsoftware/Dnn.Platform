dnn = dnn || {};
dnn.modules = dnn.modules || {};
dnn.modules.dynamicContentManager = dnn.modules.dynamicContentManager || {};

dnn.modules.dynamicContentManager.cf = (function () {
    return {
        init: function() {
            var userSettings = window.parent['__userSettings'];

            return {
                userSettings: userSettings
            };
        }
    };
}());