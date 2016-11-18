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
        * Subscribes to the panel open event
        *
        * @method onPanelOpen
        * @param {Function} callback callback called when the event happens
        */
        this.onPanelOpen = function onPanelOpen(callback) {
            eventEmitter.addPanelOpenEventListener(callback);
        };

        /**
        * Unsubscribes to the panel open event
        *
        * @method offPanelOpen
        * @param {Function} callback callback called when the event happens
        */
        this.offPanelOpen = function offPanelOpen(callback) {
            eventEmitter.removePanelOpenEventListener(callback);
        };

        /**
        * Subscribes to the panel close event
        *
        * @method onPanelClose
        * @param {Function} callback callback called when the event happens
        */
        this.onPanelClose = function onPanelClose(callback) {
            eventEmitter.addPanelCloseEventListener(callback);
        };

        /**
        * Unsubscribes to the panel close event
        *
        * @method onPanelClose
        * @param {Function} callback callback called when the event happens
        */
        this.offPanelClose = function offPanelClose(callback) {
            eventEmitter.removePanelCloseEventListener(callback);
        };

        /**
        * Opens the persona bar panel
        *
        * @method openPanel
        * @param {String} path identifier of the path
        * @param {Object} params parameters for the panel
        */
        this.openPanel = function openPanel(path, params) {
            if (!params.identifier) {
                params.identifier = path;
            }

            if (!params.settings) {
                params.settings = util.findMenuSettings(params.identifier);
            }
            params.path = path;
            util.loadPanel(path, params);
        };

        /**
        * Closes the persona bar panel
        *
        * @method closePanel
        */
        this.closePanel = function closePanel() {
            util.closePersonaBar();
        };
    };
});