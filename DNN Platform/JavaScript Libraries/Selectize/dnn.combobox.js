(function ($) {
    //Customize Selectize Plugin

    Selectize.define('dnn-combo-box', function (options) {
        this.get_options = function() {
            return this.search('').items;
        };

        this.get_items = function () {
            return this.items;
        };

        this.get_count = function() {
            return this.get_options().length
        }
    });

    //End Customize

    $.fn.dnnComboBox = function (options) {
        var opts = $.extend({}, {}, options), $elements = this;
        opts.plugins = $.merge(opts.plugins, ['dnn-combo-box']);
        if (typeof opts.onChange === "string") {
            var onChangeEvent = opts.onChange;
            opts.onChange = function() {
                eval('(' + onChangeEvent + ')').apply(this, arguments);
            }
        }

        if (typeof opts.load === "string") {
            var loadEvent = opts.load;
            opts.load = function () {
                eval('(' + loadEvent + ')').apply(this, arguments);
            }
        }

        if (typeof opts.render === "object") {
            for (var event in opts.render) {
                if (opts.render.hasOwnProperty(event)) {
                    var originEvent = opts.render[event];
                    opts.render[event] = function () {
                        return eval('(' + originEvent + ')').apply(this, arguments);
                    }
                }
            }
        }

        var originalInitialize = opts.onInitialize;
        opts.onInitialize = function () {
            this.$control_input.attr('aria-label', "Search");

            if (typeof originalInitialize === "function") {
                originalInitialize.apply(this, arguments);
            }
        };

        if (opts.maxOptions <= 0) {
            opts.maxOptions = undefined;
        }

        if (opts.maxItems <= 0) {
            opts.maxItems = undefined;
        }

        if (opts.checkbox) {
            opts.hideSelected = false;
            opts.render = $.extend({}, {
                option: function (data, escape) {
                    var $option = $('<div><span></span><label></label></div>');
                    $option.find('label').html(data.text);

                    return '<div class="option">' + $option.html() + '</div>';
                }
            }, opts.render);

            var buildSummary = function() {
                this.$control.find('input.summary').remove();
                var $summary = $('<input class="summary" aria-label="Summary" />').prependTo(this.$control);

                var options = this.options;
                var items = this.items;
                var labels = items.map(function(i) {
                    return options[i][opts.labelField];
                });
                var summaryText = "";
                if (items.length === this.get_options().length) {
                    summaryText = opts.localization["AllItemsChecked"];
                } else if (items.length === 1) {
                    summaryText = labels.join(',');
                } else {
                    summaryText = items.length + ' ' + opts.localization["ItemsChecked"];
                }

                $summary.val(summaryText);
            }

            var originalonChangeEvent = opts.onChange;
            opts.onChange = function (value) {
                buildSummary.call(this);

                if (typeof originalonChangeEvent == "function") {
                    originalonChangeEvent.call(this, value);
                }
            };

            var originalonInitializeEvent = opts.onInitialize;
            opts.onInitialize = function () {
                buildSummary.call(this);

                if (typeof originalonInitializeEvent == "function") {
                    originalonInitializeEvent.call(this);
                }
            };

            $elements.addClass('show-checkbox');
        }

        $elements.each(function () {
            var $this = $(this);

            $this.selectize(opts);

            if (opts.checkbox) {
                var selectize = $this[0].selectize;
                selectize.onOptionSelect = function (e) {
                    if (e.type !== "mousedown") return;
                    var value, $target, $option, self = this;

                    if (e.preventDefault) {
                        e.preventDefault();
                        e.stopPropagation();
                    }

                    $target = $(e.currentTarget);
                    if ($target.hasClass('create')) {
                        self.createItem(null, function() {
                            if (self.settings.closeAfterSelect) {
                                self.close();
                            }
                        });
                    } else {
                        value = $target.attr('data-value');
                        if (typeof value !== 'undefined') {
                            self.lastQuery = null;
                            self.setTextboxValue('');

                            var alreadySelected = self.items.indexOf(value) !== -1;
                            if (alreadySelected) {
                                self.removeItem(value);
                                e.stopImmediatePropagation();
                            } else {
                                self.addItem(value);
                            }
                            
                            if (self.settings.closeAfterSelect) {
                                self.close();
                            } else if (!self.settings.hideSelected && e.type && /mouse/.test(e.type)) {
                                if (alreadySelected) {
                                    if (self.$activeOption) self.$activeOption.removeClass('selected').addClass('active');
                                } else {
                                    self.setActiveOption(self.getOption(value));
                                }
                            }
                        }
                    }
                };
            }
        });

        return this;
    };
})(jQuery);