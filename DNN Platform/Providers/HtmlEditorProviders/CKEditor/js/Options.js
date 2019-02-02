(function($, win) {
    win.ShowNotificationBar = function(text, notificationType, imagePath) {
        showNotification({
            type: notificationType,
            message: text,
            autoClose: true,
            duration: 20,
            imagepath: imagePath
        });
    }

    win.pageLoad = function() {
        $('.panelLoading').show();
        $(".copyButton").button({ icons: { primary: "ui-icon-copy" } });
        $(".removeButton").button({ icons: { primary: "ui-icon-trash" } });
        $(".importButton").button({ icons: { primary: "ui-icon-arrowreturnthick-1-s" } });
        $(".exportButton").button({ icons: { primary: "ui-icon-disk" } });
        $(".DefaultButton").button();
        $(".Toolbar").buttonset();

        EnableSorting();

        $("#createGroup").click(function () {
            $(".groups").append('<li class="groupItem">' +
                         '<span class="ui-icon ui-icon-cancel" title="' + win.deleteGroup + '"></span><span class="ui-icon ui-icon-arrowthick-2-n-s"></span>' +
                         '<a href="#" class="groupName" title="' + win.editGroupName + '">' + win.newGroupName + '</a>' +
                                     '<input type="text" class="groupEdit"><div class="ui-state-default ui-corner-all saveGroupName">' +
                                     '<span class="ui-icon ui-icon-check" title="' + win.saveGroupName + '"></div>' +
                                     '<ul class="groupButtons"></ul><div style="clear:both"></div></li>');

            EnableSorting();
        });

        $("#addRowBreak").click(function () {
            $(".groups").append('<li class="groupItem rowBreakItem">' +
                         '<span class="ui-icon ui-icon-cancel" title="' + win.deleteGroup + '"></span><span class="ui-icon ui-icon-arrowthick-2-n-s"></span>' +
                         '<p class="rowBreakLabel">' + win.newRowName + '</p>' +
                         '<a href="#" class="groupName" title="' + win.editGroupName + '">rowBreak</a>' +
                                     '<input type="text" class="groupEdit"><div class="ui-state-default ui-corner-all saveGroupName">' +
                                     '<span class="ui-icon ui-icon-check" title="' + win.saveGroupName + '"></div>' +
                                     '<ul class="groupButtons"><li class="groupButton ui-state-default ui-corner-all rowBreak">' +
                                          '<span class="ui-icon ui-icon-cancel"></span>' +
                                          '<span class="item">/</span>' +
                                         '</li></ul><div style="clear:both"></div></li>');

            EnableSorting();
            $('[id*="Options_ToolbarSet"]').val($('.groups').SerializeToolbars());
        });

        $('#SettingsTabs').tabs({
            activate: function () {
                var sel = $('#SettingsTabs').tabs('option', 'active');
                $('[id*="CKEditor_Options_LastTabId"]').val(sel);
            },
            active: $('[id*="CKEditor_Options_LastTabId"]').val()
        });

        $('#SettingsBox').height($(window).height() - 100);
        $('.ui-tabs .ui-tabs-panel').height($(window).height() - 285);

        $('#ExportDialog').dialog({
            autoOpen: false,
            width: 350,
            buttons: {
                "Cancel": function () {
                    $(this).dialog("close");
                }, 
                "Export Now": function() {
                    window.location = $(".ExportHidden").attr("href"); $(this).dialog("close");
                }
            },
            open: function () {
                $(this).parent().appendTo("form");
            }
        });

        $('#ImportDialog').dialog({
            autoOpen: false,
            width: 350,
            buttons: {
                 "Cancel": function() {
                     $(this).dialog("close");
                 },
                 "Import Now": function() {
                     window.location = $(".ImportHidden").attr("href"); $(this).dialog("close");
                 }
            },
            open: function () {
                $(this).parent().appendTo("form");
            }
        });

        $('#ToolbarGuide').dialog({
            autoOpen: false,
            buttons: {
                "OK": function() {
                    $(this).dialog("close");
                }
            },
            open: function () {
                $(this).parent().appendTo("form");
            }
        });

        if ($(".settingValueInputNumeric").spinner) {
            $(".settingValueInputNumeric").spinner();
        }

        if ($(".settingValueContainer").tooltip) {
            $(".settingValueInputNumeric").tooltip();
        }

        $("#CKEditor_Options_rBlSetMode input").button();
        $("#CKEditor_Options_rBlSetMode").buttonset();
    }

    win.showDialog = function (id) {
        $('#' + id).dialog("open");
    }

    win.EnableSorting = function() {
        if ($(".rowBreakItem .rowBreak").parent("ul").length) {
            $(".rowBreakItem .rowBreak").parent("ul").removeClass("groupButtons");
        }

        $(".groups").sortable({
            connectWith: ".groups, .availableButtons",
            placeholder: "ui-state-highlight",
            update: function () {
                $('[id*="Options_ToolbarSet"]').val($('.groups').SerializeToolbars());
            }
        });

        $(".groupButtons").sortable({
            connectWith: ".groupButtons, .availableButtons",
            placeholder: "ui-state-highlight",
            start: function (e, ui) {
                $('.groupButtons').css('min-height', '50px');
                $('.groupButtons').sortable('refreshPositions');
            },
            update: function () {
                $('[id*="Options_ToolbarSet"]').val($('.groups').SerializeToolbars());
            }
        });

        $(".availableButtons").sortable({
            connectWith: ".groupButtons",
            placeholder: "ui-state-highlight",
            start: function (e, ui) {
                $('.groupButtons').css('min-height', '50px');
                $('.groupButtons').sortable('refreshPositions');
            },
            remove: function (e, ui) {
                if (ui.item.attr('class').indexOf('separator') != -1) {
                    var $separator = ui.item.clone(true);
                    $separator.children(".ui-icon").remove();
                    $(this).append($separator);
                }
                ui.item.prepend('<span class="ui-icon ui-icon-cancel" title="Delete this Toolbar"></span>');
                EnableSorting();
                $('[id*="Options_ToolbarSet"]').val($('.groups').SerializeToolbars());
            },
            update: function (e, ui) {

            }
        });

        $(".groups .ui-icon-cancel,.groupButtons .ui-icon-cancel").hide();

        $(".groups li,.groupButtons li").not(".rowBreak").hover(function () {
            $(this).find(".ui-icon-cancel").eq(0).stop(true, true).fadeIn('fast');
        }, function () {
            $(this).find(".ui-icon-cancel").eq(0).stop(true, true).fadeOut('slow');
        });


        $(".groupItem .saveGroupName").not(".rowBreakItem .saveGroupName").click(function () {
            var input = $(this).prev();
            var a = input.prev();

            a.show();
            input.hide();

            a.html(input.val());

            $(this).hide();

            $('[id*="Options_ToolbarSet"]').val($('.groups').SerializeToolbars());
        });

        $(".groupItem .groupName").not(".rowBreakItem .groupName").click(function () {
            var a = $(this);
            var input = a.next();

            a.hide();
            input.show();

            input.val(a.html()).focus();

            input.next().css('display', 'inline-block');
        });

        $(".groupItem .groupEdit").not(".rowBreakItem .groupEdit").keydown(function (e) {
            if (e.keyCode === $.ui.keyCode.ENTER) {
                $(this).parent().find('.saveGroupName').click();

                return false;
            }
        });

        $(".groups .ui-icon-cancel").click(function () {
            $(this).parent(".groupItem").children(".groupButtons").children("li").each(function (e) {
                var $item = $(this);

                if ($item.attr('class').indexOf('separator') == -1 &&
                    $item.attr('class').indexOf('rowBreak') == -1) {

                    $item.children(".ui-icon").remove();
                    $(".availableButtons").children(".separator").before($item);
                }

                $('[id*="Options_ToolbarSet"]').val($('.groups').SerializeToolbars());

            });

            $(this).parent(".groupItem").remove();
        });

        $(".groupButtons .ui-icon-cancel").click(function () {
            var $item = $(this).parent("li");

            if ($item.attr('class').indexOf('separator') == -1 &&
               $item.attr('class').indexOf('rowBreak') == -1) {
                $item.children(".ui-icon").remove();
                $(".availableButtons").children(".separator").before($item);
            } else {
                $item.remove();
            }

            $('[id*="Options_ToolbarSet"]').val($('.groups').SerializeToolbars());
        });

        $('.panelLoading').hide();
    }

    $.fn.SerializeToolbars = function () {
        var items = [];
        this.children().each(function () {
            var $this = $(this);

            if (!$this.has("li").length) {
                return;
            }

            var buttons = [];
            $this.children("ul").children("li").each(function (e) {
                var html = $(this).children(".item").html();
                buttons.push(html);
            });

            var item = { name: $(this).children("a").html(), items: buttons };
            items.push(item);
        });

        var json = JSON.stringify(items);
        return json.substring(1, json.length - 1).replaceAll("\"name\"", "name").replaceAll("\"items\"", "items");
    };

    String.prototype.replaceAll = function (token, newToken, ignoreCase) {
        var str, i = -1, _token;
        if ((str = this.toString()) && typeof token === "string") {
            _token = ignoreCase === true ? token.toLowerCase() : undefined;
            while ((i = (
                _token !== undefined ?
        str.toLowerCase().indexOf(
        _token,
        i >= 0 ? i + newToken.length : 0
        ) : str.indexOf(
        token,
        i >= 0 ? i + newToken.length : 0
        )
            )) !== -1) {
                str = str.substring(0, i)
            .concat(newToken)
            .concat(str.substring(i + token.length));
            }
        }
        return str;
    };

    $(win).bind('resize', function () {
        $('#SettingsBox').height($(window).height() - 100);
        $('.ui-tabs .ui-tabs-panel').height($(window).height() - 285);
    });

})(jQuery, window);


        