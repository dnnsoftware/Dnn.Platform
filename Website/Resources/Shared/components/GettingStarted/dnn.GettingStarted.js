; if (typeof window.dnn === "undefined" || window.dnn === null) { window.dnn = {}; }; //var dnn = dnn || {};

// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

(function ($, window, document, undefined) {
    "use strict";

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variables rather than globals
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).

    //
    // GettingStarted
    //
    var GettingStarted = this.GettingStarted = function (element, options) {
        this.element = element;
        this.options = options;

        this.init();
    };

    GettingStarted.prototype = {

        constructor: GettingStarted,

        init: function () {
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend({}, GettingStarted.defaults(), this.options);

            this._controller = new GettingStartedController();

            this.$this = $(this);

            this.$element = this.element ? $(this.element) : this._createLayout();
            this._$iframe = this.$element.find("iframe");
            this._$downloadManual = this.$element.find("." + this.options.headerRightCss).find("a");
            //this._$downloadManual.attr("href", this.options.userManualLinkUrl);
            //this._$selectedItemCaption = this._$selectedItemContainer.find("." + this.options.selectedValueCss);

            if (this.options.showOnStartup) {
                this.show();
            }
        },

        _createLayout: function () {
            var checkBoxId = dnn.uid("gs_");
            var signUpBoxId = dnn.uid("gs_");

            var layout = $("<div class='" + this.options.containerCss + "'/>")
                .append($("<div class='" + this.options.headerCss + "'/>")
                    .append($("<div class='" + this.options.headerLeftCss + "'/>")
                        .append($("<div/>")
                            .append($("<div class='" + this.options.headerLeftTextCss + "'/>")
                                .append($("<div/>")
                                    .append($("<label for='" + signUpBoxId + "'>" + this.options.signUpLabel + "</label>"))
                                    .append($("<span>" + this.options.signUpText + "</span>"))))
                            .append($("<div class='" + this.options.headerLeftInputCss + "'/>")
                                .append($("<div/>")
                                    .append($("<div class='" + this.options.inputboxWrapperCss + "'/>")
                                        .append($("<input type='text' id='" + signUpBoxId + "' maxlength='200' autocomplete='off'/>")))
                                    .append($("<a href='javascript:void(0);' title='" + this.options.signUpButton + "'>" + this.options.signUpButton + "</a>"))))))
                    .append($("<div class='" + this.options.headerRightCss + "'/>")
                        .append($("<div/>")
                            .append($("<a href='javascript:void(0);' title='" + this.options.downloadManualButton + "'><span>" + this.options.downloadManualButton + "</span></a>")))))
                .append($("<div class='" + this.options.contentCss + "'/>")
                    .append($("<div/>")
                        .append($("<iframe src='about:blank' scrolling='auto' frameborder='0' />"))))
                .append($("<div class='" + this.options.footerCss + "'/>")
                    .append($("<div class='" + this.options.footerBorderCss + "'/>")
                        .append($("<div/>")))
                    .append($("<div class='" + this.options.footerLeftCss + "'/>")
                        .append($("<input type='checkbox' id='" + checkBoxId + "' value='dontshow' name='ShowDialog' >"))
                        .append($("<label for='" + checkBoxId + "'>" + this.options.dontShowDialogLabel + "</label>")))
                    .append($("<div class='" + this.options.footerRightCss + "'/>")
                        .append($("<a href='//twitter.com/dnncorp' class='" + this.options.twitterLinkCss + "' title='" + this.options.twitterLinkTooltip + "'/>"))
                        .append($("<a href='//facebook.com/dotnetnuke' class='" + this.options.facebookLinkCss + "' title='" + this.options.facebookLinkTooltip + "'/>"))));

            return layout;
        },

        _onCloseDialog: function () {
            var checkBox = this.$element.find("." + this.options.footerLeftCss).find("input");
            if(checkBox.prop("checked")){
                this._controller.hideDialog();
            }
        },

        show: function () {
            this._$iframe.attr("src", this.options.contentUrl);
            this.$element.dialog({
                modal: true,
                autoOpen: true,
                dialogClass: "dnnFormPopup " + this.options.dialogCss,
                title: this.options.title,
                resizable: false,
                width: 950,
                height: 640,
                close: $.proxy(this._onCloseDialog, this)
            });
        }

    };

    GettingStarted._defaults = {
        dialogCss: "gs-dialog",
        containerCss: "gs-container",
        headerCss: "gs-header",
        headerLeftCss: "gs-header-left-side",
        headerLeftTextCss: "gs-header-left-side-text",
        headerLeftInputCss: "gs-header-left-side-input",
        inputboxWrapperCss: "gs-header-left-side-inputbox-wrapper",
        headerRightCss: "gs-header-right-side",
        contentCss: "gs-content",
        footerCss: "gs-footer",
        footerBorderCss: "gs-footer-border",
        footerLeftCss: "gs-footer-left-side",
        footerRightCss: "gs-footer-right-side",
        twitterLinkCss: "gs-twitter-button",
        facebookLinkCss: "gs-facebook-button"
    };

    GettingStarted.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(GettingStarted._defaults, settings);
        }
        return GettingStarted._defaults;
    };


    //
    // GettingStarted Controller
    //
    var GettingStartedController = this.GettingStartedController = function (options) {
        this.options = options;
        this.init();
    };

    GettingStartedController.prototype = {
        constructor: GettingStartedController,

        init: function() {
            this.options = $.extend({}, GettingStartedController.defaults(), this.options);
            this._serviceUrl = $.dnnSF(this.options.moduleId).getServiceRoot(this.options.serviceRoot);
        },

        _callPost: function(data, onLoadHandler, method) {
            var serviceSettings = {
                url: this._serviceUrl + method,
                beforeSend: $.dnnSF(this.options.moduleId).setModuleHeaders,
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: data,
                type: "POST",
                async: true,
                success: onLoadHandler,
                error: $.onAjaxError
            };
            $.ajax(serviceSettings);
        },

        hideDialog: function (onHideDialogCallback) {
            var onHideDialogHandler = $.proxy(this._onHideDialog, this, onHideDialogCallback);
            this._callPost({}, onHideDialogHandler, this.options.hideDialogMethod);
        },

        _onHideDialog: function (onHideDialogCallback, data, textStatus, jqXhr) {
            if (typeof onHideDialogCallback === "function") {
                onHideDialogCallback.apply(this, [data]);
            }
        }

    };

    GettingStartedController._defaults = {
        serviceRoot: "InternalServices",
        hideDialogMethod: "GettingStarted/HideGettingStartedPage"
    };

    GettingStartedController.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(GettingStartedController._defaults, settings);
        }
        return GettingStartedController._defaults;
    };


}).apply(dnn, [jQuery, window, document]);


dnn.showGettingStarted = function (options) {
    $(document).ready(function () {
        var instance = new dnn.GettingStarted(null, options);
    });
};

