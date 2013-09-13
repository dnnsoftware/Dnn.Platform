// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

(function ($) {
    window.SearchController = function (ko, settings, root) {
        var that = this;

	    this.servicesFramework = root.servicesFramework;
        this.results = ko.observableArray([]);
        
        this.totalResults = ko.observable(0);
        
        this.localizer = function () {
	        return root.localizationController;
        };
        
        this.search = function () {
            var startSearch = function () {
                $('.filter-content', settings.moduleScope).addClass('searching');
            };

            var finishSearch = function () {
                $('.filter-content', settings.moduleScope).removeClass('searching');
            };

            var success = function (model) {
                // Load the new results into our results array all at once to avoid flicker.
                var results = [];

                $.each(model.Results,
                    function (index, result) {
                        results.push(new SearchResult(ko, settings, result, root));
                    });

                that.results(results);
                that.totalResults(model.TotalResults || 0);

                finishSearch();
            };

            var failure = function (xhr, status) {                
                if ((status || new String()).length > 0) {
                    $.dnnAlert({
                        title: localizer.getString('SearchErrorTitle'),
                        text: localizer.getString('SearchError') + ': ' + (status || 'Unknown error')
                    });
                }

                that.results([]);
                that.totalResults(0);

                finishSearch();
            };

            startSearch();

            root.requestService('Subscriptions/GetSubscriptions', 'get', root.getSearchParameters(), success, failure, that.loading);
        };
	            
        // Wait for other components to finish registration
        $(document).ready(function () {
            if (root.pageControl != null) {
                root.pageControl.page.subscribe(
                    function () {
                        that.search();
                    });
            }

            // Do an initial search.
            that.search();
        });
    };
})(window.jQuery);