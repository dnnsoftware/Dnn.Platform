/*
 * DotNetNuke® - http://www.dotnetnuke.com
 * Copyright (c) 2002-2020
 * by DotNetNuke Corporation
 * All Rights Reserved
 */

/***
 * @class Localization
 *
 * Check if window storage is avaible, get resources settings, check resources timestamp, return sotred resources
 * or request for new resources, store it if can and return resources
 *
 * @depends dnn.jquery.js
 *
 * @param {object} options: {
 *  {string} service
 *  {string} controller
 *  {string} resxName, name for store data, normally MyResourcesResx
 *
 *  {object} resourceSettings: {
 *      {object} local, if defined, the remote request is not needed and the values for culture an timestamp are set here.
 *      {
 *          {string} culture, if defined, contains the current culture, so the 
 *          {string} resxTimeStamp, if defined, contains the current culture
 *      },
 *      {string} method, method name to obtain resource settings
 *      {string} methodType (get), get or post
 *      {object} data (null), add parameters to request (get or post)
 *      {string} paramNames: { name of parameters returned by the server
 *          {string} culture (culture), name for culture parameter
 *          {string} resxTimeStamp (resxTimeStamp), name for timestamp parameter
 *      },
 *      {function} callback200(data) (null), callback to call after resource settings request for status 200,
                                     data will return an object {culture: 'culture', resxTimeStamp: number}
 *      {function} callbackError (null), callback to call after resource settigs request for status not 200
 *  },
 *
 *  {object} resources: {
 *      {string} method: method name to obtain resources
 *      {string} methodType (get), get or post
 *      {object} data (null), add parameters to request (get or post)
 *      {string} paramName (localization), name of parameter for returned data (if null, then resx are directly in the response).
 *      {function} callback200(data) (null), callback to call after resource request or storage load for status 200,
 *                                     normally you want use this callback to assign your resources,
                                       data will return an object {localization: {resource_name: 'resource value'}}
 *      {function} callbackError (null), callback to call after resource request or storage load for status not 200
 *  }
 * }
 *
 */

// Namespace
window.dnn = window.dnn || {};
window.dnn.utils = window.dnn.utils || {};

if (typeof window.dnn.utils.Localization === "undefined") {
    (function IIFE() {
        var LocalizationClass;

        LocalizationClass = (function IIFE() {
            'use strict';

            //var this.options, this.resourceSettings;

            var detectStorage,
                getStoredData,
                setStoredData,
                compareTimeStamp,
                getServiceUrl,
                getResourcesSettings,
                getResourcesSettings200,
                getResources,
                getResources200,
                updateCulture,
                copyOtherSettings;

            /* Class properties */
            Localization["class"] = 'Localization';
            Localization.type = 'Class';
            Localization.storageAllowed = false;

            /* Private Constants and Properties */
            //this.options = {};

            /* Constructor */
            function Localization(options) {
                //console.log('~Localization');
                this.options = {};

                // Defaults
                if (!options.service || typeof options.service !== 'string') throw 'No service provided';
                this.options.service = options.service;

                if (!options.controller || typeof options.controller !== 'string') throw 'No controller provided';
                this.options.controller = options.controller;

                if (!options.resxName) throw 'No resxName provided';
                this.options.resxName = options.resxName;

                this.options.culture = options.culture || '';

                this.options.key = this.options.resxName + this.options.culture;
                this.options.keyTS = this.options.resxName + 'TimeStamp' + this.options.culture;

                this.options.key = this.options.resxName;
                this.options.keyTS = this.options.resxName + 'TimeStamp' + this.options.culture;

                if (this.options.culture !== '') {
                    this.options.key += '.' + this.options.culture;
                    this.options.keyTS += '.' + this.options.culture;
                }

                // Resource Settings
                if (!options.resourceSettings || typeof options.resourceSettings !== 'object')
                    throw 'No resourceSettings provided';
                this.options.resourceSettings = options.resourceSettings;

                if (this.options.resourceSettings.local) {
                    if (!this.options.resourceSettings.local.culture ||
                        !this.options.resourceSettings.local.resxTimeStamp) {
                        throw 'No local resourceSettings provided';
                    }
                    options.resourceSettings.paramNames = options.resourceSettings.paramNames || {};
                } else {
                    if (!options.resourceSettings.method || typeof options.resourceSettings.method !== 'string')
                        throw 'No method for get resource settings provided';
                    this.options.resourceSettings.method = options.resourceSettings.method;

                    this.options.resourceSettings.methodType = options.resourceSettings.methodType || 'get';

                    if (!options.resourceSettings.paramNames ||
                        typeof options.resourceSettings.paramNames !== 'object') {
                        options.resourceSettings.paramNames = {};
                    }

                    this.options.resourceSettings.params = this.options.resourceSettings.params || {};
                    this.options.resourceSettings.callback200 = options.resourceSettings.callback200 || null;
                    this.options.resourceSettings.callbackError = options.resourceSettings.callbackError || null;
                }
                options.resourceSettings.paramNames.culture = options.resourceSettings.paramNames.culture || 'culture';
                options.resourceSettings.paramNames.resxTimeStamp =
                    options.resourceSettings.paramNames.resxTimeStamp || 'resxTimeStamp';
                this.options.resourceSettings.paramName = options.resourceSettings.paramNames;

                // Resources
                if (!options.resources || typeof options.resources !== 'object') throw 'No resources provided';
                this.options.resources = options.resources;

                if (!options.resources.method || typeof options.resources.method !== 'string')
                    throw 'No method for get resource settings provided';
                this.options.resources.method = options.resources.method;

                this.options.resources.methodType = options.resources.methodType || 'get';

                this.options.resources.params = this.options.resources.params || {};
                this.options.resources.callback200 = options.resources.callback200 || null;
                this.options.resources.callbackError = options.resources.callbackError || null;

                detectStorage.call(this);
                if (this.options.resourceSettings.local) {
                    getResourcesSettings200.call(this, this.options.resourceSettings.local);
                } else {
                    getResourcesSettings.call(this);
                }
            }

            /* Private Methods */
            detectStorage = function() {
                var fail, uid;
                try {
                    uid = new Date();
                    (Localization.storageAllowed = window.localStorage).setItem(uid, uid);
                    fail = Localization.storageAllowed.getItem(uid) !== uid + '';
                    Localization.storageAllowed.removeItem(uid);
                    Localization.storageAllowed = fail ? false : true;
                } catch (ex) {
                    Localization.storageAllowed = false;
                }
            };

            getStoredData = function() {

                var stored;
                if (!Localization.storageAllowed) return false;

                try {
                    stored = JSON.parse(window.localStorage.getItem(this.options.key));
                } catch (err) {
                    stored = {};
                    window.localStorage.removeItem(this.options.key);
                }
                return {
                    storedTS: window.localStorage.getItem(this.options.keyTS),
                    stored: stored
                };
            };

            setStoredData = function(stored, storedTS) {
                if (!Localization.storageAllowed) return false;

                try {
                    window.localStorage.setItem(this.options.key, JSON.stringify(stored));
                    window.localStorage.setItem(this.options.keyTS, storedTS);
                } catch (ex) {
                    if (ex.name === "QuotaExceededError") {
                        console.log("Local Storage might be full.");
                    }
                    console.log(ex);
                    if (window.localStorage[this.options.key]) {
                        window.localStorage.removeItem(this.options.key);
                    }
                    if (window.localStorage[this.options.keyTS]) {
                        window.localStorage.removeItem(this.options.keyTS);
                    }
                    return false;
                }
            };

            compareTimeStamp = function(storedTS) {
                if (!Localization.storageAllowed) return false;
                return getStoredData.call(this).storedTS + '' === storedTS + '';
            };

            getServiceUrl = function(method) {
                return $.dnnSF().getServiceRoot(this.options.service) + this.options.controller + '/' + method;
            };

            getResourcesSettings = function() {
                var self = this;
                $.ajax({
                    url: getServiceUrl.call(self, this.options.resourceSettings.method),
                    type: this.options.resourceSettings.methodType,
                    data: this.options.resourceSettings.data,
                    beforeSend: $.dnnSF().setModuleHeaders,
                    complete: null, // Study this case
                    success: function(data) {
                        getResourcesSettings200.call(self, data);
                    },
                    error: this.options.resourceSettings.callbackError
                });
            };

            copyOtherSettings = function(settings, otherSettings) {
                return $.extend(settings, otherSettings);
            };

            getResourcesSettings200 = function(data) {

                this.resourceSettings = {
                    culture: null,
                    resxTimeStamp: null
                };

                // Not browser storage allowed and no callback for this request
                if (!Localization.storageAllowed && typeof this.options.resourceSettings.callback200 !== 'function') {
                    this.resourceSettings.culture = data[this.options.resourceSettings.paramNames.culture];
                    updateCulture.call(this, this.resourceSettings.culture);
                    getResources.call(this);
                    return;
                }

                if (!data[this.options.resourceSettings.paramNames.culture]) {
                    throw 'No culture from server';
                }
                if (!data[this.options.resourceSettings.paramNames.resxTimeStamp]) {
                    throw 'No resxTimeStamp from server';
                }

                this.resourceSettings.culture = data[this.options.resourceSettings.paramNames.culture];
                updateCulture.call(this, this.resourceSettings.culture);
                this.resourceSettings.resxTimeStamp = data[this.options.resourceSettings.paramNames.resxTimeStamp];

                this.resourceSettings = copyOtherSettings(this.resourceSettings, data);

                if (typeof this.options.resourceSettings.callback200 === 'function') {
                    this.options.resourceSettings.callback200(this.resourceSettings);

                    // Not browser storage allowed but callback for this request: callback and getResources
                    if (!Localization.storageAllowed) {
                        getResources.call(this);
                        return;
                    }
                }

                // Return data from local or get new
                if (this.resourceSettings.culture && this.resourceSettings.resxTimeStamp) {
                    if (compareTimeStamp.call(this, this.resourceSettings.resxTimeStamp)) {
                        if (typeof this.options.resources.callback200 === 'function')
                            this.options.resources.callback200({ localization: getStoredData.call(this).stored });
                    } else {
                        getResources.call(this);
                    }
                } else {
                    getResources.call(this);
                }

                if (typeof this.options.resourceSettings.callback200 === 'function') {
                    this.options.resourceSettings.callback200(this.resourceSettings);
                }
            };

            getResources = function() {
                var self = this;
                this.options.resources.data = this.options.resources.data || {};
                this.options.resources.data.culture = this.resourceSettings.culture;

                $.ajax({
                    url: getServiceUrl.call(self, this.options.resources.method),
                    type: this.options.resources.methodType,
                    data: this.options.resources.data,
                    beforeSend: $.dnnSF().setModuleHeaders,
                    complete: null, // Study this case
                    success: function(data) {
                        getResources200.call(self, data);
                    },
                    error: this.options.resources.callbackError
                });
            };

            // If called, need store data
            getResources200 = function(data) {
                var resData;
                resData = { localization: null };

                if (this.options.resources.paramName) {
                    if (!data[this.options.resources.paramName]) {
                        throw 'No localization from server';
                    } else {
                        resData.localization = data[this.options.resources.paramName];
                    }
                } else if (!data) {
                    throw 'No localization from server';
                } else {
                    resData.localization = data;
                }

                if (resData.localization && this.resourceSettings.resxTimeStamp)
                    setStoredData.call(this, resData.localization, this.resourceSettings.resxTimeStamp);

                if (typeof this.options.resources.callback200 === 'function')
                    this.options.resources.callback200(resData);
            };

            updateCulture = function(culture) {
                if (culture != this.options.culture) {
                    this.options.culture = this.resourceSettings.culture;
                }
                this.options.key = this.options.resxName + '.' + this.options.culture;
                this.options.keyTS = this.options.resxName + 'TimeStamp.' + this.options.culture;
            };

            /* test-code */
            /* end-test-code */

            return Localization;
        })();

        window.dnn.utils.Localization = LocalizationClass;

    }).call(this);
}
