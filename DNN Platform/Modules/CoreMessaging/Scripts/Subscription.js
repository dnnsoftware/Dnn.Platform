// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

(function ($) {
    window.Subscription = function Subscription (ko, settings, bindingElement) {
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
        this.localizationController = new LocalizationController(settings, that);
        this.subscriptionsViewModel = new SubscriptionsViewModel(ko, settings, that);

        this.localizer = function () {
            return that.localizationController;
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
            { value: '1', text: that.getString('Daily') },
            { value: '2', text: that.getString('Hourly') },
            { value: '3', text: that.getString('Weekly') },
            { value: '-1', text: that.getString('Never') }
        ];
        
        this.notifyFrequency = ko.observable(settings.notifyFrequency);
        this.msgFrequency = ko.observable(settings.msgFrequency);
        
        this.save = function() {
            var startSearch = function () {
                $('#fsFrequency', settings.moduleScope).addClass('searching');
            };

            var finishSearch = function () {
                $('#fsFrequency', settings.moduleScope).removeClass('searching');
            };

            var success = function (model) {
            	
                that.messageFrequency = model.messagesEmailFrequency;
                that.notificationFrequency = model.notificationsEmailFrequency;

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
                NotifyFreq: $('#ddlNotify').val(),
                MsgFreq: $('#ddlMsg').val()
            };

            that.requestService('Subscriptions/UpdateSystemSubscription', 'post', params, success, failure);
        };
        
        this['delete'] = function (model) {
            var localizer = that.localizationController;
            
            var success = function () {
                $('#divUnsubscribed').show();                
                that.subscriptionsViewModel.search();
                $("#divUnsubscribed").delay(1800).hide(400);
            };

            var failure = function (xhr, status) {
                $.dnnAlert({
                    title: localizer.getString('SubscribeFailureTitle'),
                    text: localizer.getString('SubscribeFailure').replace("{0}", status || that.getString('UnknownError'))
                });
            };
            
            $("<div class='dnnDialog'></div>").html(localizer.getString('UnsubscribeConfirm'))
                .dialog({
                    modal: true,
                    autoOpen: true,
                    dialogClass: "dnnFormPopup",
                    width: 400,
                    height: 200,
                    resizable: false,
                    title: localizer.getString('UnsubscribeConfirmTitle'),
                    buttons:
                        [
                            {
                                id: "ok_button",
                                text: localizer.getString('Yes'),
                                "class": "dnnPrimaryAction",
                                click: function () {

                                    var params = {                
                                        SubscriptionId: model.subscriptionId
                                    };

                                    that.requestService('Subscriptions/DeleteContentSubscription', 'post', params, success, failure);
                                    $(this).dialog("close");
                                }
                            },
                            {
                                id: "no_button",
                                text: localizer.getString('No'),
                                click: function () {
                                    $(this).dialog("close");
                                },
                                "class": "dnnSecondaryAction"
                            }
                        ]
                });
            };
        
        ko.applyBindings(this, document.getElementById(bindingElement));
    };

    Subscription.defaultSettings = {
        moduleId: -1,
        portalId: 0,
        pageSize: 25,
        notifyFrequency: 2,
        msgFrequency: 0        
    };
})(window.jQuery);