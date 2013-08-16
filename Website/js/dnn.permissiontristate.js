/// <reference name="MicrosoftAjax.js" />

Type.registerNamespace('dnn.controls');

dnn.controls.triStateManager = function (images, toolTips) {
    var that;

    that = {
        initControl: function (hidden) {
            var hdn = jQuery(hidden);
            if (!hdn.hasClass('lockedPerm')) {
                if (hdn.hasClass('noDenyPerm')) {

                    hdn.siblings('img').click(function () {
                        if (hidden.value === 'True') {
                            hidden.value = 'Null';
                        } else {
                            hidden.value = 'True';
                        }

                        this.src = images[hidden.value];
                        this.alt = toolTips[hidden.value];
                    });

                } else {

                    hdn.siblings('img').click(function () {
                        if (hidden.value === 'True') {
                            hidden.value = 'False';
                        } else if (hidden.value === 'False') {
                            hidden.value = 'Null';
                        } else {
                            hidden.value = 'True';
                        }

                        this.src = images[hidden.value];
                        this.alt = toolTips[hidden.value];
                    });

                }
            }
        }
    };

    return that;
}