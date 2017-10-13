(function ($) {
    $.fn.qaTooltip = function (options) {
        var opts = $.extend({}, $.fn.qaTooltip.defaultOptions, options),
                   $wrap = this;
        $wrap.each(function () {
            var $this = $(this);
            $this.css('position', 'relative');
            $this.find('a.tag').css('cursor', 'pointer');
            $this.onHover = false;
            function hoverOver() {
                $this.onHover = true;
                if (opts.useParentActiveClass) {
                    $this.addClass(opts.parentActiveClass);
                }
                if (opts.postRender && typeof opts.postRender === 'function') {
                    if (opts.asyncDelegate && typeof opts.asyncDelegate === 'function') {
                        opts.asyncDelegate.call($this, function () {
                            if ($this.onHover) {
                                $this.find(opts.tooltipSelector).show();
                                opts.postRender.call($this);
                            }
                        });
                    }
                    else {
                        if ($this.onHover) {
                            $this.find(opts.tooltipSelector).show();
                            opts.postRender.call($this);
                        }

                    }
                }
                else if (opts.asyncDelegate && typeof opts.asyncDelegate === 'function') {
                    opts.asyncDelegate.call($this, function () {
                        if ($this.onHover) {
                            $this.find(opts.tooltipSelector).show();
                        }
                    });
                }
                else
                    $this.find(opts.tooltipSelector).show();
            }

            function hoverOut() {
                $this.onHover = false;
                if (opts.useParentActiveClass) {
                    $this.removeClass(opts.parentActiveClass);
                }
                $this.find(opts.tooltipSelector).hide();
            }

            $this.hoverIntent({
                over: function () {
                    hoverOver();
                },
                out: function () {
                    hoverOut();
                },
                timeout: 200,
                interval: 200
            });

            if (opts.enableTouch) {
                $this.bind("touchstart", hoverOver);
                $this.bind("touchend", hoverOut);
            }

            if (opts.suppressClickSelector) {
                $this.find(opts.suppressClickSelector).click(function (e) {
                    e.preventDefault();
                });
            }
        });
        return $wrap;
    };

    $.fn.qaTooltip.defaultOptions = {
        tooltipSelector: '.tag-menu',
        suppressClickSelector: '',
        parentActiveClass: 'active',
        useParentActiveClass: false,
        enableTouch: false
    };

})(jQuery);