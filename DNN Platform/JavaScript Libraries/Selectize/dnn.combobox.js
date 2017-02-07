(function($) {
    $.fn.dnnComboBox = function(options) {
        var opts = $.extend({}, {}, options),
        $elements = this;

        $elements.each(function () {
            var $this = $(this);

            $this.selectize(opts);
        });

        return this;
    };
})(jQuery);