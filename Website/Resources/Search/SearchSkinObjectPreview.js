(function ($) {
    if (typeof dnn == 'undefined') window.dnn = {};
    if (typeof dnn.searchSkinObject == 'undefined') {
        dnn.searchSkinObject = function (options) {
            var settings = {
                delayTriggerAutoSearch: 100,
                minCharRequiredTriggerAutoSearch: 2,
                searchType: 'S',
                enableWildSearch: true,
                cultureCode: 'en-US'
            };
            this.settings = $.extend({}, settings, options);
        };
        dnn.searchSkinObject.prototype = {
            _ignoreKeyCodes: [9, 13, 16, 17, 18, 19, 20, 27, 33, 34, 35, 36, 37, 38, 39, 40, 45],
            init: function () {
                var throttle = null, self = this;
                var makeUrl = function (val, service) {
                    var url = service ? service.getServiceRoot('internalservices') + 'searchService/preview' : null;
                    if (!url) return null;
                    var params = {};
                    params['keywords'] = val.replace(/^\s+|\s+$/g, '');
                    if (!self.settings.enableWildSearch) params["forceWild"] = "0";
                    params['culture'] = self.settings.cultureCode;
                    if (self.settings.portalId >= 0)
                        params['portal'] = self.settings.portalId;
                    var urlAppend = [];
                    $.each(params, function (index, value) {
                        urlAppend.push([index, encodeURIComponent(value)].join('='));
                    });

                    if (urlAppend.length) {
                        url += url.indexOf('?') === -1 ? '?' : '&';
                        url += urlAppend.join('&');
                    }
                    return url;
                };

                var generatePreviewTemplate = function (data, $wrap) {
                    var preview = $('.searchSkinObjectPreview', $wrap);
                    if (preview.length)
                        preview.remove();

                    var markup = '<ul class="searchSkinObjectPreview">';
                    if (data && data.length) {
                        for (var i = 0; i < data.length; i++) {
                            var group = data[i];
                            if (group.Results && group.Results.length) {
                                var groupTitle = group.DocumentTypeName;
                                markup += '<li class="searchSkinObjectPreview_group">' + groupTitle + '</li>';
                                for (var j = 0; j < group.Results.length; j++) {
                                    var item = group.Results[j];
                                    var itemTitle = item.Title;
                                    var itemUrl = item.DocumentUrl;
                                    var itemDescription = item.Description;
                                    var itemSnippet = item.Snippet;
                                    markup += '<li data-url="' + itemUrl + '">';
                                    if (item.Attributes.Avatar) {
                                        markup += '<span><img src="' + item.Attributes.Avatar + '" class="userpic" /></span>';
                                    }
                                    markup += '<span>' + itemTitle + '</span>';
                                    if (itemDescription) {
                                        markup += '<p>' + itemDescription + '</p>';
                                    }
                                    if (itemSnippet) {
                                        markup += '<p>' + itemSnippet + '</p>';
                                    }
                                    markup += '</li>';
                                } // end for group items
                            }
                        } // end for group

                        var moreResults = $wrap.attr('data-moreresults');
                        markup += '<li><a href="javascript:void(0)" class="searchSkinObjectPreview_more">' + moreResults + '</a></li>';
                        markup += '</ul>';
                    }
                    else {
                        var noResult = $wrap.attr('data-noresult');
                        markup += '<li>' + noResult + '</li></ul>';
                    }

                    $wrap.append(markup);
                    preview = $('.searchSkinObjectPreview', $wrap);

                    //attach click event
                    $('li', preview).on('click', function () {
                        var navigateUrl = $(this).attr('data-url');
                        if (navigateUrl) {
                            window.location.href = navigateUrl;
                        }
                        return false;
                    });

                    //attach see more       
                    $('.searchSkinObjectPreview_more', $wrap).on('click', function () {
                        var $searchButton = $wrap.next();
                        if (!$searchButton.length) {
                            $searchButton = $wrap.parent().next();
                        }
                        $searchButton[0].click();
                        return false;
                    });
                };

                $('.searchInputContainer a.dnnSearchBoxClearText').on('click', function () {
                    var $this = $(this);
                    var $wrap = $this.parent();
                    $('.searchInputContainer input').val('').focus();
                    $this.removeClass('dnnShow');
                    $('.searchSkinObjectPreview', $wrap).remove();
                    return false;
                });

                $('.searchInputContainer').next().on('click', function() {
                    var $this = $(this);
                    var inputBox = $this.prev().find('input[type="text"]');
                    var val = inputBox.val();
                    if (val.length) {
                        return true;
                    }
                    return false;
                });

                $('.searchInputContainer input').on('keyup', function(e) {
                    var k = e.keyCode || e.witch;
                    if ($.inArray(k, self._ignoreKeyCodes) > -1) return;

                    var $this = $(this);
                    var $wrap = $this.parent();
                    var val = $this.val();
                    var container = $this.parent('.searchInputContainer');
                    if (!val) {

                        $('a.dnnSearchBoxClearText', $wrap).removeClass('dnnShow');
                        $('.searchSkinObjectPreview', $wrap).remove();
                    } else {
                        $('a.dnnSearchBoxClearText', $wrap).addClass('dnnShow');

                        if (self.settings.searchType != 'S' ||
                            val.length < self.settings.minCharRequiredTriggerAutoSearch) return;

                        if (throttle) {
                            clearTimeout(throttle);
                            delete throttle;
                        }

                        throttle = setTimeout(function() {
                            var service = $.dnnSF ? $.dnnSF(-1) : null;
                            var url = makeUrl(val, service);
                            if (url) {
                                $.ajax({
                                    url: url,
                                    beforeSend: service ? service.setModuleHeaders : null,
                                    success: function(result) {
                                        if (result)
                                            generatePreviewTemplate(result, container);
                                    },
                                    error: function() {
                                    },
                                    type: 'GET',
                                    dataType: 'json',
                                    contentType: "application/json"
                                });
                            }
                        }, self.settings.delayTriggerAutoSearch);
                    }
                })
                .on('paste', function() {
                    $(this).triggerHandler('keyup');
                })
                .on('keypress', function(e) {
                    var k = e.keyCode || e.which;
                    if (k == 13) {
                        var $this = $(this);
                        var $wrap = $this.parent();
                        var val = $this.val();
                        if (val.length) {
                            var $searchButton = $wrap.next();
                            if (!$searchButton.length) {
                                $searchButton = $wrap.parent().next();
                            }
                            $searchButton[0].click();
                            e.preventDefault();
                        } else {
                            e.preventDefault();
                        }
                    }
                });
            }
        };
    }
})(jQuery);
