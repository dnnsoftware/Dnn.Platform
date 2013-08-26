// DotNetNuke® - http://www.dotnetnuke.com
//
// Copyright (c) 2002-2013 DotNetNuke Corporation
// All rights reserved.

(function (dnn) {
    'use strict';

    if (typeof dnn === 'undefined') dnn = {};
    if (typeof dnn.social === 'undefined') dnn.social = {};

    // The primary factory object for Social Library functionality.
    dnn.social.Module = function SocialModule (settings) {
        var that = this;

        this.settings = settings;

        this.componentFactory = new dnn.social.ComponentFactory(this.settings.moduleId);

        // Retrieve the component factory we use to resolve dependencies.
        this.getComponentFactory = function () {
            return that.componentFactory;
        };

        // Instantiate a new Services Framework caller for the specified server-side controller.
        this.getService = function (controller) {
            return new dnn.social.ServiceCaller($, that.settings, controller);
        };

        // Instantiate a new history.js integration controller (once)
        this.getHistory = function () {
            if (that.componentFactory.hasComponent('HistoryController') == false) {
                that.componentFactory.register(new dnn.social.History($, that.settings.navigationKey), 'HistoryController');
            }

            return that.componentFactory.resolve('HistoryController');
        };

        // Get a Social Controls controller for a particular Content Item and optional model containing initial state.
        this.getSocialController = function (contentItemId, model, objectKey) {
            return new dnn.social.SocialController($, ko, that.settings, that, model, contentItemId, objectKey);
        };

        // Instantiate a UI paging control that interacts with a particular controller from componentFactory.
        // You must register your controller in componentFactory and then pass its name into this method. Then,
        // when someone clicks Next or Previous etc. from the paging control, it will call the appropriate
        // navigation methods on your controller that will allow you to load a new set of data or whatever you
        // are doing when someone changes pages.  Your controller must implement 'totalRecords' and should subscribe
        // to page changes via pageControl.page.subscribe(function() { /* on change */ })
        this.getPagingControl = function (controllerName) {
            return new dnn.social.PagingControl($, ko, that.settings, that, controllerName);
        };

        // Get a CommentControl viewmodel for a set of inline comments.
        this.getCommentControl = function (control, contentItemId) {
            var s = $.extend({}, that.settings);
            s.moduleScope = $(control)[0];
            s.contentItemId = contentItemId;

            return new dnn.social.CommentControl($, ko, s);
        };

        // Create a LocalizationController that loads resources from the module SharedResources.resx,
        // as well as any additional views that are passed into the method via the resourceFiles array.
        // If resourceFiles is null or empty, only SharedResources is loaded.  Note that you only need to 
        // specify the list of resource files the first time this is called; after that, the reslting
        // component is registered in ComponentFactory so that the localized strings do not need to be reloaded.
        this.getLocalizationController = function () {
            if (that.componentFactory.hasComponent('LocalizationController') == false) {
                that.componentFactory.register(new dnn.social.LocalizationController($, that.settings, that));
            }

            return that.componentFactory.resolve('LocalizationController');
        };

        // Enable period updates to avoid getting 401 Unauthorized responses to requests that are sent after
        // a long period of time has elapsed betewen the last request and the most recent.  This just
        // sends an empty request based on some period of time configured based on IIS settings.
        this.disableSessionTimeout = function () {
            if (typeof window.sessionTimeoutDisabler === 'undefined') {
                var disabler = new dnn.social.SessionTimeoutDisabler($, that.settings, that);

                window.sessionTimeoutDisabler = disabler;

                disabler.start();
            }
        };

        // Retrieve a controller capable of searching for Content Items of a particular Content Type.
        this.getContentSearchController = function (contentTypeId) {
            return new dnn.social.ContentSearchController($, ko, that.settings, that, contentTypeId);
        };

        // Retrieve a focus controller for managing clicks to the main page or child controls and transferring
        // focus to 'non-focusable' items in HTML (i.e., a <div>).
        this.getFocusController = function (children) {
            return new dnn.social.FocusController($, ko, settings, that.social, that.settings.moduleScope, children);
        };
    };
})(window.dnn);