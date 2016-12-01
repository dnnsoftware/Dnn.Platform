/*
DotNetNuke® - http://www.dotnetnuke.com
Copyright (c) 2002-2016
by DotNetNuke Corporation
All Rights Reserved
*/

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

        var isMobile, utility;

        var dnnPageRecycleBin;

        var init, initMobile, load, loadMobile,
            initRecycleBin, viewRecycleBin;

        utility = null;

        ko.mapping = koMapping;

        init = function (wrapper, util, params, callback) {
            utility = util;

            dnnPageRecycleBin = new DnnPageRecycleBin(utility.resx.Recyclebin, utility.sf, utility);

            initRecycleBin(wrapper);

            if (typeof callback === 'function') {
                callback();
            }
        };

        initMobile = function (wrapper, util, params, callback) {
            isMobile = true;
            this.init(wrapper, util, params, callback);
        };

        load = function (params, callback) {
            viewRecycleBin();

            if (dnn && dnn.dnnPageHierarchy) {
                dnn.dnnPageHierarchy.load();
            }
        };

        loadMobile = function (params, callback) {
            isMobile = true;
            this.load(params, callback);
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
            load: load,
            initMobile: initMobile,
            loadMobile: loadMobile
        };
    });
