define(['jquery'], function ($) {
    return {
        init: function (wrapper, util, params, callback) {
            const vsn = "v0.3-alpha";
            $.ajax({
                dataType: "script",
                cache: true,
                url: "modules/dnn.prompt/scripts/bundles/prompt-bundle.js",
                success: function () {
                    window.dnnPrompt = new window.DnnPrompt(vsn, wrapper, util, params);
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
