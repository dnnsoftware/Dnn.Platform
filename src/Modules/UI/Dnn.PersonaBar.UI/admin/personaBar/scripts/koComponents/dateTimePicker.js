// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

define(['knockout', 'moment',
    'main/koBindingHandlers/datePicker'], function (ko, moment) {
    'use strict';

    function composeDate(datePart, hour, amPm) {
        var dateStr = datePart + " " + hour + amPm;
        return moment(dateStr, "MM/DD/YYYY hh:mmA").toDate();
    }

    ko.components.register('datetime-picker', {
        viewModel: function (params) {
            var self = this;

            this.resx = params.resx;
            this.date = params.date;
            this.minDate = params.minDate;
            
            this.datePart = ko.computed({
                read: function() {
                    return !self.date() ? "" : moment(self.date()).format("MM/DD/YYYY");
                },
                write: function (value) {
                    if (self.datePart() !== value) {
                        self.date(composeDate(value, self.hour(), self.amPm()));    
                    }                    
                }
            });

            this.hour = ko.computed({
                read: function() {
                    return !self.date() ? "12:00" : moment(self.date()).format("hh:mm");
                },
                write: function (value) {                    
                    self.date(composeDate(self.datePart(), value, self.amPm()));                    
                }
            });

            this.amPm = ko.computed({
                read: function() {
                    return !self.date() ? "AM" : moment(self.date()).format("A");
                },
                write: function (value) {
                    self.date(composeDate(self.datePart(), self.hour(), value));                    
                }
            });
        },
        template: { require: 'text!../koComponents/dateTimePicker.html' }
    });
});