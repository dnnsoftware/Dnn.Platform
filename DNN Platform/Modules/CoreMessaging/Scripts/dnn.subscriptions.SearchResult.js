// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

(function (dnn) {
    'use strict';

    if (typeof dnn === 'undefined') dnn = {};
    if (typeof dnn.subscriptions === 'undefined') dnn.subscriptions = {};

    dnn.subscriptions.SearchResult = function ($, ko, settings, root, model) {
    	var that = this;

	    var komodel = function(mo) {
		    var obj = {};

		    if (typeof(mo) !== 'undefined') {
			    var convert =
				    function(key, value) {
					    var k = toCamelCase(key);

					    if (value === null || typeof(value) === 'undefined') {
						    obj[k] = ko.observable();
					    } else if (value instanceof Array) {
						    var list = [];

						    $.each(value,
							    function(index, element) {
								    list.push(dnn.social.komo(element));
							    });

						    obj[k] = ko.observableArray(list);
					    } else {
						    obj[k] = ko.observable(value);
					    }
				    };

			    if (mo != null) {
				    switch (typeof(mo)) {
				    case 'undefined':
					    break;
				    case 'string':
				    case 'number':
					    obj = mo;
					    break;
				    case 'object':
					    var keys = Object.keys(mo);

					    $.each(keys,
						    function(unused, key) {
							    convert(key, mo[key]);
						    });

					    break;
				    case 'array':
					    obj = ko.observableArray(mo);
					    break;
				    default:
					    console.log('Unknown object type ({0})'.replace("{0}", typeof(mo)));
					    break;
				    }
			    }
		    }

		    return obj;
	    };

	    var toCamelCase = function(key) {
		    if (typeof key === 'number') {
			    return key;
		    }

		    var k = key[0].toLowerCase();

		    if (key.length > 1) {
			    return k + key.substr(1);
		    }

		    return k;
	    };

        $.extend(this, komodel(model));
        
        //var localizer = social.getLocalizationController();
        //this.service = social.getService('Subscriptions');
    };
})(window.dnn);