// Enhanced registerNamespace implementation
const registerNamespace = (function () {
    // Private function to handle namespace creation
    function createNamespace(namespace) {
        const parts = namespace.split('.');
        let current = window || global;

        for (let i = 0; i < parts.length; i++) {
            // Create the namespace if it doesn't exist
            current[parts[i]] = current[parts[i]] || {};
            current = current[parts[i]];
        }

        return current;
    }

    // The main function with additional functionality
    function registerNS(namespace, initialValue) {
        const ns = createNamespace(namespace);

        // If an initial value is provided, extend the namespace
        if (initialValue !== undefined) {
            Object.assign(ns, initialValue);
        }

        return ns;
    }

    return registerNS;
})();

// More comprehensive implementation
function registerClass(config) {
    const {
        typeName,
        baseType,
        interfaceTypes = [],
        constructor
    } = config;

    // Create the class
    function Type(...args) {
        // Interface contract checking
        interfaceTypes.forEach(interfaceType => {
            for (let method in interfaceType.prototype) {
                if (typeof interfaceType.prototype[method] === 'function'
                    && !this[method]) {
                    throw new Error(`Must implement interface method: ${method}`);
                }
            }
        });

        // Base type constructor
        if (baseType) {
            baseType.apply(this, args);
        }

        // Own constructor
        if (constructor) {
            constructor.apply(this, args);
        }
    }

    // Inheritance
    if (baseType) {
        Type.prototype = Object.create(baseType.prototype);
        Type.prototype.constructor = Type;
    }

    // Interface implementation
    interfaceTypes.forEach(interfaceType => {
        Object.assign(Type.prototype, interfaceType.prototype);
    });

    return Type;
}

if (!window) this.window = this;

registerNamespace('Type');

Type.registerNamespace = registerNamespace;

Function.prototype.registerClass = registerClass;

Type.registerNamespace('Sys');

Sys.Browser = {};

Sys.Browser.InternetExplorer = {};
Sys.Browser.Firefox = {};
Sys.Browser.Safari = {};
Sys.Browser.Opera = {};

Sys.Browser.agent = null;
Sys.Browser.hasDebuggerStatement = false;
Sys.Browser.name = navigator.appName;
Sys.Browser.version = parseFloat(navigator.appVersion);

if (navigator.userAgent.indexOf(' MSIE ') > -1) {
    Sys.Browser.agent = Sys.Browser.InternetExplorer;
    Sys.Browser.version = parseFloat(navigator.userAgent.match(/MSIE (\d+\.\d+)/)[1]);
    Sys.Browser.hasDebuggerStatement = true;
}
else if (navigator.userAgent.indexOf(' Firefox/') > -1) {
    Sys.Browser.agent = Sys.Browser.Firefox;
    Sys.Browser.version = parseFloat(navigator.userAgent.match(/ Firefox\/(\d+\.\d+)/)[1]);
    Sys.Browser.name = 'Firefox';
    Sys.Browser.hasDebuggerStatement = true;
}
else if (navigator.userAgent.indexOf(' Safari/') > -1) {
    Sys.Browser.agent = Sys.Browser.Safari;
    Sys.Browser.version = parseFloat(navigator.userAgent.match(/ Safari\/(\d+\.\d+)/)[1]);
    Sys.Browser.name = 'Safari';
}
else if (navigator.userAgent.indexOf('Opera/') > -1) {
    Sys.Browser.agent = Sys.Browser.Opera;
}


Sys.WebForms = {};
Sys.WebForms.PageRequestManager = {
    getInstance: function () {
        return {
            add_beginRequest: function (par) { },
            add_endRequest: function (par) { }
        }
    }
};

Sys.Application = {
    add_load: function () {

    }
}

window.$find = function (id) {
    throw new Error('Not implemented'); // for making popups working
}

window.$get = function (id, element) {

    if (!element) return document.getElementById(id);
    if (element.getElementById) return element.getElementById(id);

    var nodeQueue = [];
    var childNodes = element.childNodes;
    for (var i = 0; i < childNodes.length; i++) {
        var node = childNodes[i];
        if (node.nodeType == 1) {
            nodeQueue[nodeQueue.length] = node;
        }
    }

    while (nodeQueue.length) {
        node = nodeQueue.shift();
        if (node.id == id) {
            return node;
        }
        childNodes = node.childNodes;
        for (i = 0; i < childNodes.length; i++) {
            node = childNodes[i];
            if (node.nodeType == 1) {
                nodeQueue[nodeQueue.length] = node;
            }
        }
    }

    return null;
}

Sys.Serialization = {
    JavaScriptSerializer: {
        serialize : function (object) {
            return JSON.stringify(object);
        },
        deserialize : function (data) {
            return JSON.parse(data);
        }
    }
}