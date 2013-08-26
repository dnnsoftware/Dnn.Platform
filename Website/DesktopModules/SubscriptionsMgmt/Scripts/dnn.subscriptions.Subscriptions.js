// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

dnn.activities = dnn.activities || {};

(function (dnn) {
    'use strict';

    dnn.subscriptions.Subscriptions = function Subscriptions($, ko, settings) {
        var that = this;

        $.extend(this, dnn.subscriptions.Subscriptions.defaultSettings, settings);

        this.social = new dnn.social.Module(settings);

        this.service = this.social.getService('Activity');

        this.desktopModuleId = ko.observable(-1);

        this.searchText = ko.observable(new String());

        var searchController = new dnn.subscriptions.SearchController($, ko, settings, this.social, this);

        this.componentFactory = this.social.getComponentFactory();
        this.componentFactory.register(searchController);
        this.componentFactory.register(this.social.getPagingControl('SearchController'));

        // Sorted knockout table of results. We actually don't do any sorting clientside; we let the SF controller
        // handle sorting. We just have to connect the header onClick so that the sort method/order will be switched
        // at the appropriate time, triggering a new search call to the controller.
        this.sortedTable = new dnn.subscriptions.SortTable($, $('#subscription-table', settings.moduleScope)[0], searchController.results, null);
        this.sortedTable.column = 'Activity';
        this.sortedTable.ascending = true;
        this.sortedTable.updateColumns();

        // Return a JSON document specifying the filter parameters.
        
        this.getSearchParameters = function () {
            var pager = that.componentFactory.resolve('PagingControl');
            if (pager == null) {
                return null;
            }

            return {
                searchText: that.searchText(),
                pageIndex: pager.page(),
                pageSize: pager.pageSize || 25,
                sortColumn: that.sortedTable.column,
                sortAscending: that.sortedTable.ascending
            };
        };

        this.loadStart = function () {
            var pager = that.componentFactory.resolve('PagingControl');
            if (pager.page() != 0) {
                pager.page(0);
            }
            else {
                searchController.search();
            }
        };

        this.pagingControl = ko.dependentObservable(
            function () {
                var pager = that.componentFactory.resolve('PagingControl');
                if (pager) {
                    return pager.markup();
                }

                return new String();
            });

        // Reload search results 250ms after last keypress in the search input for a live-update effect.
        this.searchText.subscribe(
            function () {
                if (typeof (that.searchTimeout) !== 'undefined' && that.searchTimeout !== null) {
                    clearTimeout(that.searchTimeout);
                }

                that.searchTimeout = setTimeout(
                    function () {
                        that.loadStart();
                    }, 250);
            });
    };

    dnn.subscriptions.Subscriptions.defaultSettings = {
        moduleId: -1,
        portalId: 0,
        pageSize: 25
    };
})(window.dnn);