define(['jquery'], function ($) {
    'use strict';
    
    /*
     * This module is responsible to manage all the main Persona Bar events
     * (i.e.: open panel, close panel). 
     * With this module consumers can subscribe, unsubscribe to events and emit events
     */
    var eventEmitter = {
        addPanelOpenEventListener: function addPanelOpenEventListener(callback) {
            $(document).on('personabar:onOpenPanel', callback);
        },
        removePanelOpenEventListener: function removePanelOpenEventListener(callback) {
            $(document).off('personabar:onOpenPanel', callback);
        },
        addPanelCloseEventListener: function addPanelCloseEventListener(callback) {
            $(document).on('personabar:onClosePanel', callback);
        },
        removePanelCloseEventListener: function removePanelCloseEventListener(callback) {
            $(document).off('personabar:onClosePanel', callback);
        },
        emitOpenPanelEvent: function emitOpenPanelEvent() {
            $(document).trigger('personabar:onOpenPanel');
        },
        emitClosePanelEvent: function emitClosePanelEvent() {
            $(document).trigger('personabar:onClosePanel');
        }
    };
    return eventEmitter;
});