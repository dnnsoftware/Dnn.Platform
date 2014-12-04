function ShowNotificationBar(text,notificationType,imagePath) {
	showNotification({
		type : notificationType,
		message: text,
		autoClose: true,
		duration: 20,
		imagepath : imagePath
		});
}

function pageLoad() {
	
jQuery('.panelLoading').show();
jQuery(".copyButton").button({ icons: { primary: "ui-icon-copy" } });
jQuery(".removeButton").button({icons: {primary: "ui-icon-trash"}});
jQuery(".importButton").button({icons: {primary: "ui-icon-arrowreturnthick-1-s"}});
jQuery(".exportButton").button({icons: {primary: "ui-icon-disk"}});
jQuery(".DefaultButton").button();

jQuery(".Toolbar").buttonset();

EnableSorting();

jQuery("#createGroup").click(function () {
    jQuery(".groups").append('<li class="groupItem">' +
		         '<span class="ui-icon ui-icon-cancel" title="' + deleteGroup + '"></span><span class="ui-icon ui-icon-arrowthick-2-n-s"></span>' +
		         '<a href="#" class="groupName" title="' + editGroupName + '">' + newGroupName + '</a>' +
							 '<input type="text" class="groupEdit"><div class="ui-state-default ui-corner-all saveGroupName">' +
							 '<span class="ui-icon ui-icon-check" title="' + saveGroupName + '"></div>' +
							 '<ul class="groupButtons"></ul><div style="clear:both"></div></li>');

    EnableSorting();
});

jQuery("#addRowBreak").click(function () {
    jQuery(".groups").append('<li class="groupItem rowBreakItem">' +
		         '<span class="ui-icon ui-icon-cancel" title="' + deleteGroup + '"></span><span class="ui-icon ui-icon-arrowthick-2-n-s"></span>' +
				 '<p class="rowBreakLabel">' + newRowName + '</p>' + 
		         '<a href="#" class="groupName" title="' + editGroupName + '">rowBreak</a>' +
							 '<input type="text" class="groupEdit"><div class="ui-state-default ui-corner-all saveGroupName">' +
							 '<span class="ui-icon ui-icon-check" title="' + saveGroupName + '"></div>' +
							 '<ul class="groupButtons"><li class="groupButton ui-state-default ui-corner-all rowBreak">' +
                                  '<span class="ui-icon ui-icon-cancel"></span>' +
								  '<span class="item">/</span>' +
                                 '</li></ul><div style="clear:both"></div></li>');

    EnableSorting();
	jQuery('[id*="CKEditor_Options_ToolbarSet"]').val(jQuery('.groups').SerializeToolbars());
});

jQuery('#SettingsTabs').tabs(
   {
       activate: function () {
           var sel = jQuery('#SettingsTabs').tabs('option', 'active');
           jQuery('[id*="CKEditor_Options_LastTabId"]').val(sel);
       },
       active: jQuery('[id*="CKEditor_Options_LastTabId"]').val()
   });

jQuery('#SettingsBox').height(jQuery(window).height() - 100);
jQuery('.ui-tabs .ui-tabs-panel').height(jQuery(window).height() - 285);


jQuery('#ExportDialog').dialog({
    autoOpen: false,
    width: 350,
    buttons: { "Cancel": function () { jQuery(this).dialog("close"); }, "Export Now": function () { window.location = jQuery(".ExportHidden").attr("href"); jQuery(this).dialog("close"); } },
    open: function () {
        jQuery(this).parent().appendTo("form");
    }
});
jQuery('#ImportDialog').dialog({
    autoOpen: false,
    width: 350,
    buttons: { "Cancel": function () { jQuery(this).dialog("close"); }, "Import Now": function () { window.location = jQuery(".ImportHidden").attr("href"); jQuery(this).dialog("close"); } },
    open: function () {
        jQuery(this).parent().appendTo("form");
    }
});
jQuery('#ToolbarGuide').dialog({
    autoOpen: false,
    buttons: { "OK": function () { jQuery(this).dialog("close"); } },
    open: function () {
        jQuery(this).parent().appendTo("form");
    }
});

if (jQuery(".settingValueInputNumeric").spinner) {
	jQuery(".settingValueInputNumeric").spinner();
}

if (jQuery(".settingValueContainer").tooltip) {
	jQuery(".settingValueInputNumeric").tooltip();
}

jQuery("#CKEditor_Options_rBlSetMode input").button();
jQuery("#CKEditor_Options_rBlSetMode").buttonset();
        }

        jQuery(window).bind('resize', function () {
jQuery('#SettingsBox').height(jQuery(window).height() - 100);
jQuery('.ui-tabs .ui-tabs-panel').height(jQuery(window).height() - 285);
        });

        function showDialog(id) {
jQuery('#' + id).dialog("open");
        }
        function EnableSorting() {
			if (jQuery(".rowBreakItem .rowBreak").parent("ul").length) {
				jQuery(".rowBreakItem .rowBreak").parent("ul").removeClass("groupButtons");
			}
			
			jQuery(".groups").sortable({
				connectWith: ".groups",
				placeholder: "ui-state-highlight",
				update: function () {
					jQuery('[id*="CKEditor_Options_ToolbarSet"]').val(jQuery('.groups').SerializeToolbars());
		}
});


jQuery(".groupButtons").sortable({
    connectWith: ".groupButtons",
    placeholder: "ui-state-highlight",
    start: function (e, ui) {
        jQuery('.groupButtons').css('min-height', '50px');
        jQuery('.groupButtons').sortable('refreshPositions');
    },
    update: function () {
        jQuery('[id*="CKEditor_Options_ToolbarSet"]').val(jQuery('.groups').SerializeToolbars());
    }
});

jQuery(".availableButtons").sortable({
    connectWith: ".groupButtons",
    placeholder: "ui-state-highlight",
    start: function (e, ui) {
        jQuery('.groupButtons').css('min-height', '50px');
        jQuery('.groupButtons').sortable('refreshPositions');
    },
    remove: function (e, ui) {
        if (ui.item.attr('class').indexOf('separator') != -1) {
			var $separator = ui.item.clone(true);
			$separator.children(".ui-icon").remove();
			jQuery(this).append($separator);
        }
        ui.item.prepend('<span class="ui-icon ui-icon-cancel" title="Delete this Toolbar"></span>');
        EnableSorting();
        jQuery('[id*="CKEditor_Options_ToolbarSet"]').val(jQuery('.groups').SerializeToolbars());
    },
    update: function (e, ui) {

    }
});

jQuery(".groups .ui-icon-cancel,.groupButtons .ui-icon-cancel").hide();

jQuery(".groups li,.groupButtons li").not(".rowBreak").hover(function () {
    jQuery(this).find(".ui-icon-cancel").eq(0).stop(true, true).fadeIn('fast');
}, function () {
    jQuery(this).find(".ui-icon-cancel").eq(0).stop(true, true).fadeOut('slow');
});

jQuery(".groupItem .groupName").not(".rowBreakItem .groupName").click(function () {
    var a = jQuery(this);
    var input = a.next();

    a.hide();
    input.show();

    input.val(a.html()).focus();

    input.next().css('display', 'inline-block');

    jQuery(".saveGroupName").click(function () {
        var input = jQuery(this).prev();
        var a = input.prev();

        a.show();
        input.hide();

        a.html(input.val());

        jQuery(this).hide();

        jQuery('[id*="CKEditor_Options_ToolbarSet"]').val(jQuery('.groups').SerializeToolbars());
    });
});

jQuery(".groups .ui-icon-cancel").click(function() {
    jQuery(this).parent(".groupItem").children(".groupButtons").children("li").each(function(e) {
        var $item = jQuery(this);

        if ($item.attr('class').indexOf('separator') == -1 && 
		    $item.attr('class').indexOf('rowBreak') == -1) {
				
				$item.children(".ui-icon").remove();
				jQuery(".availableButtons").children(".separator").before($item);
        }
		
		jQuery('[id*="CKEditor_Options_ToolbarSet"]').val(jQuery('.groups').SerializeToolbars());

    });

    jQuery(this).parent(".groupItem").remove();
});

jQuery(".groupButtons .ui-icon-cancel").click(function() {
    var $item = jQuery(this).parent("li");

    if ($item.attr('class').indexOf('separator') == -1 && 
       $item.attr('class').indexOf('rowBreak') == -1) {
		   $item.children(".ui-icon").remove();
		   jQuery(".availableButtons").children(".separator").before($item);
    } else {
		   $item.remove();
    }
	
	jQuery('[id*="CKEditor_Options_ToolbarSet"]').val(jQuery('.groups').SerializeToolbars());
});

jQuery('.panelLoading').hide();
        }
		// End Page_Load

        (function ($) {
$.fn.SerializeToolbars = function() {
    var items = [];

    this.children().each(function() {
        var $this = jQuery(this);

        if (!$this.has("li").length) {
return;
        }

        var buttons = [];
        $this.children("ul").children("li").each(function(e) {
var html = jQuery(this).children(".item").html();
buttons.push(html);
        });

        var item = { name: jQuery(this).children("a").html(), items: buttons };
        items.push(item);
    });

    var json = JSON.stringify(items);
    return json.substring(1, json.length - 1).replaceAll("\"name\"", "name").replaceAll("\"items\"", "items");
};
        })(jQuery);

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