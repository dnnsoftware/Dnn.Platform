var dnn = dnn || {};
dnn.modules = dnn.modules || {};
dnn.modules.spa = dnn.modules.spa || {};
dnn.modules.spa.dnnContactListSpa = dnn.modules.spa.dnnContactListSpa || {};

dnn.modules.spa.dnnContactListSpa.quickSettingsDispatcher = (function () {
    "use strict";
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
