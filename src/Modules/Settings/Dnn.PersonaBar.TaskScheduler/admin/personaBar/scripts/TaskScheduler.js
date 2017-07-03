'use strict';
define(['jquery',
    'main/config'
],
    function ($, cf) {
        var utility;
        var config = cf.init();

        return {
            init: function (wrapper, util, params, callback) {
                utility = util;

                window.dnn.initScheduler = function initializeScheduler() {
                    return {
                        utility: utility,
                        moduleName: 'TaskScheduler'
                    };
                };
                utility.loadBundleScript('modules/dnn.taskscheduler/scripts/bundles/task-scheduler-bundle.js');

                if (typeof callback === 'function') {
                    callback();
                }
            },

            load: function (params, callback) {
                if (typeof callback === 'function') {
                    callback();
                }
            }
        };
    });


