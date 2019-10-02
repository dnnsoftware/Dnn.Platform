define(['jquery'], function ($) {
    'use strict';
    
    /*
     * This module is responsible to manage all the main Persona Bar events
     * (i.e.: open panel, close panel). 
     * With this module consumers can subscribe, unsubscribe to events and emit events
     */
    var eventEmitter = {
        addOpenEventListener: function (callback) {
            $(document).on('editbar:onOpen', callback);
        },
        removeOpenEventListener: function (callback) {
            $(document).off('editbar:onOpen', callback);
        },
        addCloseEventListener: function (callback) {
            $(document).on('editbar:onClose', callback);
        },
        removeCloseEventListener: function (callback) {
            $(document).off('editbar:onClose', callback);
        }
    };
    return eventEmitter;
});