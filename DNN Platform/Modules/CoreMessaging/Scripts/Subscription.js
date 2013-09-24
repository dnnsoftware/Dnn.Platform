// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

(function ($) {
    window.Subscription = function Subscription (ko, settings) {
        var that = this;

        $.extend(this, Subscription.defaultSettings, settings);
        $.extend(this, settings);
	    
		this.requestService = function(path, type, data, success, failure) {
		    $.ajax({
			    type: type,
			    url: that.servicesFramework.getServiceRoot('CoreMessaging') + path,
			    beforeSend: that.servicesFramework.setModuleHeaders,
			    data: data,
			    cache: false
		    }).done(function(response) {
			    success.call(that, response);
		    }).fail(function(xhr, status) {
			    failure.call(that, xhr);
		    });
	    };
	    
        this.servicesFramework = $.ServicesFramework(settings.moduleId);
	    this.pageControl = new PagingControl(ko, settings, that);
        this.searchController = new SearchController(ko, settings, that);
        this.localizationController = new LocalizationController(settings, that);

        this.localizer = function () {
            return that.localizationController;
        };
        
        this.pager = function () {
            return that.pageControl;
        };

        this.searcher = function () {
            return that.searchController;
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

                return localizer.getString(localString).replace("{0}", search.totalResults());
            });
        
        // Return a JSON document specifying the filter parameters.
        this.getSearchParameters = function () {
            var pager = that.pageControl;
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
            	for (var index in model) {
            		var ac = model[index];
		            switch(ac.subscriberId) {
		            	case that.msgSubId():
		            		that.messageFrequency = ac.frequency;
		            		break;
		            	case that.notifySubId():
		            		that.notificationFrequency = ac.frequency;
		            		break;
		            }
	            }

                $('#divUpdated').show();
                $("#divUpdated").delay(1800).hide(400);
                finishSearch();
            };

            var failure = function (xhr, status) {
                var localizer = that.localizationController;

                $.dnnAlert({
                    title: localizer.getString('SubscribeFailureTitle'),
                    text: localizer.getString('SubscribeFailure').replace("{0}", status || that.getString('UnknownError'))
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

            that.requestService('Subscriptions/UpdateSystemSubscription', 'post', params, success, failure);
        };
        
        this['delete'] = function (model) {
            var success = function () {
                $('#divUpdated').show();
                
                var searcher = that.searcher();
                searcher.search();
                $("#divUpdated").delay(1800).hide(400);
            };

            var failure = function (xhr, status) {
                var localizer = that.localizationController;
                $.dnnAlert({
                    title: localizer.getString('SubscribeFailureTitle'),
                    text: localizer.getString('SubscribeFailure').replace("{0}", status || that.getString('UnknownError'))
                });
            };

            var params = {
                SubscriptionTypeId: model.subscriptionTypeId,
                SubscriberId: model.subscriberId
            };

            that.requestService('Subscriptions/DeleteContentSubscription', 'post', params, success, failure);
        };
    };

    Subscription.defaultSettings = {
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
})(window.jQuery);