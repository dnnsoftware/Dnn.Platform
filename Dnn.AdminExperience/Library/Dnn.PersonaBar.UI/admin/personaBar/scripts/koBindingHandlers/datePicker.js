// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

define(['jquery', 'knockout', 'pikaday', 'moment'], function ($, ko, pikaday, moment) {
    'use strict';

    var format = "MM/DD/YYYY";

    ko.bindingHandlers.datePicker = {
        init: function (element, valueAccessor, allBindings) {
            var value = valueAccessor();             
            var valueUnwrapped = ko.unwrap(value);

            var minDate = allBindings.get('minDate');
            if (minDate) {
                minDate.subscribe(function (value) {
                    var picker = $(element).data("picker");
                    var date = !(value instanceof Date) ? new Date() : new Date(value.getTime());
                    date.setHours(0, 0, 0, 0);
                    picker.setMinDate(date);
                    // The calendar is sometimes not correctly refreshed after min date set, until a selected date is set
                    picker.setDate(moment(date).format(format));
                });
            }

            var picker = new pikaday({
                minDate: new Date(),
                defaultDate: valueUnwrapped,
                format: format,
                bound: false,
                onSelect: function () {
                    value(picker.toString());
                }
            });
            $(element).append(picker.el);
            $(element).data("picker", picker);
        },
        update: function (element, valueAccessor) {
            var value = valueAccessor();
            var valueUnwrapped = ko.unwrap(value);
            var picker = $(element).data("picker");
            picker.setDate(valueUnwrapped);
        }
    };
});