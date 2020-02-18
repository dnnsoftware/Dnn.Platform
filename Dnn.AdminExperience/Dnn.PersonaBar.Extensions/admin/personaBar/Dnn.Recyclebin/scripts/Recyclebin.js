// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

/*
* Module responsible to manage the Recycle Bin
*/
define(['jquery',
    'knockout',
    'knockout.mapping',
    './RecycleBin.ViewModel',
	'dnn.jquery',
	'dnn.extensions',
	'dnn.jquery.extensions',
	'jquery.tokeninput',
	'dnn.jScrollBar',
    'jquery-ui.min'],
    function ($, ko, koMapping, DnnPageRecycleBin) {
        'use strict';

        var dnnPageRecycleBin;

        var init, load,
            initRecycleBin, viewRecycleBin;

        var utility = null;

        ko.mapping = koMapping;

        init = function (wrapper, util, params, callback) {
            utility = util;

            dnnPageRecycleBin = new DnnPageRecycleBin(utility.resx.Recyclebin, utility.sf, utility, null, params.settings);

            initRecycleBin(wrapper);

            if (typeof callback === 'function') {
                callback();
            }
        };

        load = function (params, callback) {
            viewRecycleBin();

            if (dnn && dnn.dnnPageHierarchy) {
                dnn.dnnPageHierarchy.load();
            }
        };

        initRecycleBin = function (wrapper) {
            dnnPageRecycleBin.init(wrapper);
            viewRecycleBin();
        };

        viewRecycleBin = function () {
            if (typeof dnn.dnnPageHierarchy != "undefined" && dnn.dnnPageHierarchy.hasPendingChanges()) {
                return dnn.dnnPageHierarchy.handlePendingChanges();
            }
            dnnPageRecycleBin.show();
        };

        return {
            init: init,
            load: load
        };
    });
