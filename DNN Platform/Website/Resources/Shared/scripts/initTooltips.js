jQuery(document).ready(function($) {
    $('.dnnTooltip').dnnTooltip();
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function() {
        $('.dnnTooltip').dnnTooltip();
    });
});