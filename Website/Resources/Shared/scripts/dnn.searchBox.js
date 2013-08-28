(function ($, window) {

    $.dnnSearchBox = function (options, element) {
        this.$el = $(element);
        this._init(options);
    };

    $.dnnSearchBox.defaults = {
        defaultText: "Enter Search term",
        advancedText: 'Advanced',
        seemoreText: 'See more results',
        noresultText: 'No any results',
        showAdvanced: false,
        advancedId: null,
        enablePreview: true,
        refreshSearchResult: false,
        refreshSearchResultDelay: 400,
        refreshSearchResultMinChars: 2,
        previewUrl: null,
        previewMinChars: 2,
        previewDelay: 400,
        previewQueryParamName: 'keywords',
        previewOnError: null,
        searchQueryParamName: 'search',
        searchFiltersParamName: 'filter',
        searchFunction: null
    };

    $.dnnSearchBox.prototype = {
        _init: function (options) {
            this.options = $.extend(true, {}, $.dnnSearchBox.defaults, options);
            this._generateTemplate();
            this._attachEvents();
        },

        _generateTemplate: function () {
            var originalVal = this.$el.val();
            var markup = '<div class="dnnSearchBox">';
            var advancedEnabled = this.options.showAdvanced && this.options.advancedId && $('#' + this.options.advancedId).length;
            if (advancedEnabled) {
                markup += '<span class="dnnSearchBox_advanced_query" /><a class="dnnSearchBoxClearAdvanced"></a>';
            }
            markup += '<input id="' + this.options.id + '_input" type="text" value="' + originalVal + '" autocomplete="off" />' +
                            '<a class="dnnSearchBoxClearText"></a>';

            markup += '<a class="dnnSearchButton"></a>';

            if (advancedEnabled) {
                markup += '<div class="dnnSearchBox_advanced">' +
                                '<a class="dnnSearchBox_advanced_label">' + this.options.advancedText + '</a>' +
                                '<div class="dnnSearchBox_advanced_dropdown">';
                markup += '</div></div>';
            }

            markup += '<div class="dnnClear"></div>' +
                         '</div>';

            this.$el.hide();
            this.$wrap = $(markup).insertAfter(this.$el);
            var wrapWidth = this.$wrap.width();
            this.realInput = $('#' + this.options.id + '_input');

            var realInputRight = 50;
            var advancedDropdown = $('.dnnSearchBox_advanced_dropdown', this.$wrap);
            var advancedForm = $('#' + this.options.advancedId);
            if (advancedEnabled) {
                advancedForm.appendTo(advancedDropdown);
                var w = $('.dnnSearchBox_advanced', this.$wrap).width();
                var w2 = $('.dnnSearchBox_advanced_query', this.$wrap).width();
                realInputRight = w + w2 + 50;
                $('.dnnSearchBox_advanced_query', this.$wrap).hide().css({ marginRight: w + 30 });
                $('.dnnSearchBoxClearAdvanced', this.$wrap).css({ right: w + 38, top: 0 });
            }
            this.realInput.css({ right: realInputRight, width: wrapWidth - realInputRight - 8 });
            if (originalVal) {
                this.realInput.next().addClass('dnnShow').css({ right: realInputRight - 15});
            }
        },    

        _makeUrl: function (param, service) {
            var url = this.options.previewUrl ? this.options.previewUrl :
                    service ? service.getServiceRoot('internalservices') + 'searchService/preview' : null;
            if (!url) return null;

            var params = {};
            params[this.options.previewQueryParamName] = param.replace(/^\s+|\s+$/g, '');
            var urlAppend = [];
            $.each(params, function (index, value) {
                urlAppend.push([index, encodeURIComponent(value)].join('='));
            });

            if (urlAppend.length) {
                url += url.indexOf('?') === -1 ? '?' : '&';
                url += urlAppend.join('&');
            }
            return url;
        },

        _generatePreviewTemplate: function (keyword, data) {
            var preview = $('.dnnSearchBox_preview', this.$wrap);
            if (preview.length)
                preview.remove();

            var markup = '<ul class="dnnSearchBox_preview">';
            if (data && data.length) {                

                for (var i = 0; i < data.length; i++) {
                    var group = data[i];
                    if (group.Results && group.Results.length) {
                        var groupTitle = group.ContentItemTypeName;
                        markup += '<li class="dnnSearchBox_preview_group">' + groupTitle + '</li>';
                        for (var j = 0; j < group.Results.length; j++) {
                            var item = group.Results[j];
                            var itemTitle = item.Title;
                            var itemUrl = item.DocumentUrl;
                            var itemSnippet = item.Snippet;
                            markup += '<li data-url="' + itemUrl + '"><span>' + itemTitle + '</span>';
                            if (itemSnippet)
                                markup += '<p>' + itemSnippet + '</p></li>';
                            else
                                markup += '</li>';

                        } // end for group items
                    }
                } // end for group
                markup += '<li><a href="javascript:void(0)" class="dnnSearchBox_preview_more">' + this.options.seemoreText + '</a></li>';
                markup += '</ul>';
            }
            else {
                markup += '<li>' + this.options.noresultText + '</li><ul>';
            }

            this.$wrap.append(markup);
            preview = $('.dnnSearchBox_preview', this.$wrap);

            //attach click event
            $('li', preview).on('click', function () {
                var navigateUrl = $(this).attr('data-url');
                if (navigateUrl) {
                    window.location.href = navigateUrl;
                }
                return false;
            });

            //attach see more
            var searchBtn = $('.dnnSearchButton', this.$wrap);
            $('.dnnSearchBox_preview_more', this.$wrap).on('click', function () {
                searchBtn.triggerHandler('click');
                return false;
            });
        },

        _search: function (val) {
            if (this.options.searchFunction && typeof this.options.searchFunction == 'function') {
                var params = {};
                params[this.options.searchQueryParamName] = encodeURIComponent(val);
                var urlAppend = [];
                $.each(params, function (index, value) {
                    urlAppend.push([index, encodeURIComponent(value)].join('='));
                });
                urlAppend.push([this.options.searchFiltersParamName, ''].join('='));

                this.options.searchFunction(val, urlAppend);
            }
        },
        
        _ignoreKeyCodes: [9, 13, 16, 17, 18, 19, 20, 27, 33, 34, 35, 36, 37, 38, 39, 40, 45],

        _attachEvents: function () {
            var self = this;
            var realInput = this.realInput;
            var clearTextBtn = $('a.dnnSearchBoxClearText', this.$wrap);
            var advanced = $('.dnnSearchBox_advanced', this.$wrap);

            // placeholder stuff
            var placeholderSupported = ('placeholder' in document.createElement('input'));
            if (placeholderSupported)
                realInput.attr('placeholder', this.options.defaultText);
            else {
                realInput.on('focus', function () {
                    if (realInput.hasClass('dnnPlaceholder'))
                        realInput.val('').removeClass('dnnPlaceholder');
                })
                .on('blur', function () {
                    var val = realInput.val();
                    if (!val)
                        realInput.val(self.options.defaultText).addClass('dnnPlaceholder');
                    else
                        realInput.removeClass('dnnPlaceholder');
                })
                .blur();
            }

            // clear textbox button
            realInput.on('keyup', function (e) {
                var k = e.keyCode || e.witch;
                if (self._ignoreKeyCodes.indexOf(k) > -1) return;

                var val = realInput.val();
                var right = parseInt(realInput.css('right').replace('px', '')) - 15;
                if (!val) {
                    clearTextBtn.removeClass('dnnShow');
                    // hide preview
                    $('.dnnSearchBox_preview', this.$wrap).remove();
                }
                else {
                    clearTextBtn.css({
                    	right: right
                    }).addClass('dnnShow');
                    if (self.options.enablePreview &&
                        val.length >= self.options.previewMinChars) {
                        // enable preview
                        if (self.throttle) {
                            clearTimeout(self.throttle);
                            delete self.throttle;
                        }
                        self.throttle = setTimeout(function () {

                            var service = self.options.moduleId ? ($.dnnSF ? $.dnnSF(self.options.moduleId) : null) : null;
                            var url = self._makeUrl(val, service);
                            if (url) {
                                $.ajax({
                                    url: url,
                                    beforeSend: service ? service.setModuleHeaders : null,
                                    success: function (result) {
                                        if (result)
                                            self._generatePreviewTemplate(val, result);
                                    },
                                    error: function (jqXhr, textStatus, errorThrown) {
                                        if ($.isFunction(self.options.previewOnError)) {
                                            self.options.previewOnError(jqXhr, textStatus, errorThrown);
                                        }
                                    },
                                    type: 'GET',
                                    dataType: 'json',
                                    contentType: "application/json"
                                });
                            }
                        }, self.options.previewDelay);
                    }
                    else if (self.options.refreshSearchResult &&
                        self.options.searchFunction &&
                        typeof self.options.searchFunction == 'function' &&
                        val.length >= self.options.refreshSearchResultMinChars) {
                        
                        // enable auto search refresh
                        if (self.throttle) {
                            clearTimeout(self.throttle);
                            delete self.throttle;
                        }

                        self.throttle = setTimeout(function () {
                            self.options.searchFunction(val);
                        }, self.options.refreshSearchResultDelay);
                    }
                }
            })

            .on('paste', function () {
                realInput.triggerHandler('keyup');
            })

            .on('keypress', function (e) {
                var k = e.keyCode || e.witch;
                if (k == 13) {
                    $('.dnnSearchButton', this.$wrap).trigger('click');
                    e.preventDefault();
                }
            });

            clearTextBtn.on('click', function () {
                realInput.val('').focus();
                clearTextBtn.removeClass('dnnShow');
                $('.dnnSearchBox_preview', this.$wrap).remove();
                return false;
            });

            if (this.options.showAdvanced) {
                var advancedLabel = $('.dnnSearchBox_advanced_label', advanced);
                var advancedContent = $('.dnnSearchBox_advanced_dropdown', advanced);

                advancedLabel.on('click', function () {
                    advancedLabel.toggleClass('dnnExpanded');
                    advancedContent.toggle();
                    return false;
                }).mouseup(function () {
                    return false;
                });

                $('body').mouseup(function () {
                    if (advancedContent.is(':visible')) {
                        advancedContent.hide();
                        advancedLabel.removeClass('dnnExpanded');
                    }
                });

                advancedContent.mouseup(function () {
                    return false;
                });
                realInput.mouseup(function() {
                    return false;
                });
            }

            // search button hit
            $('.dnnSearchButton', this.$wrap).on('click', function () {
                // already auto refresh search result, no need to go to search result page
                if (self.options.refreshSearchResult) {
                    if (self.options.beforeRefreshSearchResult && typeof self.options.beforeRefreshSearchResult == 'function')
                        self.options.beforeRefreshSearchResult();
                    
                    self.realInput.triggerHandler('keyup');
                    return false;
                }

                var val = realInput.val();
                if (val) {
                    self._search(val);
                }
                return false;
            });
        }
    };

    $.fn.dnnSearchBox = function (options) {
        var instance = $.data(this, 'dnnSearchBox');

        if (instance) {
            instance._init();
        }
        else {
            instance = $.data(this, 'dnnSearchBox', new $.dnnSearchBox(options, this));
        }
    };

})(jQuery, window);