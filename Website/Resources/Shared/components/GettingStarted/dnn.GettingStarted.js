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

            this.$this = $(this);

            this.$element = this.element ? $(this.element) : this._createLayout();

//            this._$selectedItemContainer = this.$element.find("." + this.options.selectedItemCss);
//            this._$selectedItemCaption = this._$selectedItemContainer.find("." + this.options.selectedValueCss);
        },

        _createLayout: function () {
            var checkBoxId = dnn.uid();
            var signUpBoxId = dnn.uid();

            var layout = $("<div class='" + this.options.containerCss + "'/>")
                    .append($("<div class='" + this.options.headerCss + "'/>")
                        .append($("<div class='" + this.options.headerLeftCss + "'/>")
                            .append($("<div/>")
                                .append($("<div class='" + this.options.headerLeftTextCss + "'/>")
                                    .append($("<div/>")
                                        .append($("<label for='" + signUpBoxId + "'>" + "Sign Up For Our Newsleter" + "</label>"))
                                        .append($("<span>" + "Get Tips, tricks, and Product Updates In Your Inbox" + "</span>"))))

                                .append($("<div class='" + this.options.headerLeftInputCss + "'/>")
                                    .append($("<div/>")
                                        .append($("<div class='" + this.options.inputboxWrapperCss + "'/>")
                                            .append($("<input type='text' id='" + signUpBoxId + "' maxlength='200' autocomplete='off'/>")))
                                        .append($("<a href='javascript:void(0);' title='" + this.options.signUpButtonTooltip + "'>" + "Sign Up" + "</a>"))))))

                        .append($("<div class='" + this.options.headerRightCss + "'/>")
                            .append($("<div/>")
                                .append($("<a href='javascript:void(0);' title='" + this.options.downloadUserManualTooltip + "'><span>" + "Download the User Manual" + "</span></a>")))))

                    .append($("<div class='" + this.options.contentCss + "'/>")
                        .append($("<div/>")
                            .append($("<iframe src='about:blank' scrolling='auto' frameborder='0' />"))))

                    .append($("<div class='" + this.options.footerCss + "'/>")
                        .append($("<div class='" + this.options.footerBorderCss + "'/>")
                            .append($("<div/>")))

                        .append($("<div class='" + this.options.footerLeftCss + "'/>")
                            .append($("<input type='checkbox' id='" + checkBoxId + "' value='check1' name='dontshow' >"))
                            .append($("<label for='" + checkBoxId + "'>" + "Don't show this again" + "</label>")))

                        .append($("<div class='" + this.options.footerRightCss + "'/>")
                            .append($("<a href='javascript:void(0);' class='" + this.options.tweetCss + "' title='" + this.options.tweetTooltip + "'/>"))
                            .append($("<a href='javascript:void(0);' class='" + this.options.facebookCss + "' title='" + this.options.facebookTooltip + "'/>"))))

            return layout;
        },

        show: function() {
            this.$element.dialog({
                modal: true,
                autoOpen: true,
                dialogClass: "dnnFormPopup",
                title: "Welcome to your website",
                resizable: false,
                width: 950,
                height: 640,
                close: function () { /*alert("closing");*/ }
            });
        }

    };

    GettingStarted._defaults = {
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
        tweetCss: "gs-tweet-button",
        facebookCss: "gs-facebook-button"
    };

    GettingStarted.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(GettingStarted._defaults, settings);
        }
        return GettingStarted._defaults;
    };

}).apply(dnn, [jQuery, window, document]);


dnn.showGettingStarted = function (options) {
    $(document).ready(function () {
        var instance = new dnn.GettingStarted(null, options);
        instance.show();
    });
};

