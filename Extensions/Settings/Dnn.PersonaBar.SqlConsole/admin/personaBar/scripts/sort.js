//#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//#endregion

define(['jquery', 'knockout'], function ($, ko) {
    'use strict';

    var changeSort = function (e) {
        var $element = $(e.currentTarget);
        var $arrow = $element.find('.sort');
        var viewModel = ko.dataFor($element.parent().parent().parent()[0]);
        var column = ko.dataFor($element[0]);

        $arrow.removeClass('asc desc');

        if (viewModel.sortColumn() !== column) {
            viewModel.sortType(0);

            $element.removeClass('current');
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

        if (sortType === 1 || sortType === 2) {
            $arrow.addClass(sortType === 1 ? 'asc' : 'desc');
        }
    }

    var init = function(element, valueAccessor, allBindings, viewModel, bindingContext) {
        var $element = $(element);

        $element.addClass('table-sortable')
            .append('<span class="sort"></span>')
            .click(changeSort);
    }

    var update = function(element, valueAccessor, allBindings, viewModel, bindingContext) {}


    ko.bindingHandlers.sort = {
        init: init,
        update: update
    };
});