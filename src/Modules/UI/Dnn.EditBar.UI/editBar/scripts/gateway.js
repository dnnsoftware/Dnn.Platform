define(['./eventEmitter'], function (eventEmitter) {
    'use strict';
    
    /*
     * This module is responsible to expose an Interface for the Persona Bar.
     * This interface is exposed to the parent window (the main site) in order
     * to allow easy interaction and integration between the PB and the site (i.e.: Edit Bar).
     * Consumers can subscribe/unsubscribe to events like open, close panels.
     */
    return function Gateway(util) {
        /**
        * Subscribes to the edit bar open event
        *
        * @method onOpen
        * @param {Function} callback callback called when the event happens
        */
        this.onOpen = function (callback) {
            eventEmitter.addOpenEventListener(callback);
        };

        /**
        * Unsubscribes to the edit bar open event
        *
        * @method offOpen
        * @param {Function} callback callback called when the event happens
        */
        this.offOpen = function (callback) {
            eventEmitter.removeOpenEventListener(callback);
        };

        /**
        * Subscribes to the edit bar close event
        *
        * @method onClose
        * @param {Function} callback callback called when the event happens
        */
        this.onClose = function (callback) {
            eventEmitter.addCloseEventListener(callback);
        };

        /**
        * Unsubscribes to the edit bar close event
        *
        * @method offClose
        * @param {Function} callback callback called when the event happens
        */
        this.offClose = function (callback) {
            eventEmitter.removeCloseEventListener(callback);
        };

        /**
        * trigger menu item's action
        *
        * @method action
        * @param {String} path identifier of the path
        * @param {Object} params parameters for the panel
        */
        this.action = function (menuName, actionName, params) {
            util.callAction(menuName, actionName, params);
        };
    };
});