module.exports = ['$rootElement',
    function ($rootElement) {

        // Retrieve module id from data on roor element (The element ng-app is attached to).
        var moduleId = $rootElement.data().moduleId;

        // Set up services framework.
        var servicesFramework = $.ServicesFramework(moduleId);

        // Get the headers used to authenticate calls to dnn. 
        function getSecurityHeaders() {

            var headers = {
                ModuleId: servicesFramework.getModuleId(),
                TabId: servicesFramework.getTabId(),
            };

            var antiForgeryKey = servicesFramework.getAntiForgeryKey();

            headers[antiForgeryKey] = servicesFramework.getAntiForgeryValue();

            // for some reason this is what DNN accepts for ValidateAntiForgeryToken
            headers["RequestVerificationToken"] = servicesFramework.getAntiForgeryValue();

            return headers;
        };

        return {
            getSecurityHeaders: getSecurityHeaders
        };
    }];
