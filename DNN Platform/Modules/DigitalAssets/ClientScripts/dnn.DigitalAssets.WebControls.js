(function($) {
    // fix telerik custom datepicker popup arrow position       
    $.dnnRadPickerHack = function () {
        //when click this icon, hide error info
        var hideErrorInfo = function () {
            $(this).toggleErrorMessage({ show: false, removeErrorMessage: false });
        };
        var dnnRadPickerPopupFix = function () {
            if ($.support.cssFloat) {
                var id = $(this).attr('id');
                var popupId = id.replace('popupButton', 'calendar_wrapper');
                var popupElement = $('#' + popupId);
                var wrapperId = id.replace('popupButton', 'wrapper');
                var wrapperElement = $('#' + wrapperId);

                var popupElementTop = popupElement.parent().position().top;
                var wrapperElementTop = wrapperElement.offset().top;
                var popupTbl = popupElement.find('.RadCalendar_Default');
                var nextEle = popupTbl.next();
                if (nextEle.hasClass('RadCalendar_Default_PopupArrow_Down') || nextEle.hasClass('RadCalendar_Default_PopupArrow_Up'))
                    nextEle.remove();

                if (popupElementTop < wrapperElementTop) { // popup above the calendar ctrl, so show arrow down
                    popupTbl.after('<div class="RadCalendar_Default_PopupArrow_Down"></div>');
                }
                else { // popup below the calendar ctrl, so show arrow up
                    popupTbl.after('<div class="RadCalendar_Default_PopupArrow_Up"></div>');
                }
            }

            $(this).toggleErrorMessage({ show: false, removeErrorMessage: false });
        };
        $('.RadPicker_Default a.rcCalPopup').unbind('click', dnnRadPickerPopupFix).bind('click', dnnRadPickerPopupFix);
        $('.RadPicker_Default .riTextBox').unbind('focus', hideErrorInfo).bind('focus', hideErrorInfo);
    };

    // remove combobox inline style
    $.dnnComboBoxLoaded = function (sender) {
        if (sender.constructor.__typeName == "Telerik.Web.UI.RadComboBox") {
            $(sender._inputDomElement).closest(".RadComboBox").removeAttr("style");
        }
    };

    //fix combobox hide error info
    $.dnnComboBoxHack = function (sender) {
        $(('#' + sender._clientStateFieldID).replace('_ClientState', '')).toggleErrorMessage({ show: false, removeErrorMessage: false });
    };

    //fix combobox scroll
    $.dnnComboBoxScroll = function (sender) {
        if ($.support.cssFloat) {
            var container = $(('#' + sender._clientStateFieldID + ' .rcbScroll').replace('ClientState', 'DropDown'));
            if (container.data('scrollPane')) {
                container.data('scrollPane').data('jsp').reinitialise();
            } else {
                container.data('scrollPane', container.jScrollPane());
            }
        }
    };

    $.dnnComboBoxItemRequested = function (sender) {
        setTimeout(function () {
            var container = $(('#' + sender._clientStateFieldID + ' .rcbScroll').replace('ClientState', 'DropDown'));
            if (container.data('scrollPane')) {
                container.data('scrollPane').data('jsp').reinitialise();
            }
        }, 0);
    };

    // fix grid issues 
    $.dnnGridCreated = function (sender) {
        var clientId = sender.ClientID;
        var $grid = $('#' + clientId);
        $('input.rgSortDesc, input.rgSortAsc', $grid).click(function () {
            var href = $(this).parent().find('a').get(0).href;
            window.location = href;
            return false;
        });

        if ($grid.hasClass('dnnTooltipGrid')) {
            $grid.dnnHelperTipDestroy();
            $('.rgRow, .rgAltRow', $grid).each(function () {
                var info = "Here is some text will show up and explian more about this information";
                $(this).dnnHelperTip({ helpContent: info, holderId: clientId });
            });
        }

        var grid = $find(clientId);

        // while using customised checkbox - remove onclick event then attach onchange
        var headerCheck = $('.rgCheck', $grid);
        if (headerCheck.length) {
            headerCheck.each(function () {
                var checkbox = $(this).find('input[type="checkbox"]').get(0);
                var onclick = checkbox.onclick;
                checkbox.onchange = onclick;
                checkbox.onclick = null;
            });

            // when use customised scrollbar and customised checkbox, the row cannot be correctly selected
            $('.rgDataDiv input[type="checkbox"]', $grid).change(function () {
                var masterTable = grid.get_masterTableView();
                var rowIndex = $(this).closest('tr').get(0).rowIndex;
                var checked = this.checked;
                if (checked)
                    masterTable.selectItem(rowIndex);
                else
                    masterTable.deselectItem(rowIndex);
            });
        }

        // initialize dnnGrid customised scrollbar
        $('.rgDataDiv').each(function () {
            var $this = $(this);
            var ele = $this.get(0);
            ele.scrollPane = $this.jScrollPane();
            var api = ele.scrollPane.data('jsp');
            var throttleTimeout;
            $(window).bind(
                'resize',
                function () {
                    if (!$.support.cssFloat) {
                        if (throttleTimeout) {
                            clearTimeout(throttleTimeout);
                            throttleTimeout = null;
                        }
                        throttleTimeout = setTimeout(
                            function () {
                                api.reinitialise();
                                throttleTimeout = null;
                            },
                            50
                        );
                    } else {
                        api.reinitialise();
                    }
                }
            );
            if (window.__rgDataDivScrollTopPersistArray && window.__rgDataDivScrollTopPersistArray.length) {
                var y = window.__rgDataDivScrollTopPersistArray.pop();
                api.scrollToY(y);
            }
        });
    };
})(jQuery);