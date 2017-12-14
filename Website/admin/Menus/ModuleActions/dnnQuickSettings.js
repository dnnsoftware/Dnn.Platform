(function($) {
    "use strict";
    /*
    * Creates a new quick setting object.
    *
    * @method dnnQuickSettings
    * @param {Object} options Options needed to create a new quick settings.
    * @param {int} options.moduleId Module identifier associated to the quick setting.
    * @param {function} options.onSave A callback function that will executed when save button is clicked. This function MUST
    *                           returns a promise (see https://api.jquery.com/category/deferred-object/) in order the quick module 
    *                           object is allowed to close menu once the save callback action has finished.
    * @param {function} options.onCancel A callback function that will executed when cancel button is clicked. This function MUST
    *                           returns a promise (see https://api.jquery.com/category/deferred-object/) in order the quick module 
    *                           object is allowed to close menu once the cancel callback action has finished.
    * @return {Object} Returnsa quick setting object.
    */
    $.fn.dnnQuickSettings = function (options) {
        var opts = $.extend({}, $.fn.dnnQuickSettings.defaultOptions, options);
        var onCancel = opts.onCancel;
        var onSave = opts.onSave;

        var $self = this;
        var moduleId = opts.moduleId;

        var $container = $("#moduleActions-" + moduleId + "-QuickSettings");
        var $saveButton = $container.find(".qsFooter a.primarybtn");
        var $cancelButton = $container.find("a.secondarybtn");

        var closeMenu = function (ul) {
            var $menuRoot = $('#moduleActions-' + moduleId + ' ul.dnn_mact');
            $menuRoot.removeClass('showhover').data('displayQuickSettings', false);
            if (ul && ul.position()) {
                if (ul.position().top > 0) {
                    ul.hide('slide', { direction: 'up' }, 80, function () {
                        dnn.removeIframeMask(ul[0]);
                    });
                } else {
                    ul.hide('slide', { direction: 'down' }, 80, function () {
                        dnn.removeIframeMask(ul[0]);
                    });
                }
            }
        }

        var throwErrorWhenInvalidPromise = function checkPromiseHandler(promise, callbackName) {
            if (!promise || typeof promise !== 'object') {
                throw "The '" + callbackName + "' callback should return a promise.";
            }

            if (typeof promise.done !== 'function') {
                throw "The '" + callbackName + "' callback should return a promise with a valid 'done' function.";
            }
        };

        $cancelButton.click(function () {
            if (typeof onCancel !== "function") {
                throw "The 'onCancel' callback must be a function";
            }

            var promise = onCancel.call(this);
            throwErrorWhenInvalidPromise(promise, "onCancel");

            promise.done(
                function () {
                    closeMenu($container.parent());
                }
            );
        });

        $saveButton.click(function () {
            if (typeof onSave !== "function") {
                throw "The 'onSave' callback must be a function";
            }

            var promise = onSave.call(this);
            throwErrorWhenInvalidPromise(promise, "onSave");

            promise.done(
                function () {
                    closeMenu($container.parent());
                }
            );
        });

        return $self;
    }

    $.fn.dnnQuickSettings.defaultOptions = {
        moduleId: -1,
        onCancel: function () {
            var deferred = $.Deferred();
            deferred.resolve();
            return deferred.promise();
        },
        onSave: function () {
            var deferred = $.Deferred();
            deferred.resolve();
            return deferred.promise();
        },
        quickSettingsDispatcher: $.fn.dnnQuickSettings.quickSettingsDispatcher
    };

    $.fn.dnnQuickSettings.quickSettingsDispatcher = (function () {
        var addSubcriber, notify;
        var subcribers;

        var EVENT_TYPES = {
            SAVE: "SAVE",
            CANCEL: "CANCEL"
        };

        subcribers = {}

        addSubcriber = function addSubcriberHandler(moduleId, eventType, callback) {
            if (!EVENT_TYPES[eventType]) {
                throw "QuickSettingsDispatcher - Subcritions to event type '" + eventType + "' not supported.";
            }

            if (!subcribers[moduleId]) {
                subcribers[moduleId] = {};
            }

            subcribers[moduleId][eventType] = callback;
        };

        notify = function notifyHandler(moduleId, eventType, params) {
            if (!EVENT_TYPES[eventType]) {
                throw "QuickSettingsDispatcher - Notifications to event type '" + eventType + "' not supported.";
            }

            if (subcribers[moduleId] && subcribers[moduleId][eventType]
                && typeof subcribers[moduleId][eventType] == "function") {
                subcribers[moduleId][eventType](params);
            }
        };

        return {
            addSubcriber: addSubcriber,
            notify: notify,
            eventTypes: EVENT_TYPES
        };
    }());

})(jQuery);

