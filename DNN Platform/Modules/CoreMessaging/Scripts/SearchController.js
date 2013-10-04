// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

(function ($) {
    window.SearchController = function (ko, settings, root) {
        var self = this;

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

                self.results(results);
                self.totalResults(model.TotalResults || 0);

                finishSearch();
            };

            var failure = function (xhr, status) {                
                if ((status || new String()).length > 0) {
                    $.dnnAlert({
                        title: localizer.getString('SearchErrorTitle'),
                        text: localizer.getString('SearchError') + ': ' + (status || 'Unknown error')
                    });
                }

                self.results([]);
                self.totalResults(0);

                finishSearch();
            };

            startSearch();

            var searchParameters = {
                pageIndex: 0,
                pageSize: 10,
                sortExpression: self.sortedByColumnName() + ' ' + self.sortedByOrder()
            };

            root.requestService('Subscriptions/GetSubscriptions', 'get', searchParameters, success, failure, self.loading);
        };
	          
        this.sortedByColumnName = ko.observable('');
        this.sortedByOrder = ko.observable('');
        this.currentPage = ko.observable(10);
        
        this.sortColumn = function (columnName) {
            var sort = '';
            if (self.sortedByColumnName() == columnName) {
                if (self.sortedByOrder() == 'asc') sort = 'desc';
                if (self.sortedByOrder() == '') sort = 'asc';
            } else {
                sort = 'asc';
                self.sortedByColumnName(columnName);
            }

            self.sortedByOrder(sort);
            self.currentPage(0);
            self.search();
        };

        this.sortByDescription = function () {
            self.sortColumn('Description');
        };

        this.sortBySubscriptionType = function () {
            self.sortColumn('SubscriptionType');
        };

        this.sortCss = function (columnName) {
            if (self.sortedByColumnName() == columnName) {
                if (self.sortedByOrder() == 'asc') return 'sortAsc';

                if (self.sortedByOrder() == 'desc') return 'sortDesc';
            }
            
            return '';
        };

        this.sortCssDescription = ko.computed(function () {
            return self.sortCss('Description');
        });
        
        this.sortCssSubscriptionType = ko.computed(function () {
            return self.sortCss('SubscriptionType');
        });
        
        // Wait for other components to finish registration
        $(document).ready(function () {
            if (root.pageControl != null) {
                root.pageControl.page.subscribe(
                    function () {
                        self.search();
                    });
            }

            // Do an initial search.
            self.search();
        });
    };
})(window.jQuery);