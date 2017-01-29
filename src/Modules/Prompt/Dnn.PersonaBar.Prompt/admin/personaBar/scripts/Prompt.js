define(['jquery'], function ($) {
    return {
        init: function (wrapper, util, params, callback) {
            $.ajax({
                dataType: "script",
                cache: true,
                url: "modules/dnn.prompt/scripts/bundles/prompt-bundle.js",
                success: function () {
                    window.dnnPrompt = new window.DnnPrompt('1.0.0.0', util, params);
                },
            });
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
