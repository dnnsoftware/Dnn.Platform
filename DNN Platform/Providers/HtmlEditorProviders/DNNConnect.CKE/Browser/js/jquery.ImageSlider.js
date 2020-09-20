var maxWidth = 360;
var maxHeight = 300;
var ratio = 100;
function SetupSlider(sliderId, minimumValue, maximumValue, inStepsOf, orientationValue, valueValue, inputControlClass) {
    jQuery(sliderId).slider({
        min: minimumValue,
        max: maximumValue,
        step: inStepsOf,
        orientation: orientationValue,
        value: valueValue,
        slide: function (event, ui) {
            jQuery(inputControlClass).val(ui.value);
            SetPreview(inputControlClass, ui.value)
        }
    });
    jQuery(inputControlClass).val(jQuery(sliderId).slider("value"))
}
function SetSliderValue(sliderId, textBoxControl) {
    var amount = textBoxControl.value;
    var minimum = jQuery(sliderId).slider("min");
    var maximum = jQuery(sliderId).slider("max");
    if (amount > minimum || amount < maximum) {
        jQuery(sliderId).slider('option', 'value', amount)
    }
}
function SetPreview(inputClass, Value) {
    var height = jQuery("#imgOriginal").css("height");
    var width = jQuery("#imgOriginal").css("width");
    if (inputClass == '#txtWidth') {
        width = Value;
        if (width > maxWidth) {
            ratio = maxWidth / width;
            jQuery('#imgResized').css("width", maxWidth);
            jQuery('#imgResized').css("height", height * ratio);
            height = height * ratio
        } else {
            jQuery('#imgResized').css("width", width)
        }
    }
    if (inputClass == '#txtHeight') {
        height = Value;
        if (height > maxHeight) {
            ratio = maxHeight / height;
            jQuery('#imgResized').css("height", maxHeight);
            jQuery('#imgResized').css("width", width * ratio);
            width = width * ratio
        } else {
            jQuery('#imgResized').css("height", height)
        }
    }
}
function ChangedSliderW(textControl) {
    var width = textControl.value;
    var height = jQuery("#imgOriginal").css("height");
    jQuery("#SliderWidth").slider('option', 'value', textControl.value);
    if (width > maxWidth) {
        ratio = maxWidth / width;
        jQuery('#imgResized').css("width", maxWidth);
        jQuery('#imgResized').css("height", height * ratio);
        height = height * ratio;
        width = width * ratio
    } else {
        jQuery('#imgResized').css("width", width)
    }
}
function ChangedSliderH(textControl) {
    jQuery("#SliderHeight").slider('option', 'value', textControl.value);
    var height = textControl.value;
    var width = jQuery("#imgOriginal").css("width");
    if (height > maxHeight) {
        ratio = maxHeight / height;
        jQuery('#imgResized').css("height", maxHeight);
        jQuery('#imgResized').css("width", width * ratio);
        width = width * ratio
    } else {
        jQuery('#imgResized').css("height", height)
    }
}