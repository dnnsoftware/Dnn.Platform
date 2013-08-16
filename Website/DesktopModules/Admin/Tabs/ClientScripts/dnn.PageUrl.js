; if (typeof dnn === "undefined" || dnn === null) { dnn = {}; }; //var dnn = dnn || {};

// the semi-colon before function invocation is a safety net against concatenated 
// scripts and/or other plugins which may not be closed properly.
(function ($, window, document, undefined) {
    "use strict";

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variables rather than globals
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).

    // The actual plugin constructor
    var PageUrl = this.PageUrl = function (options) {
        this.options = options;
        this.init();
    };

    PageUrl.prototype = {
        constructor: PageUrl,

        init: function () {

            this.options = $.extend({}, PageUrl.defaults(), this.options);
            this.$this = $(this);
            this._serviceUrl = $.dnnSF().getServiceRoot("InternalServices");

            // Select a Page to Test section:
            this._$pageUrlContainer = $("#" + this.options.pageUrlContainerId);
            this._$pageUrlInputInput = $("#" + this.options.pageUrlInputId);
            this._$pageUrlInputInput.prefixInput({ prefix: "/" });
        },

        makeUpdatable: function() {
            var $button = $("<a href='javascript:void(0);' class='dnnSecondaryAction' />")
                .text($.htmlEncode(this.options.updateUrlButtonCaption))
                .prop("title", this.options.updateUrlButtonTooltip);
            var savePageUrlHandler = $.proxy(this._savePageUrl, this);
            $button.on("click", savePageUrlHandler);
            this._$pageUrlInputInput.onEnter(savePageUrlHandler);
            this._$pageUrlContainer.append($button);
            this._$pageUrlContainer.removeClass().addClass("um-page-url-container-updatable");
        },

        _savePageUrl: function () {

            var path = this._$pageUrlInputInput.val();

            var postData = this.options.updateUrlDto;
            postData.Path = path;

            if (!this._updateCustomUrlServiceSettings) {
                this._updateCustomUrlServiceSettings = {
                    beforeSend: $.dnnSF().setModuleHeaders,
                    url: this._serviceUrl + "PageService/UpdateCustomUrl",
                    type: "POST",
                    async: true,
                    success: $.proxy(this._onUpdateUrl, this),
                    error: $.onAjaxError
                };
            }
            var serviceSettings = this._updateCustomUrlServiceSettings;
            serviceSettings.data = postData;
            $.ajax(serviceSettings);
        },

        _onUpdateUrl: function (data, textStatus, jqXhr) {
            if (data.Success) {
                this.$this.trigger("onUpdateUrl");
                this._$pageUrlInputInput.quickHighlight();
            }
            else {
                $.dnnAlert({ title: this.options.errorTitle || "Error", text: data.ErrorMessage || "Unknown error" });
                if (data.SuggestedUrlPath) {
                    this._$pageUrlInputInput.val(data.SuggestedUrlPath);
                }
            }
        }

    };

    PageUrl._defaults = {};

    PageUrl.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(PageUrl._defaults, settings);
        }
        return PageUrl._defaults;
    };



}).apply(dnn, [jQuery, window, document]);


dnn.createPageUrl = function (options) {
    $(document).ready(function () {
        var instance = new dnn.PageUrl(options);
        dnn.PageUrlSynchronizer.getInstance().setPageUrl(instance);
    });
};


(function ($, window, document, undefined) {
    "use strict";

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variables rather than globals
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).


    var PageUrlSynchronizer = this.PageUrlSynchronizer = dnn.singletonify(PageUrlSynchronizerInternal);


    function PageUrlSynchronizerInternal() {
        this.init();
    };

    PageUrlSynchronizerInternal.prototype = {
        constructor: PageUrlSynchronizerInternal,

        init: function () {
            this._$urlManagementInstance = null;
            this._$pageUrlInstance = null;
        },

        setPageUrl: function (instance) {
            if (!instance) {
                return;
            }
            this._$pageUrlInstance = $(instance);
            this._$pageUrlInstance.on("onUpdateUrl", $.proxy(this._onUpdateUrl, this));
            //if (this._urlManagementInstance) {
            instance.makeUpdatable();
            //}
        },

        _onUpdateUrl: function() {
            if (this._urlManagementInstance) {
                this._urlManagementInstance.refreshView();
            }
        },

        setUrlManagement: function (instance) {
            if (!instance) {
                return;
            }
            this._urlManagementInstance = instance;
            //if (this._$pageUrlInstance.length > 0) {
            //    this._$pageUrlInstance[0].makeUpdatable();
            //}
        }

    };

}).apply(dnn, [jQuery, window, document]);

