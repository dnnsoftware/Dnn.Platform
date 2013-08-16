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
	else if (String.isNullOrEmpty(this)) {
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

if (typeof Array.prototype.insertAt !== "function") {
    Array.prototype.insertAt = function (index) {
        this.splice.apply(this, [index, 0].concat(
            Array.prototype.slice.call(arguments, 1)));
        return this;
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
            if (instance == null) {
                instance = new constructorFunc(args);
                instance.constructor = null;
            }
            return instance;
        };
    };
};
