(function ($, dnn) {
    if (typeof dnn == 'undefined' || dnn == null) dnn = {};
    dnn.searchResult = {};

    dnn.searchResult.defaultSettings = {
        comboAdvancedDates: '',
        comboAdvancedScope: '',
        noresultsText: 'No Results Found',
        advancedText: 'Advanced',
        sourceText: 'Source:',
        authorText: 'Author:',
        likesText: 'Likes:',
        viewsText: 'Views:',
        commentsText: 'Comments:',
        tagsText: 'Tags:',
        addTagText: 'Add a tag',
        resultsCountText: 'About {0} Results',
        currentPageIndexText: 'Current Page Number:',
        linkTarget: '',
        cultureCode: 'en-US'
    };

    dnn.searchResult.advancedSearchOptions = {
        tags: [],
        after: '',
        types: [],
        exactSearch: false
    };

    dnn.searchResult.service = null;

    dnn.searchResult.queryUrl = function () {
        if (!dnn.searchResult.service)
            dnn.searchResult.service = $.dnnSF ? $.dnnSF(dnn.searchResult.moduleId) : null;

        if (!dnn.searchResult.service)
            return null;

        var url = dnn.searchResult.service.getServiceRoot('internalservices')
                    + 'searchService/search';

        var params = {};
        var query = dnn.searchResult.queryOptions;
        var advancedQuery = query.advancedTerm ? ' ' + query.advancedTerm : '';
        var queryString = query.searchTerm ? query.searchTerm.replace(/^\s+|\s+$/g, '') + advancedQuery : query.advancedTerm;

        params['search'] = queryString;
        params['pageIndex'] = query.pageIndex;
        params['pageSize'] = query.pageSize;
        params['sortOption'] = query.sortOption;
        params['culture'] = dnn.searchResult.defaultSettings.cultureCode;

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
    
    dnn.searchResult.renderPager = function (totalHits, pageIndex, more) {
        // render
        $('.dnnSearchResultPager').show();
        $('.dnnSearchResultPager > .dnnLeft > span').html(dnn.searchResult.defaultSettings.resultsCountText.replace('{0}', totalHits));
        var pager = '';

        if (pageIndex > 1 || more) {
            
            pager += '<span class="dnnPager-current" >' + dnn.searchResult.defaultSettings.currentPageIndexText + ' ' + pageIndex + '</span>';
            
            if (pageIndex > 1) {
                pager += '<a class="dnnPager-prev" href="javascript:void(0)" />';
            } else {
                pager += '<a class="dnnPager-prev dnnPager-disable" href="javascript:void(0)" />';
            }

            if (more) {
                pager += '<a class="dnnPager-next" href="javascript:void(0)" />';
            } else {
                pager += '<a class="dnnPager-next dnnPager-disable" href="javascript:void(0)" />';
            }
        }

        $('.dnnSearchResultPager > .dnnRight').html(pager);

        // binding pager event
        $('.dnnSearchResultPager > .dnnRight a').on('click', function () {
            if ($(this).hasClass('dnnPager-disable')) return false;

            if ($(this).hasClass('dnnPager-prev'))
                dnn.searchResult.queryOptions.pageIndex--;
            else if ($(this).hasClass('dnnPager-next'))
                dnn.searchResult.queryOptions.pageIndex++;

            dnn.searchResult.doSearch();
            return false;
        });
    };

    dnn.searchResult.renderResult = function(data, renderUrl) {
        var markup = '';
        markup += '<div class="dnnSearchResultItem-Title">';
        markup += '<a href="' + data.DocumentUrl + '"' + dnn.searchResult.defaultSettings.linkTarget + '>' + data.Title + '</a></div>';
        if(renderUrl)
            markup += '<div class="dnnSearchResultItem-Link"><a href="' + data.DocumentUrl + '"' + dnn.searchResult.defaultSettings.linkTarget + '>' + data.DocumentUrl + '</a></div>';
        
        markup += '<div class="dnnSearchResultItem-Description">' + data.Snippet + '</div>';
        markup += '<div class="dnnSearchResultItem-Others">';
        markup += '<span>' + dnn.searchResult.defaultSettings.lastModifiedText + ' </span>';
        markup += data.DisplayModifiedTime;
        markup += '</div>';

        markup += '<div class="dnnSearchResultItem-Others">';
        markup += '<span>' + dnn.searchResult.defaultSettings.sourceText + ' </span>';
        markup += '<a href="javascript:void(0)" class="dnnSearchResultItem-sourceLink" data-value="' + data.DocumentTypeName + '" >' + data.DocumentTypeName + '</a>';

        if (data.AuthorName && data.AuthorProfileUrl) {
            markup += '&nbsp;&nbsp;&nbsp;<span>' + dnn.searchResult.defaultSettings.authorText + ' </span>';
            markup += '<a href="' + data.AuthorProfileUrl + '" target="_blank">' + data.AuthorName + '</a>';
        }
                        
        if (data.Tags && data.Tags.length) {
            markup += '&nbsp;&nbsp;&nbsp;<span class="tagSpan">' + dnn.searchResult.defaultSettings.tagsText + ' </span>';
            var k = 0;
            for (k = 0; k < data.Tags.length - 1; k++) {
                markup += '<a href="javascript:void(0)" class="dnnSearchResultItem-tagsLink" data-value="' + data.Tags[k] + '" >' + data.Tags[k] + ',</a>';
            }
            markup += '<a href="javascript:void(0)" class="dnnSearchResultItem-tagsLink" data-value="' + data.Tags[k] + '" >' + data.Tags[k] + '</a>';
        }
        
        markup += '</div>';

        return markup;
    };
    
    dnn.searchResult.renderResults = function (data) {

        var markup = '', totalHits = 0;
        if (data && data.results && data.results.length) {
            var results = data.results;
            totalHits = data.totalHits;
            for (var i = 0; i < results.length; i++) {
                var result = results[i];
                if (result.Results.length > 1) {
                    // grouped
                    var groupedTitle = result.Title;
                    var groupedUrl = result.DocumentUrl;
                    
                    // render grouped title
                    markup += '<div class="dnnSearchResultItem"><div class="dnnSearchResultItem-Title">';
                    markup += '<a href="' + groupedUrl + '"' + dnn.searchResult.defaultSettings.linkTarget + '>' + groupedTitle + '</a></div>';
                    markup += '<div class="dnnSearchResultItem-Link"><a href="' + groupedUrl + '"' + dnn.searchResult.defaultSettings.linkTarget + '>' + groupedUrl + '</a></div></div>';
                    
                    // render subsets
                    for (var j = 0; j < result.Results.length; j++) {
                        markup += '<div class="dnnSearchResultItem-Subset">' + dnn.searchResult.renderResult(result.Results[j]) + '</div>';
                    }

                } else {
                    // non grouped
                    markup += '<div class="dnnSearchResultItem">' + dnn.searchResult.renderResult(result.Results[0], true) + '</div>';
                }
            }
        }
        else {
            markup += '<div class="dnnSearchResultItem">';
            markup += '<div class="dnnSearchResultItem-Title">' + dnn.searchResult.defaultSettings.noresultsText + '</div></div>';
        }

        $('.dnnSearchResultContainer').html(markup);

        // attach tags link events
        $('.dnnSearchResultContainer a.dnnSearchResultItem-tagsLink').on('click', function () {
            var $this = $(this);
            var tag = $this.attr('data-value');
            if (tag) {
                dnn.searchResult.advancedSearchOptions.tags = [];
                dnn.searchResult.advancedSearchOptions.tags.push(tag);
                dnn.searchResult.generateAdvancedSearchTerm();
                dnn.searchResult.queryOptions.pageIndex = 1;
                dnn.searchResult.doSearch();
            }

            return false;
        });

        // attach source link events
        $('.dnnSearchResultContainer a.dnnSearchResultItem-sourceLink').on('click', function () {
            var $this = $(this);
            var source = $this.attr('data-value');
            dnn.searchResult.advancedSearchOptions.types = [];
            dnn.searchResult.advancedSearchOptions.types.push(source);
            dnn.searchResult.generateAdvancedSearchTerm();
            dnn.searchResult.queryOptions.pageIndex = 1;
            dnn.searchResult.doSearch();

            return false;
        });

        // render pager
        if (totalHits > 0) {
            dnn.searchResult.renderPager(totalHits, dnn.searchResult.queryOptions.pageIndex, data.more);
        } else {
            $('.dnnSearchResultPager').hide();
        }
    };

    dnn.searchResult.doSearch = function () {
        var sterm = dnn.searchResult.queryOptions.searchTerm;
        var advancedTerm = dnn.searchResult.queryOptions.advancedTerm;
        if ((!sterm || $.trim(sterm).length <= 1) && (!advancedTerm || $.trim(advancedTerm).length <= 1)) {
            dnn.searchResult.renderResults(null);
            return;
        }

        dnn.searchResult.addLoading();

        var url = dnn.searchResult.queryUrl();
        if (url) {
            $.ajax({
                url: url,
                beforeSend: dnn.searchResult.service.setModuleHeaders,
                success: function (results) {
                    dnn.searchResult.renderResults(results);
                },
                complete: function () {
                    dnn.searchResult.removeLoading();
                },
                type: 'GET',
                dataType: 'json',
                contentType: "application/json"
            });
        }
    };

    dnn.searchResult.generateAdvancedSearchTerm = function () {
        // clean term first
        var term = $('#dnnSearchResult_dnnSearchBox_input').val();
        var advancedTerm = "";
        term = term
                .replace(/\"/gi, '')
                .replace(/^\s+|\s+$/g, '');

        if (dnn.searchResult.advancedSearchOptions.exactSearch && term)
            term = '"' + term + '"';

        var tags = dnn.searchResult.advancedSearchOptions.tags;
        if (tags && tags.length) {

            for (var i = 0; i < tags.length; i++) {
                if (advancedTerm)
                    advancedTerm += ' ';
                advancedTerm += '[' + tags[i] + ']';
            }
        }

        var after = dnn.searchResult.advancedSearchOptions.after;
        if (after) {
            if (advancedTerm)
                advancedTerm += ' ';
            advancedTerm += 'after:' + after;
        }

        var types = dnn.searchResult.advancedSearchOptions.types;
        if (types && types.length) {
            if (advancedTerm)
                advancedTerm += ' ';
            advancedTerm += 'type:' + types.join(',');
        }

        dnn.searchResult.queryOptions.advancedTerm = advancedTerm;
        dnn.searchResult.queryOptions.searchTerm = term;

        var advancedTextCtrl = $('#dnnSearchResult_dnnSearchBox_input').prev().prev();
        var advancedTextClear = $('#dnnSearchResult_dnnSearchBox_input').prev();
        var wrapWidth = $('#dnnSearchResult_dnnSearchBox_input').parent().width();

        if (advancedTerm) {
            advancedTextCtrl.show();
            advancedTextClear.addClass('dnnShow');
            var htmlAdvancedTerm = advancedTerm.replace(/\[/g, '[&nbsp;').replace(/\]/g, '&nbsp;]')
                .replace(/after:/g, '<b>after: </b>').replace(/type:/g, '<b>type: </b>');
            
            var w = advancedTextCtrl.html(htmlAdvancedTerm).width();
            $('#dnnSearchResult_dnnSearchBox_input').val(term).css({
                right: w + 165,
                width: wrapWidth - w - 165 - 8
            });

            //$('#dnnSearchResult_dnnSearchBox_input').next().css({
            //    right: w + 150
            //});

        } else {
            advancedTextCtrl.html('').hide();
            var w1 = $('#dnnSearchResult_dnnSearchBox_input').next().next().next().width();
            $('#dnnSearchResult_dnnSearchBox_input').css({
                right: w1 + 50,
                width: wrapWidth - w1 - 50 - 8
            });

            $('#dnnSearchResult_dnnSearchBox_input').next().css({
                right: w1 + 35
            });
            advancedTextClear.removeClass('dnnShow');
        }

        if (term) {
            $('#dnnSearchResult_dnnSearchBox_input').next().addClass('dnnShow');
        } else {
            $('#dnnSearchResult_dnnSearchBox_input').next().removeClass('dnnShow');
        }
    };

    dnn.searchResult.addLoading = function () {
        var resultContainer = $('.dnnSearchResultContainer');
        var h = resultContainer.outerHeight(), w = resultContainer.outerWidth();
        var loading = $('.dnnSearchLoading');
        if (loading.length == 0) {
            loading = $('<div class="dnnSearchLoading"></div>');
            loading.insertBefore(resultContainer);
        }

        loading.css({ width: w, height: h });
    };

    dnn.searchResult.removeLoading = function () {
        $('.dnnSearchLoading').remove();
    };

    dnn.searchResult.init = function (settings) {

        dnn.searchResult.defaultSettings = $.extend(dnn.searchResult.defaultSettings, settings);

        // search box
        dnn.searchResult.searchInput = $('#dnnSearchResult_dnnSearchBox').dnnSearchBox({
            id: 'dnnSearchResult_dnnSearchBox',
            advancedText: dnn.searchResult.defaultSettings.advancedText,
            showAdvanced: true,
            advancedId: 'dnnSearchResultAdvancedForm',
            enablePreview: false,
            refreshSearchResult: true,
            beforeRefreshSearchResult: function () {
                $('#dnnSearchResult-advancedTipContainer').slideUp('fast');
            },
            moduleId: dnn.searchResult.moduleId,
            searchFunction: function (val) {
                dnn.searchResult.queryOptions.searchTerm = val;
                dnn.searchResult.queryOptions.pageIndex = 1;
                dnn.searchResult.doSearch();
            }
        });

        $('#advancedTagsCtrl').dnnTagsInput({ width: '230px', minInputWidth: '80px', defaultText: dnn.searchResult.defaultSettings.addTagText });

        $('a.dnnSearchResultAdvancedTip').on('click', function () {
            $('#dnnSearchResult-advancedTipContainer').slideToggle('fast');
            return false;
        });

        $('#dnnSearchResultAdvancedSearch').on('click', function (e, isTrigger) {
            var tags = $('#advancedTagsCtrl').val() ? $('#advancedTagsCtrl').val().split(',') : [];

            var afterCtrl = $find(dnn.searchResult.defaultSettings.comboAdvancedDates);
            var afterCtrlVal = afterCtrl.get_value();

            var scopeCtrl = $find(dnn.searchResult.defaultSettings.comboAdvancedScope);
            var scopeCtrlItems = scopeCtrl.get_items();
            var scopeList = [];
            for (var i = 0; i < scopeCtrlItems.get_count() ; i++) {
                var scopeCtrlItem = scopeCtrlItems.getItem(i);
                if (scopeCtrlItem.get_checked()) {
                    scopeList.push(scopeCtrlItem.get_text());
                }
            }
            if (scopeList.length == scopeCtrlItems.get_count())
                scopeList = [];

            var exactSearch = $('#dnnSearchResultAdvancedExactSearch').is(':checked');

            dnn.searchResult.advancedSearchOptions = {
                tags: tags,
                after: afterCtrlVal,
                types: scopeList,
                exactSearch: exactSearch
            };

            dnn.searchResult.generateAdvancedSearchTerm();
            dnn.searchResult.queryOptions.pageIndex = 1;
            dnn.searchResult.doSearch();

            if(!isTrigger) $('.DnnModule .dnnSearchBoxPanel .dnnSearchBox_advanced_label').triggerHandler('click');
            return false;
        });

        $('#dnnSearchResultAdvancedClear').on('click', function () {
            $('#advancedTagsCtrl').dnnImportTags('');

            var afterCtrl = $find(dnn.searchResult.defaultSettings.comboAdvancedDates);
            var afterCtrlItem = afterCtrl.get_items().getItem(0);
            afterCtrlItem.select();

            var scopeCtrl = $find(dnn.searchResult.defaultSettings.comboAdvancedScope);
            var scopeCtrlItems = scopeCtrl.get_items();
            for (var i = 0; i < scopeCtrlItems.get_count() ; i++) {
                var scopeCtrlItem = scopeCtrlItems.getItem(i);
                scopeCtrlItem.set_checked(true);
            }

            $('#dnnSearchResultAdvancedExactSearch').removeAttr('checked');

            dnn.searchResult.advancedSearchOptions = {
                tags: [],
                after: '',
                types: [],
                exactSearch: false
            };

            dnn.searchResult.generateAdvancedSearchTerm();
            dnn.searchResult.queryOptions.pageIndex = 1;
            dnn.searchResult.doSearch();

            $('.DnnModule .dnnSearchBoxPanel .dnnSearchBox_advanced_label').triggerHandler('click');

            return false;
        });

        $('#dnnSearchResult_dnnSearchBox_input').prev().on('click', function () {
            $('#advancedTagsCtrl').dnnImportTags('');

            var afterCtrl = $find(dnn.searchResult.defaultSettings.comboAdvancedDates);
            var afterCtrlItem = afterCtrl.get_items().getItem(0);
            afterCtrlItem.select();

            var scopeCtrl = $find(dnn.searchResult.defaultSettings.comboAdvancedScope);
            var scopeCtrlItems = scopeCtrl.get_items();
            for (var i = 0; i < scopeCtrlItems.get_count() ; i++) {
                var scopeCtrlItem = scopeCtrlItems.getItem(i);
                scopeCtrlItem.set_checked(true);
            }

            $('#dnnSearchResultAdvancedExactSearch').removeAttr('checked');

            dnn.searchResult.advancedSearchOptions = {
                tags: [],
                after: '',
                types: [],
                exactSearch: false
            };

            dnn.searchResult.generateAdvancedSearchTerm();
            dnn.searchResult.queryOptions.pageIndex = 1;
            dnn.searchResult.doSearch();

            return false;
        });

        $('.RadComboBoxDropDown').on('mouseup', function () {
            return false;
        });

        $('.dnnSearchResultSortOptions > li > a').on('click', function () {
            var $this = $(this);
            if ($this.parent().hasClass('active')) return false;

            var sortOption = $this.attr('href');
            $('.dnnSearchResultSortOptions > li').removeClass('active');
            $this.parent().addClass('active');

            switch (sortOption) {
                case '#byDate':
                    dnn.searchResult.queryOptions.sortOption = 1;
                    break;
                default:
                    dnn.searchResult.queryOptions.sortOption = 0;
                    break;
            }

            dnn.searchResult.queryOptions.pageIndex = 1;
            dnn.searchResult.doSearch();
            return false;

        });

        if ($('#advancedTagsCtrl').val()) {
            setTimeout(function() { $('#dnnSearchResultAdvancedSearch').trigger("click", [true])}, 0);
        } else {
            dnn.searchResult.doSearch();
        }
    };

})(jQuery, dnn);