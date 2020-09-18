Object.isNullOrUndefined = function (value) {
	return (typeof (value) === "undefined" || value === null);
};

String.isNullOrEmpty = function (value) {
	return (Object.isNullOrUndefined(value) || value === "");
};

String.toBoolean = function(value) {
    if (typeof value === "string") {
        var str = value.toLowerCase();
        return str === "true" || str === "yes" || str === "on" || str === "1";
    }
    return value === true || value === 1;
},

String.Empty = "";

Object.IsArray = function (obj) {
    if (obj && typeof obj === 'object' && obj.constructor === Array) {
        return true;
    }
    return false;
};

if (typeof Object.getOwnPropertyNames === "undefined") {
    Object.getOwnPropertyNames = function(obj) {
        var ownProperties = [];
        for (var key in obj) {
            if (obj.hasOwnProperty && obj.hasOwnProperty(key)) {
                ownProperties.push(key);
            }
        }
        return ownProperties;
    };
};

if (typeof String.toCamel === "undefined") {
	String.toCamel = function (text) {
		return String.isNullOrEmpty(text) ? text : (text.charAt(0).toLowerCase() + text.substr(1));
	};
};

if (typeof Object.ToCamel === "undefined") {
    Object.ToCamel = function(o) {
        var camelizedObject;
        if (Object.IsArray(o)) {
            camelizedObject = [];
            for (var i = 0, size = o.length; i < size; i++) {
                camelizedObject.push(Object.ToCamel(o[i]));
            }
        }
        else {
            camelizedObject = {};
            var propertyValue;
            for (var propertyName in o) {
                if (Object.prototype.hasOwnProperty.call(o, propertyName) && typeof(propertyName) === "string") {
                    propertyValue = o[propertyName];
                    if (typeof(propertyValue) === "object" && propertyValue !== null) {
                        propertyValue = Object.ToCamel(propertyValue);
                    }
                    camelizedObject[String.toCamel(propertyName)] = propertyValue;
                }
            }
        }
        return camelizedObject;
    };
};

String.prototype.append = function (stringToAppend, separator) {
	if (String.isNullOrEmpty(stringToAppend)) {
		return this.toString();
	}
	else if (String.isNullOrEmpty(this.toString())) {
		return stringToAppend;
	}
	else {
		var result = [];
		result[0] = this;
		result[1] = stringToAppend;
		return result.join(separator);
	}
};

if (typeof String.prototype.trim !== 'function') {
	String.prototype.trim = function() { return this.replace( /^\s\s*/ , '').replace( /\s\s*$/ , ''); };
}

if (typeof String.prototype.ltrim !== 'function') {
	String.prototype.ltrim = function() { return this.replace( /^\s+/ , ''); };
}

if (typeof String.prototype.rtrim !== 'function') {
	String.prototype.rtrim = function() { return this.replace( /\s+$/ , ''); };
}

if (typeof String.prototype.fulltrim !== 'function') {
	String.prototype.fulltrim = function() { return this.replace( /(?:(?:^|\n)\s+|\s+(?:$|\n))/g , '').replace( /\s+/g , ' '); };
}

if (typeof String.prototype.startsWith !== 'function') {
    String.prototype.startsWith = function (str) {
        return this.slice(0, str.length) === str;
    };
}

if (typeof String.prototype.endsWith !== 'function') {
    String.prototype.endsWith = function (suffix) {
        return this.indexOf(suffix, this.length - suffix.length) !== -1;
    };
}

if (typeof Array.prototype.insertAt !== "function") {
    Array.prototype.insertAt = function (index) {
        this.splice.apply(this, [index, 0].concat(
            Array.prototype.slice.call(arguments, 1)));
        return this;
    };
}

if (typeof Array.prototype.peek !== "function") {
    Array.prototype.peek = function () {
        return this[this.length - 1];
    };
}

if (!Array.prototype.indexOf) {
    Array.prototype.indexOf = function(searchElement /*, fromIndex */) {
        if (this == null) {
            throw new TypeError();
        }
        var t = Object(this);
        var len = t.length >>> 0;
        if (len === 0) {
            return -1;
        }
        var n = 0;
        if (arguments.length > 1) {
            n = Number(arguments[1]);
            if (n != n) { // shortcut for verifying if it's NaN
                n = 0;
            } else if (n != 0 && n != Infinity && n != -Infinity) {
                n = (n > 0 || -1) * Math.floor(Math.abs(n));
            }
        }
        if (n >= len) {
            return -1;
        }
        var k = n >= 0 ? n : Math.max(len - Math.abs(n), 0);
        for (; k < len; k++) {
            if (k in t && t[k] === searchElement) {
                return k;
            }
        }
        return -1;
    };
}

if (typeof dnn === "undefined" || dnn === null) { dnn = {}; }; //var dnn = dnn || {};

dnn.Enum = function (keyValuePairs) {
    for (var i in keyValuePairs) {
        if (Object.prototype.hasOwnProperty.call(keyValuePairs, i)) {
            this[keyValuePairs[i].value] = keyValuePairs[i].key;
        }
    }
};

dnn.Enum.prototype.getEnumName = function(enumValue) {
    for (var propertyName in this) {
        if (Object.prototype.hasOwnProperty.call(this, propertyName) && enumValue === this[propertyName]) {
            return propertyName;
        }
    }
    return "";
};

dnn.SortOrder = new dnn.Enum([{ key: 0, value: "unspecified" }, { key: 1, value: "ascending" }, { key: 2, value: "descending" }]);

dnn.executeFunctionByName = function(functionName, context /*, args */) {
    var args = Array.prototype.slice.call(arguments, 2);
    var namespaces = functionName.split(".");
    var func = namespaces.pop();
    for (var i = 0, size = namespaces.length; i < size; i++) {
        context = context[namespaces[i]];
    }
    if (typeof context[func] === "function") {
        return context[func].apply(context, args);
    }
    return null;
};

dnn.singletonify = function(constructorFunc /*, args */) {
    var instance = null;
    var args = Array.prototype.slice.call(arguments, 1);

    return new function () {
        this.getInstance = function () {
            var instanceArguments;
            var f;
            if (instance == null) {
                instanceArguments = Array.prototype.slice.call(arguments);
                f = function() {
                    return constructorFunc.apply(this, [].concat(args, instanceArguments));
                };
                f.prototype = constructorFunc.prototype;
                instance = new f();
            }
            return instance;
        };
    };
};

dnn.derive = function(child, parent) {
    var tempConstructor = function() {};
    tempConstructor.prototype = parent.prototype;
    child.prototype = new tempConstructor();
    child.prototype.constructor = child;
    child.superclass = parent.prototype;
};

(function ($, window, document, undefined) {
    "use strict";

    var KeyValueConverter = this.KeyValueConverter = function () {
    };

    KeyValueConverter.arrayToDictionary = function (pairs, keyProp, valProp) {
        var dictionary = new dnn.Dictionary();
        if (pairs && pairs.length > 0) {
            for (var i = 0, size = pairs.length; i < size; i++) {
                dictionary.set(pairs[i][keyProp], pairs[i][valProp]);
            }
        }
        return dictionary;
    };

    KeyValueConverter.dictionaryToArray = function (dictionary, keyProp, valProp) {
        var pairs = [];
        if (dictionary) {
            for (var key in dictionary) {
                var pair = {};
                pair[keyProp] = key;
                pair[valProp] = dictionary[key];
                pairs.push(pair);
            }
        }
        return pairs;
    };

    KeyValueConverter.getKeyValuePairByKey = function (pairs, key) {
        if (!pairs) {
            return null;
        }
        for (var i = 0, size = pairs.length; i < size; i++) {
            if (pairs[i].key === key) {
                return pairs[i];
            }
        }
        return null;
    };

}).apply(dnn, [jQuery, window, document]);

if (typeof Date.prototype.format === "undefined") {
    Date.prototype.format = function(pattern) {
        var hours = this.getHours();
        var ttime = "AM";
        if (pattern.indexOf("t") > -1 && hours > 12) {
            hours = hours - 12;
            ttime = "PM";
        }

        var o = {
            "M+": this.getMonth() + 1, //month
            "d+": this.getDate(), //day
            "h+": hours, //hour
            "m+": this.getMinutes(), //minute
            "s+": this.getSeconds(), //second
            "q+": Math.floor((this.getMonth() + 3) / 3), //quarter
            "S": this.getMilliseconds(), //millisecond,
            "t+": ttime
        };

        if (/(y+)/.test(pattern)) {
            pattern = pattern.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
        }
        for (var k in o) {
            if (new RegExp("(" + k + ")").test(pattern)) {
                pattern = pattern.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
            }
        }
        return pattern;
    };
}

/* Compare the current date against another date.
     *
     * @param b  {Date} the other date
     * @returns   -1 : if this < b
     *             0 : if this === b
     *             1 : if this > b
     *            NaN : if a or b is an illegal date
    */
if (typeof Date.prototype.compare === "undefined") {
    Date.prototype.compare = function (b) {
        if (b.constructor !== Date) {
            throw "invalid_date";
        }
        return (isFinite(this.valueOf()) && isFinite(b.valueOf()) ?
                    (this > b) - (this < b) : NaN);
    };
}

//Returns true if it is a DOM element
if (typeof Object.isElement === "undefined") {
    Object.isElement = function (o) {
        return typeof HTMLElement === "object" ?
                o instanceof HTMLElement : //DOM2
                o && typeof o === "object" && o.nodeType === 1 && typeof o.nodeName === "string";
    };
}

dnn.removeElement = function (element) {
    if (element && element.parentNode) {
        element.parentNode.removeChild(element);
    }
};

/*
 * Generates a GUID string, according to RFC4122 standards.
 * @returns {String} The generated GUID like "af8a8416-6e18-a307-bd9c-f2c947bbb3aa"
 */
dnn.guid = (function() {
    var partOf8 = function (dashed) {
        var part = (Math.random().toString(16) + "000000000").substr(2, 8);
        return dashed ? "-" + part.substr(0, 4) + "-" + part.substr(4, 4) : part;
    };
    return function () { return partOf8(false) + partOf8(true) + partOf8(true) + partOf8(false); };
})();

dnn.uid = (function () {
    var id = (new Date()).getTime();
    return function (prefix) {
        return (prefix || "id") + (id++);
    };
})();

dnn.isUrl = function(url) {
    var regexp = /(ftp|http|https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/;
    return regexp.test(url);
};
