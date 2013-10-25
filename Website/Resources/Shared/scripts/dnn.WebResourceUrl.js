(function ($, window, document, undefined) {
    "use strict";

    var WebResourceUrl = this.WebResourceUrl = function (pathAndQuery) {
        this._path = "";
        this._pathAndQuery = pathAndQuery;
        this._parameters = new dnn.Dictionary();
        this._isInitialized = false;
    };

    WebResourceUrl.prototype = {
        constructor: WebResourceUrl,

        path: function() {
            this._ensureInitialize();
            return this._path;
        },

        parameters: function() {
            this._ensureInitialize();
            return this._parameters;
        },

        toPathAndQuery: function () {
            var pathAndQuery = this.path().append(WebResourceUrl.toQueryString(this.parameters()), "?");
            return pathAndQuery;
        },

        _ensureInitialize: function () {
            if (!this._isInitialized) {
                this._initialize(this._pathAndQuery);
                this._isInitialized = true;
            }
        },

        _initialize: function(pathAndQuery) {
            this._parameters = new dnn.Dictionary();
            this._path = "";

            if (String.isNullOrEmpty(pathAndQuery) || typeof(pathAndQuery) !== "string") {
                return;
            }
            var parts = pathAndQuery.split("?");
            this._path = parts[0];
            if (parts.length > 1) {
                parts = parts[1].split("&");
                if (parts.length > 0) {
                    for (var i = 0, size = parts.length; i < size; i++) {
                        var keyValuePair = parts[i].split("=");
                        this._parameters.set(keyValuePair[0], WebResourceUrl.decodeParameterValue(keyValuePair[1]));
                    }
                }
            }
        }
    };

    WebResourceUrl.toQueryString = function(dictionary) {
        var queryString = "";
        var key;
        var value;
        if (!dictionary) {
            return queryString;
        }
        var keys = dictionary.keys();

        for (var i = 0, size = keys.length; i < size; i++) {
            key = keys[i];
            value = dictionary.get(key);
            if (!Object.isNullOrUndefined(value)) {
                queryString = queryString.append(key + "=" + WebResourceUrl.encodeParameterValue(value), "&");
            }
        }
        return queryString;
    };

    WebResourceUrl.decodeParameterValue = function (encodedValue) {
        // Space is encoded as + on server but decodeURIComponent doesn't decode + back.
        // So we need to restore it before decoding.
        // Create a regular expression to search all +s in the string.
        var lsRegExp = /\+/g;
        // Return the decoded string
        return decodeURIComponent(String(encodedValue).replace(lsRegExp, " "));
    };

    WebResourceUrl.encodeParameterValue = function(decodedValue) {
        // So encodeURIComponent encodes + correctly 
        // we don't need workaround as in DecodeParameterValue.
        return encodeURIComponent(decodedValue);
    };

}).apply(dnn, [jQuery, window, document]);
