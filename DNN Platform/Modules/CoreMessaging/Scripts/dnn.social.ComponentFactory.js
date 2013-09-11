// DotNetNuke® - http://www.dotnetnuke.com
//
// Copyright (c) 2002-2013 DotNetNuke Corporation
// All rights reserved.

(function (dnn) {
    'use strict';

    if (typeof dnn === 'undefined') dnn = {};
    if (typeof dnn.social === 'undefined') dnn.social = {};

    dnn.social.ComponentFactory = function ComponentFactory (moduleId) {
        var that = this;

        this.moduleId = moduleId;

        this.components = { };

        this.getModuleId = function () {
            return that.moduleId;
        };

        this.getType = dnn.social.ComponentFactory.getType;
        
        this.register = function (object, component) {
            if (typeof component === 'undefined') {
                component = that.getType(object);
            }

            if (typeof that.components[component] !== 'undefined') {
                var c = that.components[component];

                switch (typeof that.components[component]['dispose']) {
                case 'undefined':
                    break;
                case 'function':
                    try {
                        c.dispose();
                    } catch (e) {
                        console.log('Failure to dispose of component: {0}: {1}'.format(component, e.message));
                    }
                    break;
                default:
                    console.log(
                        'Nonconforming dispose method on {0}: dispose is a {1}, not a function'.format(component, typeof (c.dispose).toString()));
                    break;
                }

                delete that.components[component];
            }

            that.components[component] = object;

            return object;
        };
        
        this.resolve = function (component) {
            if (that.components.hasOwnProperty(component)) {
                return that.components[component];
            }

            return undefined;
        };

        this.hasComponent = function (component) {
            return that.components.hasOwnProperty(component);
        };
    };
    
    // In order for getType to work, you need to construct your object with a named function, in this manner:
    //
    //   dnn.social.Foo = function Foo () { ... };
    //
    // The name is the 'Foo' part in 'function Foo'. This is the only way that ComponentFactory can get the
    // type name of an object; otherwise it is just an 'object' and the automatic name lookup will fail,
    // and so you will need to specify a second parameter in register() (in this case, 'Foo'). But we will
    // make a valiant effort to figure out what your object is named if you don't say.
    dnn.social.ComponentFactory.getType = function (obj) {
        if (obj.constructor != null &&
            obj.constructor.name != null &&
            obj.constructor.name.length > 0) {
            return obj.constructor.name;
        }

        // IE
        var results = /function ([a-zA-Z]+)/.exec(obj.constructor.toString());
        if (results != null && results.length > 1) {
            return results[1];
        }

        return Object.prototype.toString.call(obj).match(/^\[object (.*)\]$/)[1];
    };
})(window.dnn);