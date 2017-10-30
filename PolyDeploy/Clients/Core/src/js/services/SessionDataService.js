module.exports = ['$http', 'apiUrl',
    function ($http, apiUrl) {

        var controllerUrl = apiUrl + 'Session/';

        // POST
        // Create a new session.
        function create() {

            // Make request.
            return $http.post(controllerUrl + 'Create').then(
                function (response) {

                    // Unpack data.
                    var data = response.data;

                    // Parse if we need to.
                    if (data.Response) {
                        data.Response = parseSessionResponse(data.Response);
                    } else {
                        data.Response = undefined;
                    }

                    return data;
                });
        }

        // GET
        // Get session by guid.
        function get(guid) {

            // Make request.
            return $http.get(controllerUrl + 'Get?guid=' + guid).then(
                function (response) {

                    // Unpack data.
                    var data = response.data;

                    // Parse if we need to.
                    if (data.Response) {
                        data.Response = parseSessionResponse(data.Response);
                    } else {
                        data.Response = undefined;
                    }

                    return data;
                });
        }

        // GET
        // Retrieve the session summary.
        function summary(guid) {

            // Make request.
            return $http.get(controllerUrl + 'Summary?guid=' + guid).then(
                function (response) {

                    // Grab unpacked data.
                    var data = response.data;

                    console.log(data);

                    var installJobs = [];

                    for (var prop in data) {

                        var installJob = {
                            order: parseInt(prop),
                            packages: []
                        };

                        var object = data[prop];

                        installJob.name = object.Name;
                        installJob.canInstall = object.CanInstall;

                        angular.forEach(object.Packages, function (pkg) {

                            var dependencies = [];

                            angular.forEach(pkg.Dependencies, function (dep) {

                                if (dep.Type === 'package') {

                                    dependencies.push(dep.Value);
                                }
                            });

                            installJob.packages.push({
                                name: pkg.Name,
                                version: pkg.VersionStr,
                                dependencies: dependencies
                            });
                        });

                        installJobs.push(installJob);
                    }

                    return installJobs;
                });
        }

        // GET
        // Start installation.
        function install(guid) {

            $http.get(controllerUrl + 'Install?guid=' + guid);
        }

        function parseSessionResponse(rawResponse) {

            // Parse to json.
            var response = JSON.parse(rawResponse);

            var newResponse = [];

            // Tidy the response to make it easier to deal with.
            for (var key in response) {

                var modulePackage = response[key];

                modulePackage.Order = parseInt(key);

                newResponse.push(modulePackage);
            }

            console.log(newResponse);

            return newResponse;
        }

        return {
            create: create,
            get: get,
            summary: summary,
            install: install
        };

    }];