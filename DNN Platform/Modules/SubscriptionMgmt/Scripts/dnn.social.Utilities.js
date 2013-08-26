// DotNetNuke® - http://www.dotnetnuke.com
//
// Copyright (c) 2002-2013 DotNetNuke Corporation
// All rights reserved.

(function (dnn) {
    'use strict';

    // fix for IE8
    Object.keys = Object.keys || function(o){
        var result = [];
        for(var name in o){
            if(o.hasOwnProperty(name))
                result.push(name);
        }
        return result;
    };

    if (typeof dnn === 'undefined') dnn = {};
    if (typeof dnn.social === 'undefined') dnn.social = {};

    dnn.social.scopeWrapperLoading = function (scopeWrapperClientID) {
        // show loading screen before ko applied -- 
        // NOTES by RICHARD, MUST use vanila javascript, 
        // please no jQuery here, this function will be called before document ready

        var scopeWrapper = document.getElementById(scopeWrapperClientID);
        var scopeWrapperStyle = scopeWrapper.currentStyle || window.getComputedStyle(scopeWrapper);
        var height = scopeWrapper.offsetHeight + parseInt(scopeWrapperStyle.marginBottom, 10)
                        + parseInt(scopeWrapperStyle.marginTop, 10);
        var width = scopeWrapper.offsetWidth + parseInt(scopeWrapperStyle.marginLeft, 10)
                        + parseInt(scopeWrapperStyle.marginRight, 10);
        var scopeWrapperParent = scopeWrapper.parentNode;

        var scopeWrapperLoading = document.getElementById('scopeWrapperLoading');
        if (scopeWrapperLoading == null) {
            scopeWrapperLoading = document.createElement('div');
            scopeWrapperLoading.id = 'scopeWrapperLoading';
        }
        scopeWrapperLoading.style.height = height + 'px';
        scopeWrapperLoading.style.width = width + 'px';
        scopeWrapper.style.display = 'none';
        scopeWrapperParent.insertBefore(scopeWrapperLoading, scopeWrapper);
        scopeWrapperLoading.style.display = 'block';

        return scopeWrapperLoading;
    };

    dnn.social.scopeWrapperLoaded = function (scopeWrapperClientID) {
        var scopeWrapper = document.getElementById(scopeWrapperClientID);
        var prev = scopeWrapper.previousSibling;
        if (prev != null && prev.id == 'scopeWrapperLoading')
            prev.parentNode.removeChild(prev);

        scopeWrapper.style.display = 'block';
    };

    var supportedRGBA = null;
    dnn.social.supportedRGBA = function () {
        if (supportedRGBA == null) {
            var scriptElement = document.getElementsByTagName('script')[0];
            var prevColor = scriptElement.style.color;
            var testColor = 'rgba(0, 0, 0, 0.5)';
            if (prevColor == testColor) {
                supportedRGBA = true;
            }
            else {
                try {
                    scriptElement.style.color = testColor;
                } catch (e) { }
                supportedRGBA = scriptElement.style.color != prevColor;
                scriptElement.style.color = prevColor;
            }
        }
        return supportedRGBA;
    };

    dnn.social.flashOnElement = function (element, color, fallbackColor) {        
        if (dnn.social.supportedRGBA()) { // for moden browser, I use RGBA
            var color = color.join(',') + ',',
            transparency = 1,
            timeout = setInterval(function () {
                if (transparency >= 0) {
                    element.style.backgroundColor = 'rgba(' + color + (transparency -= 0.015) + ')';
                    // (1 / 0.015) / 25 = 2.66 seconds to complete animation
                } else {
                    clearInterval(timeout);
                }
            }, 40); // 1000/40 = 25 fps
        } else { // for IE8, I use hex color fallback
            element.style.backgroundColor = fallbackColor;
            setTimeout(function () {
                element.style.backgroundColor = 'transparent';
            }, 1000);
        }
    };

    dnn.social.namespace = function(ns, separator, container) {
        if (typeof container === 'undefined') {
            container = window;
        }

        var o = container;

        $.each(ns.split('.'),
            function (unused, component) {
                o = o[component] = o[component] || {};
            });

        return o;
    };

    dnn.social.EventQueue = {
        push: function (action) {
            return setTimeout(action, 0);
        },
        cancel: function (action) {
            clearTimeout(action);
        }
    };

    dnn.social.authorizationRequired = function (settings) {
        if (settings.anonymous) {
            if (settings.usePopup) {
                dnnModal.show(settings.authorizationUrl + '&popUp=true', true, 300, 650, true, '');
            }
            else {
                window.location = settings.authorizationUrl;
            }
            
            return true;
        }

        return false;
    };

    dnn.social.komodel = function (model) {
        var obj = {};

        if (typeof (model) !== 'undefined') {
            var convert =
                function (key, value) {
                    var k = dnn.social.toCamelCase(key);

                    if (value === null || typeof (value) === 'undefined') {
                        obj[k] = ko.observable();
                    } else if (value instanceof Array) {
                        var list = [];

                        $.each(value,
                            function (index, element) {
                                list.push(dnn.social.komodel(element));
                            });

                        obj[k] = ko.observableArray(list);
                    } else {
                        obj[k] = ko.observable(value);
                    }
                };

            if (model != null) {
                switch (typeof(model)) {
                    case 'undefined':
                        break;
                    case 'string':
                    case 'number':
                        obj = model;
                        break;
                    case 'object':
                        var keys = Object.keys(model);

                        $.each(keys,
                            function (unused, key) {
                                convert(key, model[key]);
                            });
                        
                        break;
                    case 'array':
                        obj = ko.observableArray(model);
                        break;
                    default:
                        console.log('Unknown object type ({0})'.format(typeof (model)));
                        break;
                }
            }
        }

        return obj;
    };

    dnn.social.toCamelCase = function (key) {
        if (typeof key === 'number') {
            return key;
        }
        
        var k = key[0].toLowerCase();

        if (key.length > 1) {
            return k + key.substr(1);
        }

        return k;
    };

    dnn.social.decodeHtml = function (s) {
        if ((s || new String()).length == 0) {
            return s;
        }

        return $('<div />').html(s.replace(/<br([ \/>]+)(<[ \/]+br>)?/g, "\n")).text();
    };

    String.prototype.format = function () {
        var s = this;
        var i = arguments.length;

        while (i--) {
            s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
        }

        return s;
    };
    
    dnn.social.setControlCursor = function (control, offset) {
        control.focus();

        var input = $(control)[0];

        if (input.createTextRange) {
            var range = input.createTextRange();

            range.collapse(true);
            range.moveEnd('character', offset);
            range.moveStart('character', offset);
            range.select();
        } else if (input.setSelectionRange) {
            input.setSelectionRange(offset, offset);
        }

        input.focus();
    };

    dnn.social.notImplemented = function () {
        $.dnnAlert({
            title: 'Not implemented',
            text: 'This has not been implemented yet'
        });
    };

    // Extensions to core JavaScript types

    String.isEmpty = function (s) {
        return s == null || s.length === 0;
    };

    Object.isNullOrUndefined = function (value) {
        return (typeof (value) === "undefined" || value === null);
    };

    String.isNullOrEmpty = function (value) {
        return (Object.isNullOrUndefined(value) || value === "");
    };

    if (typeof String.prototype.startsWith !== 'function') {
        String.prototype.startsWith = function (str) {
            return this.slice(0, str.length) === str;
        };
    }

    if (typeof Number.isPositiveInteger === "undefined") {
        Number.isPositiveInteger = function (value) {
            var isInteger = !isNaN(value) && isFinite(value);
            if (typeof value === 'string') {
                isInteger = isInteger && (value.indexOf('.') === -1) && (value.indexOf(',') === -1);
            }
            else {
                isInteger = isInteger && (typeof value === 'number' && value % 1 === 0);
            }
            return isInteger && (value - 0) > 0;
        };
    };

    // Compatibility methods for browsers that lack some standard utilities
    
    if (typeof (Object.prototype.hasOwnProperty) === 'undefined') {
        // The behaviour of this method is not identical to the standard but it's a close enough approximation.
        // The primary difference is that this one doesn't care where in an inheritance tree a particular value
        // was defined, only that it was defined; the real hasOwnProperty looks only at the topmost level of the
        // inheritance tree.
        Object.prototype.hasOwnProperty = function (k) {
            return Array.indexOf(Object.keys(this) || [], k) >= 0;
        };
    };

    if (typeof String.toCamel === "undefined") {
        String.toCamel = function (text) {
            return String.isNullOrEmpty(text) ? text : (text.charAt(0).toLowerCase() + text.substr(1));
        };
    };

    if (typeof Object.ToCamel === "undefined") {
        Object.ToCamel = function (o) {
            var camelizedObject = {};
            var propertyValue;
            for (var propertyName in o) {
                if (Object.prototype.hasOwnProperty.call(o, propertyName) && typeof (propertyName) === "string") {
                    propertyValue = o[propertyName];
                    if (typeof (propertyValue) === "object" && propertyValue !== null && propertyValue.constructor !== Array) {
                        propertyValue = Object.ToCamel(propertyValue);
                    }
                    camelizedObject[String.toCamel(propertyName)] = propertyValue;
                }
            }
            return camelizedObject;
        };
    };

})(window.dnn);