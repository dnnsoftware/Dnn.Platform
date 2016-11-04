// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

define(['jquery', 'knockout'], function ($, ko) {
    'use strict';

    var changeSort = function (e) {
        var $element = $(e.target);
        var viewModel = ko.dataFor($element.parent()[0]);
        var column = ko.dataFor($element[0]);


        if (viewModel.sortColumn() !== column) {
            viewModel.sortType(0);

            $element.parent().find('.table-sortable.current').removeClass('current');
        }

        viewModel.sortColumn(column);

        var sortType = viewModel.sortType();
        switch (sortType) {
            case 0:
                viewModel.sortType(1);
                break;
            case 1:
                viewModel.sortType(2);
                break;
            case 2:
                viewModel.sortType(0);
                break;
            default:
                viewModel.sortType(1);
                break;
        }
              
        sortType = viewModel.sortType();

        viewModel.currentPage(1);
        $element.addClass('current');
        $element.find('span.sort').removeClass('asc desc');
        if (sortType === 1 || sortType === 2) {
            $element.find('span.sort').addClass(sortType === 1 ? 'asc' : 'desc');
        }
    }

    var init = function(element, valueAccessor, allBindings, viewModel, bindingContext) {
        var $element = $(element);

        $element.addClass('table-sortable')
            .append('<span class="sort"></span>')
            .click(changeSort);
    }

    var update = function(element, valueAccessor, allBindings, viewModel, bindingContext) {
    }


    ko.bindingHandlers.sort = {
        init: init,
        update: update
    };
});