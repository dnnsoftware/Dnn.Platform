/// <reference name="MicrosoftAjax.js" />

Type.registerNamespace('dnn.controls');

dnn.controls.triStateManager = function (images, toolTips) {
    var that;

    that = {
        initControl: function (hidden) {
            var $hdn = jQuery(hidden);
            if (!$hdn.hasClass('lockedPerm')) {
                if ($hdn.hasClass('noDenyPerm')) {
                    $hdn.siblings('img').click(function () {
                        if (hidden.value === 'True') {
                            hidden.value = 'Null';
                        } else {
                            hidden.value = 'True';
                        }

                        updateState(hidden);
                    });
                } else {
                    $hdn.siblings('img').click(function () {
                        if (hidden.value === 'True') {
                            hidden.value = 'False';
                        } else if (hidden.value === 'False') {
                            hidden.value = 'Null';
                        } else {
                            hidden.value = 'True';
                        }

                        updateState(hidden);
                    });
                }
            }
        }
    };

    function updateState(hidden) {
        var $hdn = jQuery(hidden);
        var state = hidden.value;

        var $collection = $hdn.parent().parent().find('td input.tristate');

        if ($hdn.hasClass('fullControl')) {
            $collection.each(function (index, elem) {
                var $elem = jQuery(elem);
                if (!$elem.hasClass('lockedPerm')) {
                    elem.value = hidden.value;
                    updateImage($elem, state);
                }
            });
        } else {
            updateImage($hdn, state);
            
            //Check if View Permission is denied (can't do other tasks if can't view)
            if ($hdn.hasClass('view')) {
                if (state === 'False') {
                    var $notView = $collection.not('input.view').not('input.navigate').not('input.browse');
                    $notView.each(function (index, elem) {
                        var $elem = jQuery(elem);
                        if (!$elem.hasClass('lockedPerm')) {
                            elem.value = state;
                            updateImage($elem, state);
                        }
                    });

                }
            } else {
                //if other permissions are set to true must have View
                if (state === 'True') {
                    if (!$hdn.hasClass('navigate') && !$hdn.hasClass('browse')) {
                        var $view = $collection.filter('input.view');
                        $view.each(function (index, elem) {
                            var $elem = jQuery(elem);
                            if (!$elem.hasClass('lockedPerm')) {
                                elem.value = state;
                                updateImage($elem, state);
                            }
                        });
                    }
                }
            }
            
            var $fullControl = $hdn.parent().parent().find('td input.fullControl');
            if ($fullControl.length > 0) {
                var fullControl = $fullControl[0];
                var $notFullControl = $collection.not('input.fullControl');
                var colstate = state;
                var setFullControl = true;
                $notFullControl.each(function (index, elem) {
                    if (colstate != elem.value) {
                        setFullControl = false;
                        return;
                    }
                });
                if (setFullControl) {
                    fullControl.value = state;
                } else {
                    fullControl.value = 'Null';
                }
                updateImage($fullControl, fullControl.value);
            }
        }
    }
    
    function updateImage($hdn, state) {
        var img = $hdn.siblings('img')[0];
        img.src = images[state];
        img.alt = toolTips[state];
    }

    return that;
}