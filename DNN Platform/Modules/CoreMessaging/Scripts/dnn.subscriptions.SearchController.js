// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

(function (dnn) {
    'use strict';

    if (typeof dnn === 'undefined') dnn = {};
    if (typeof dnn.subscriptions === 'undefined') dnn.subscriptions = {};

    dnn.subscriptions.SearchController = function SearchController($, ko, settings, social, root) {
        var that = this;

        this.social = new dnn.social.Module(settings);

        this.componentFactory = this.social.getComponentFactory();
        
        this.service = social.getService('Subscriptions');

        this.results = ko.observableArray([]);
        
        this.totalResults = ko.observable(0);
        
        this.localizer = function () {
            return that.social.getLocalizationController();
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
                        // $, ko, settings, root, social, model
                        results.push(new dnn.subscriptions.SearchResult($, ko, settings, root, social, result));
                    });

                that.results(results);
                that.totalResults(model.TotalResults || 0);

                finishSearch();
            };

            var failure = function (xhr, status) {
                var localizer = social.getLocalizationController();
                
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

            that.service.get('GetSubscriptions', root.getSearchParameters(), success, failure, that.loading);
        };
        
        // Wait for other components to finish registration
        dnn.social.EventQueue.push(
            function () {
                var pagingControl = social.getComponentFactory().resolve('PagingControl');
                if (pagingControl != null) {
                    pagingControl.page.subscribe(
                        function () {
                            that.search();
                        });
                }

                // Do an initial search.
                that.search();
            });
    };
})(window.dnn);