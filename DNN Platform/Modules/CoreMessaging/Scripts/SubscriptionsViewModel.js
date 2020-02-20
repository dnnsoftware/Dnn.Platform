// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

(function ($) {
    window.SubscriptionsViewModel = function (ko, settings, root) {
        var self = this;

        this.isLoading = ko.observable(false);
        this.results = ko.observableArray([]);
                
        this.localizer = function () {
	        return root.localizationController;
        };
        
        this.search = function () {
            var startSearch = function () {
                self.isLoading(true);
            };

            var finishSearch = function () {
                self.isLoading(false);
            };

            var success = function (model) {
                self.results(model.Results);
                self.totalCount(model.TotalResults || 0);

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
                pageIndex: self.currentPage(),
                pageSize: self.pageSize(),
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
        
        self.totalCount = ko.observable(0);

        self.pageSize = ko.observable('10');

        self.currentPage = ko.observable(0);

        self.pageSlide = ko.observable(2);
        
        self.lastPage = ko.computed(function () {
            return Math.ceil(self.totalCount() / self.pageSize());
        });
        
        this.pages = ko.computed(function () {
            var pageCount = self.lastPage();
            var pageFrom = Math.max(1, self.currentPage() - self.pageSlide());
            var pageTo = Math.min(pageCount, self.currentPage() + self.pageSlide());
            pageFrom = Math.max(1, Math.min(pageTo - 2 * self.pageSlide(), pageFrom));
            pageTo = Math.min(pageCount, Math.max(pageFrom + 2 * self.pageSlide(), pageTo));

            var result = [];
            for (var i = pageFrom; i <= pageTo; i++) {
                result.push(i);
            }

            return result;
        });
        
        self.changePage = function (page) {
            self.currentPage(page);
            self.search();
        };
        
        self.totalItemsText = ko.computed(function () {
            if (self.totalCount() == 1) {
                return self.localizer().getString('OneItem');
            } else if (self.totalCount() < self.pageSize()) {
                return self.localizer().getString('Items')
                    .replace('[ITEMS]', self.totalCount());
            } else {
                return self.localizer().getString('ItemsOnPage')
                    .replace('[ITEMS]', self.totalCount())
                    .replace('[PAGES]', Math.ceil(self.totalCount() / self.pageSize()));
            }
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