// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

(function (dnn) {
    'use strict';

    if (typeof dnn === 'undefined') dnn = {};
    if (typeof dnn.subscriptions === 'undefined') dnn.subscriptions = {};

    dnn.subscriptions.Subscription = function Subscription($, ko, settings) {
        var that = this;

        $.extend(this, dnn.subscriptions.Subscription.defaultSettings, settings);
        $.extend(this, settings);
        
        this.social = new dnn.social.Module(settings);

        this.componentFactory = this.social.getComponentFactory();
        
        this.componentFactory.register(new dnn.subscriptions.SearchController($, ko, settings, this.social, that));
        this.componentFactory.register(this.social.getPagingControl('SearchController'));

        this.localizer = function () {
            return that.social.getLocalizationController();
        };
        
        this.pager = function () {
            return that.componentFactory.resolve('PagingControl');
        };

        this.searcher = function () {
            return that.componentFactory.resolve('SearchController');
        };
        
        this.getString = function (key) {
            var localizer = that.localizer();
            if (localizer != null) {
                return localizer.getString(key);
            }

            return key;
        };
        
        this.frequencyOptions = [
            { value: '0', text: that.getString('Instant') },
            { value: '1', text: that.getString('Dialy') },
            { value: '2', text: that.getString('Hourly') },
            { value: '3', text: that.getString('Weekly') },
            { value: '-1', text: that.getString('Never') }
        ];
        
        this.notifyFrequency = ko.observable(settings.notifyFrequency);
        this.msgFrequency = ko.observable(settings.msgFrequency);
        this.notifySubId = ko.observable(settings.notifySubscriberId);
        this.msgSubId = ko.observable(settings.msgSubscriberId);
        this.notifySubTypeId = ko.observable(settings.notifySubTypeId);
        this.msgSubTypeId = ko.observable(settings.msgSubTypeId);
        
        this.totalResults = ko.dependentObservable(
            function () {
                var localizer = that.localizer();
                if (localizer == null) {
                    return null;
                }

                var search = that.searcher();
                if (search == null) {
                    return null;
                }

                var localString = 'ResultCount';

                if (search.totalResults() == 1) {
                    localString = 'SingleResultCount';
                }

                return localizer.getString(localString).format(search.totalResults());
            });
        
        // Return a JSON document specifying the filter parameters.
        this.getSearchParameters = function () {
            var pager = that.componentFactory.resolve('PagingControl');
            if (pager == null) {
                return null;
            }
            
            return {
                pageIndex: pager.page(),
                pageSize: pager.pageSize || 25
            };
        };

        this.pagingControl = ko.dependentObservable(
            function () {
                var controller = that.pager();
                if (controller != null) {
                    return controller.markup();
                }
            });

        this.prev = function () {
            var controller = that.pager();
            if (controller != null) {
                if (that.prevClass() !== 'disabled') {
                    controller.previous();
                }
            }
        };

        this.next = function () {
            var controller = that.pager();
            if (controller != null) {
                if (that.nextClass() !== 'disabled') {
                    controller.next();
                }
            }
        };

        this.refresh = function () {
            var controller = that.searcher();
            if (controller != null) {
                controller.load();
            }
        };

        this.save = function() {
            var startSearch = function () {
                $('#fsFrequency', settings.moduleScope).addClass('searching');
            };

            var finishSearch = function () {
                $('#fsFrequency', settings.moduleScope).removeClass('searching');
            };

            var success = function (model) {
                that.notificationFrequency = ko.observable(model.notifyFreq);
                that.messageFrequency = ko.observable(model.msgFreq);
                that.notifySubId = ko.observable(model.notifySubscriberId);
                that.msgSubId = ko.observable(model.msgSubscriberId);

                $('#divUpdated').show();
                $("#divUpdated").delay(1800).hide(400);
                finishSearch();
            };

            var failure = function (xhr, status) {
                var localizer = that.social.getLocalizationController();

                $.dnnAlert({
                    title: localizer.getString('SubscribeFailureTitle'),
                    text: localizer.getString('SubscribeFailure').format(status || that.getString('UnknownError'))
                });

                finishSearch();
            };

            startSearch();

            var params = {
                NotifySubscriberId: that.notifySubId(),
                MsgSubscriberId: that.msgSubId(),
                NotifyFreq: $('#ddlNotify').val(),
                MsgFreq: $('#ddlMsg').val(),
                NotiySubscriptionTypeId: that.notifySubTypeId(),
                MsgSubscriptionTypeId: that.msgSubTypeId()
            };

            this.service = that.social.getService('Subscriptions');
            this.service.post('UpdateSystemSubscription', params, success, failure);
        };
        
        this['delete'] = function (model) {
            var success = function () {
                $('#divUpdated').show();
                
                var searcher = that.searcher();
                searcher.search();
                $("#divUpdated").delay(1800).hide(400);
            };

            var failure = function (xhr, status) {
                var localizer = that.social.getLocalizationController();
                $.dnnAlert({
                    title: localizer.getString('SubscribeFailureTitle'),
                    text: localizer.getString('SubscribeFailure').format(status || that.getString('UnknownError'))
                });
            };

            var params = {
                SubscriptionTypeId: model.subscriptionTypeId,
                SubscriberId: model.subscriberId
            };

            this.service = that.social.getService('Subscriptions');
            this.service.post('DeleteContentSubscription', params, success, failure);
        };
    };

    dnn.subscriptions.Subscription.defaultSettings = {
        moduleId: -1,
        portalId: 0,
        pageSize: 25,
        notifySubscriberId: -1,
        msgSubscriberId: -1,
        notifyFrequency: 0,
        msgFrequency: 1,
        NotifySubTypeId: 1,
        MsgSubTypeId: 2
    };
})(window.dnn);