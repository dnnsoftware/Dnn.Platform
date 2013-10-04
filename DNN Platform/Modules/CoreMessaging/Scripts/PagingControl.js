// DotNetNuke® - http://www.dotnetnuke.com
//
// Copyright (c) 2002-2013 DotNetNuke Corporation
// All rights reserved.

(function ($) {
    /// <summary>Paging as in 'Page 1, 2, 3, ...'</summary>
    window.PagingControl = function PagingControl (ko, settings, root) {
        var that = this;

        $.extend(this, PagingControl.DefaultSettings, settings);

        // Start of "visible set" of pages which can be shifted by clicking the ellipses
        this.setStart = ko.observable(0);

        // Current page index (zero-based)
        this.page = ko.observable(settings.pageIndex || 0);
        this.localizationController = root.localizer();

        this.totalPages = function () {
            if (root != null) {
                var total;
                
                switch (typeof root.totalResults) {
                    case 'function':
                        total = root.totalResults();
                        break;
                    case 'undefined':
                    default:
                        total = root.totalResults;
                        break;
                }

                if (total > 0) {
                    return Math.max(1, Math.ceil(total / that.pageSize));
                }
            }

            return 0;
        };

        this.first = function () {
            that.navigateTo(0);

            that.setStart(that.page());

            return true;
        };

        this.previous = function () {
            if (that.page() > 0) {
                that.navigateTo(that.page() - 1);
            }

            that.setStart(that.page());

            return true;
        };

        this.next = function () {
            if (that.page() + 1 < that.totalPages()) {
                that.navigateTo(that.page() + 1);
            }

            that.setStart(that.page());

            return true;
        };

        this.last = function () {
            if (that.totalPages() > 0) {
                that.navigateTo(that.totalPages() - 1);

                that.setStart(that.page());
            }

            return true;
        };

        this.setPage = function (page) {
            that.navigateTo(page);

            return true;
        };

        // Shift the set of visible page links over to the right.
        this.shiftSetRight = function () {
            if (that.totalPages() - that.setStart() > that.pageSetSize) {
                that.setStart(that.setStart() + that.pageSetSize);
            }
        };

        this.shiftSetLeft = function () {
            if (that.setStart() > 0) {
                var start = that.setStart() - that.pageSetSize + 1;
                if (start < 0) {
                    start = 0;
                }

                that.setStart(start);
            }
        };

        // Can the visible page set be shifted left?
        this.leftVisible = function () {
            var set = that.getVisibleSet();

            return set[0] > 0;
        };

        // Can the visible page set be shifted right?
        this.rightVisible = function () {
            var set = that.getVisibleSet();

            // More pages exist after what is visible in the visible page set?        
            return set[set.length - 1] + 1 < that.totalPages();
        };

        this.getVisibleSet = function () {
            var set = [];
            var currentPage = that.setStart() + 1;
            var totalPages = that.totalPages();
            var visibleSetNumber = totalPages > 4 ? 3: totalPages - 2;
            if(currentPage < 4){                
                for(var i = 0; i< visibleSetNumber; i++){
                    set.push(i + 2);
                }
            }
            else if (currentPage > totalPages - 3) {
                for (var i = visibleSetNumber; i > 0; i--) {
                    set.push(totalPages - i);
                }
            }
            else {
                set.push(currentPage - 1);
                set.push(currentPage);
                set.push(currentPage + 1);
            }

            return set;
        };

        this.getString = function (key) {
                return that.localizationController.getString(key);
        };

        // We need to register an object that is reachable from the global scope so that we can register clicks for the
        // previous, next, and page buttons (data-bind="click" is not available since the markup will not be evaluated
        // by knockout; even if it were available, we don't want to force the module developer to use knockout just to
        // enable our paging control. We can use it ourselves though.)
        this.registerRootObject = function (uniqueId) {
            var key = 'pagingController' + uniqueId;

            if (typeof window[key] === 'undefined') {
                window[key] = that;
            }

            return 'window[\'{0}\']'.replace("{0}", key);
        };

        var rootObj = that.registerRootObject(settings.moduleId + '_subscription_' + Math.random());

        // The paging markup to go at the bottom of the search results.
        this.markup = function () {
            var total = that.totalPages();
            if (total > 1) {
                var currentPage = that.page();

                var html = new String();
                var currentPageIdx = currentPage + 1;
                var pageDesc = that.getString('PageDescription').replace("{0}", currentPageIdx).replace("{1}", total);
                html += '<li class="pager-ul-desc">' + pageDesc + '</li>';

                if (currentPageIdx > 1) {
                    html += '<li><a href="javascript:void(0)" onclick="return ' + rootObj + '.first()">1</a></li>';
                }
                else {
                    html += '<li><span class="disabled">1</span></li>';
                }

                var set = that.getVisibleSet();

                if (set.length > 0) {

                    if (set[0] > 2) {
                        html += '<li><a href="javascript:void(0)" onclick="' + rootObj + '.shiftSetLeft()">&hellip;</a></li>';
                    }

                    for (var i = 0; i < set.length; i++) {
                        if (currentPageIdx == set[i]) {
                            html += '<li class="current"><span>' + currentPageIdx + '</span></li>';
                        }
                        else {
                            html += '<li><a href="javascript:void(0)" onclick="' + rootObj + '.setPage(' + (set[i] - 1) + ')">' + set[i] + '</a></li>';
                        }
                    }

                    if (set[set.length - 1] < total - 1) {
                        html += '<li><a href="javascript:void(0)" onclick="' + rootObj + '.shiftSetRight()">&hellip;</a></li>';
                    }
                }

                if (currentPageIdx < total) {
                    html += '<li class="pager-ul-last"><a href="javascript:void(0)" onclick="' + rootObj + '.last()">' + total + '</a></li>';
                }
                else {
                    html += '<li class="pager-ul-last"><span class="disabled">' + total + '</span></li>';
                }

                return html;
            }

            return null;
        };

        this.navigateTo = function (page) {
            if (page + 1 > that.totalPages()) {
                return true;
            }
            
            that.page(page);

            // Get the current state but change the page value.
            that.setNavigationProperty('page', page || 0);

            return true;
        };
	    
		this.setNavigationProperty = function (property, value) {
            var state = window.History.getState();

            state.data[property] = value.toString();

            that.history.pushState(state.data, null, that.getUrl(state.data));
        };

        this.page.subscribe(
            function () {
                while (that.page() < that.setStart()) {
                    that.shiftSetLeft();
                }
                
                while (that.page() > that.setStart() + that.pageSetSize) {
                    that.shiftSetRight();
                }
            });
    };

    PagingControl.DefaultSettings = {
        pageSetSize: 5,
        pageSize: 10
    };
})(window.jQuery);