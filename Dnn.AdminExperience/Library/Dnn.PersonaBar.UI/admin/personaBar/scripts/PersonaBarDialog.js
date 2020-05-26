// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

/*jshint multistr:true */

/*
 * @class PersonaBarDialog
 *
 * Class for show Dialgo on personaBar, you must have PersonaBarDialog.css
 *
 * @param {object} options:
 * {
 *   {jQuery object} inObject (null), object for get innerHeight (visible height of screen)
 *   {bool} animation (true), fadeIn dialog on show
 *   {int} width (650), with of dialog content
 *   {string} title ('Notice'), Title of dialog
 *   {string} innerTitle (''), Title or description on the top of content
 *   {string} acceptBtnLbl ('Accept), Text for accept button, defualt 'Accept'
 *   {string} acceptBtnLbl ('Accept'), Text for acept button
 *   {string} cancelBtnLbl ('Cancel'), Text for cancel button
 *   {bool} showAcceptBtn (true), Show or not Accept button
 *   {bool} closeOnAccept (true), Set if dialog must be closed on accept button
 *   {bool} showCancelBtn (true), Show or not Cancel button
 *   {function} onCancelCallback (null), function to call on cancel button click
 *   {function} onAcceptCallback (true), Callback for on click accept button
 *   {function} beforeCloseCallback (null), function called on close but before remove dialog from DOM
 *   {function} onCloseCallback (null), function called on close dialog (and removed from DOM), normally you want use it to enable/disable buttons
 *   {function} enableAcceptForDialog (null), view *1
 *  }
 *  @param {function} koObserveCallback (null), callback for initialize your object, must return the object
 *  @param {function} afterBindCallback (null), function called after bind and before show dialog
 *
 *  *1 For now, there is an option that must be passed on returned viewModel in koObeserveCallback with name enableAcceptForDialog, for pass the function that enable/disable accept
 *  button if you want to use ko validation for this. Ex: On your return viewModel => _viewModel.enableAcceptForDialog = _viewModel.stateName.isValid;
 */

define(['jquery', 'knockout', 'jquery-ui.min', 'css!cssPath/personaBarDialog.css'], function ($, ko) {
    var PersonaBarDialogClass;

    PersonaBarDialogClass = (function IIFE() {
        'use strict';

        var _options, _html, _viewModel, _htmlForPannels, _personaBarDialogMask, _personaBarDialog, _height, _docHeight, _bottomPercent, _originalHeight, _top, _win;

        var addDialog, applyBindings, closeDialog, acceptDialog, initDialog, showDialog, updateHeight;

        /* Class Properties */
        PersonaBarDialog.class = 'PersonaBarDialog';
        PersonaBarDialog.type  = 'Class';
        PersonaBarDialog.active = false; // TODO: Improve dialog with stacked dialogs

        /* Private Properties */
        // Dialog template. htmlForPannels param will be bind inside dialogHTML,
        // for have information in gray blocks, ad to each block "panel" class
        _html = '<div class="personaBarDialogMask" style="display:none;"></div>\
                <div class="personaBarDialog" style="display:none;">\
                    <div class="container">\
                        <div class="title dialogTitle">\
                            <span class="title" data-bind="html: title"></span>\
                            <span class="btn-close" data-bind="click: closeDialog"></span>\
                        </div>\
                        <div class="content">\
                            <div class="title" data-bind="html: innerTitle"></div>\
                            <div class="panels" data-bind="with: dialogHTML"></div>\
                        </div>\
                    </div>\
                    <div class="actions">\
                      <span class="btn btn-accept" data-bind="visible: showAcceptBtn, text: acceptBtnLbl, click: acceptDialog"></span>\
                      <span class="btn btn-cancel" data-bind="visible: showCancelBtn, text: cancelBtnLbl, click: closeDialog"></span>\
                    </div>\
                </div>';
        _options = null;
        _viewModel = null;
        _personaBarDialog = null;
        _personaBarDialogMask = null;

        /* Costructor */
        function PersonaBarDialog(options, htmlForPannels, koObserveCallback, afterBindCallback) {
            //console.log('~PersonaBarDialog');
            if (PersonaBarDialog.active) {
                console.log('PersonaBarDialog already opened'); // TODO throw ex or change for stack support
                return;
            }

            PersonaBarDialog.active = true;

            // Defaults (must compare falsy values)
            if (!options) {
                options = {
                    inObject: $(document.body),
                    animation: true,
                    width: 650,
                    title: 'Notice',
                    innerTitle: '',
                    showAcceptBtn: true,
                    acceptBtnLbl: 'Accept',
                    showCancelBtn: true,
                    cancelBtnLbl: 'Cancel',
                    onAcceptCallback: null,
                    beforeCloseCallback: null,
                    onCloseCallback: null,
                    closeOnAccept: true
                };
            }

            _options = options;

            if (!options.width) {_options.width = 650;}
            if (!options.title) {_options.title = 'Notice';}
            if (!options.inObject) {_options.inObject = $(document.body);}
            if (!options.innerTitle) {_options.innerTitle = '';}
            if (!options.acceptBtnLbl) {_options.acceptBtnLbl = 'Accept';}
            if (options.showAcceptBtn !== false) {_options.showAcceptBtn = true;}
            if (!options.cancelBtnLbl) {_options.cancelBtnLbl = 'Cancel';}
            if (options.showCancelBtn !== false) {_options.showCancelBtn = true;}
            if (options.animation !== false) {_options.animation = true;}
            if (options.closeOnAccept !== false) {_options.closeOnAccept = true;}

            _viewModel = {};

            if (!htmlForPannels) htmlForPannels = '';

            // Add to DOM
            addDialog();

            // Add to queue
            setTimeout(function () {
                initDialog();
                var personaBarDialog = $('.personaBarDialog');

                personaBarDialog.width(_options.width);
                if (htmlForPannels) personaBarDialog.find('.panels').prepend(htmlForPannels);

                // Add to queue
                setTimeout(function () {
                    if (typeof koObserveCallback === 'function') applyBindings(koObserveCallback());
                    else applyBindings(undefined);

                    setTimeout(function () {
                        if (typeof afterBindCallback === 'function') afterBindCallback();
                    }, 0);

                    showDialog();
                }, 0);
            }, 0);
        }

        /* Private Methods */
        applyBindings = function (viewModel) {
            setTimeout(function () {
                _viewModel = {
                    dialogHTML: '',
                    title:         ko.observable(_options.title),
                    innerTitle:    ko.observable(_options.innerTitle),
                    acceptBtnLbl:  ko.observable(_options.acceptBtnLbl),
                    showAcceptBtn: ko.observable(_options.showAcceptBtn),
                    cancelBtnLbl:  ko.observable(_options.cancelBtnLbl),
                    showCancelBtn: ko.observable(_options.showCancelBtn),
                    acceptDialog:  acceptDialog,
                    closeDialog:   closeDialog,
                    enableAccept:  ko.observable(true)
                };

                if (typeof viewModel === 'object') {
                    _viewModel.dialogHTML = viewModel;
                    if (viewModel.enableAcceptForDialog) {
                        _viewModel.enableAccept = viewModel.enableAcceptForDialog;
                    }
                }

                ko.applyBindings(_viewModel, _personaBarDialog[0]);
            }, 0);
        };

        addDialog = function () {
            return $('body').prepend(_html);
        };

        closeDialog = function () {
            var node;

            // Remove events
            if (_win) _win.off('.personaBarDialog');
            if (_personaBarDialog) _personaBarDialog.off('DOMMouseScroll mousewheel');

            node = $('.personaBarDialog');
            if (typeof _options.beforeCloseCallback === 'function') _options.beforeCloseCallback();

            if (node.length > 0) {
                ko.cleanNode($('.personaBarDialog')[0]);
            }
            _options.inObject.height(_originalHeight);
            _personaBarDialog.remove();
            _personaBarDialogMask.remove();
            if (typeof _options.onCloseCallback === 'function') _options.onCloseCallback();

            PersonaBarDialog.active = false;
        };

        acceptDialog = function () {
            if (typeof _viewModel.enableAccept === 'boolean' && !_viewModel.enableAccept) return;
            if (typeof _viewModel.enableAccept === 'function' && !_viewModel.enableAccept()) return;
            if (typeof _options.onAcceptCallback === 'function') _options.onAcceptCallback();
            if (_options.closeOnAccept === true) closeDialog();
        };

        initDialog = function () {
            var doc, bottom, left, top, scrollDown, currentWinPos, lastWinPos, topPercent, percent;

            _win = $(window);
            doc = $(document);

            _personaBarDialogMask = $('.personaBarDialogMask');
            _personaBarDialog = $('.personaBarDialog');

            _personaBarDialog.draggable({
                handle: '.dialogTitle',
                cursor: 'move'
            });

            left = '-' + Math.round(_options.width / 2) + 'px';
            top = 100 + doc.scrollTop() + 'px';
            bottom = '40px';

            _personaBarDialog.css({
                top: top,
                'margin-left': left,
                'margin-bottom': bottom,
            });

            _personaBarDialogMask.css({
                height: doc.height() + _personaBarDialog.height()
            });

            setTimeout(function () {
                _top = doc.scrollTop();
                _personaBarDialogMask.css({
                    'min-height': doc.scrollTop() + _personaBarDialog.height() + 100 + 'px'
                });
            }, 0);

            // Prevent scroll to top window
            _win.off('scroll.personaBarDialog').on('scroll.personaBarDialog', function () {
                if (doc.scrollTop() < _top) doc.scrollTop(_top);
            });

            // Close on esc keydown
            _win.off('keydown.personaBarDialog').on('keydown.personaBarDialog', function(evt) {
                if (evt.keyCode === 27) closeDialog();
            });

            // Prevent doble scroll from inner dialog and outer window
            _personaBarDialog.off('DOMMouseScroll mousewheel').on('DOMMouseScroll mousewheel', function(evt) {
                var self, scrollTop, scrollHeight, height, delta, up, prevent;
                self = $(this);

                scrollTop    = this.scrollTop;
                scrollHeight = this.scrollHeight;
                height       = self.height();
                delta        = evt.originalEvent.wheelDelta;
                up = delta > 0;

                prevent = function() {
                    evt.stopPropagation();
                    evt.preventDefault();
                    evt.returnValue = false;
                    return false;
                };

                if (!up && -delta > scrollHeight - height - scrollTop) {
                   self.scrollTop(scrollHeight);
                    return prevent();
                } else if (up && delta > scrollTop) {
                    self.scrollTop(0);
                    return prevent();
                }
            });
        };

        showDialog = function () {
            _personaBarDialogMask.show();
            if (_options.animation) {_personaBarDialog.fadeIn(100);} else {_personaBarDialog.show();}
        };

        updateHeight = function () {
            _personaBarDialogMask.css({
                'min-height': $(document).scrollTop() + _personaBarDialog.height() + 400 + 'px'
            });
        };

        /* Public Methods */
        PersonaBarDialog.prototype.updateHeight = updateHeight;
        PersonaBarDialog.prototype.close = closeDialog;

        return PersonaBarDialog;
    })();

    return PersonaBarDialogClass;
});
