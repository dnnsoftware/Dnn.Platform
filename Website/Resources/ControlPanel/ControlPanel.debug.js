(function ($) {
	$.fn.dnnControlPanel = function (options) {
		var opts = $.extend({}, $.fn.dnnControlPanel.defaultOptions, options),
        $wrap = this;

		return $wrap.each(function () {

			var $controlPanel = $(this);

			$controlPanel.wrap(function () {
				return opts.wrappingHtml;
			});

            if ($('[id$="CommonTasksPanel"]').length > 0) $('[id$="CommonTasksPanel"]').detach().appendTo('#dnnCommonTasks .megaborder');
            else $('#dnnCommonTasks').remove();
            $('[id$="CurrentPagePanel"]').detach().appendTo('#dnnCurrentPage .megaborder');
            if ($('[id$="AdminPanel"]').length > 0) $('[id$="AdminPanel"]').detach().appendTo('#dnnOtherTools .megaborder');
            else $('#dnnOtherTools').remove();


			var $wrapper = $('#dnnCPWrap');
			if ($wrapper.hasClass('Pinned')) {
				$wrapper.parent().css({ paddingBottom: '0' });
			}
			else {
				$(document.body).css({ marginTop: $wrapper.outerHeight() });
			}

			$.fn.dnnControlPanel.show = function () {
				$controlPanel.parents(opts.controlPanelWrapper).slideDown();
			}

			$.fn.dnnControlPanel.hide = function () {
				$controlPanel.parents(opts.controlPanelWrapper).slideUp();
			}


			var canHide = false,
			    forceShow = false,
                $controlPanel = $(this);

			function EnableHide() {
				if (!forceShow) canHide = true;
			}

			function megaHoverOver() {
				hideAll();
				var menuSelector = $(this).parent().find(opts.menuBorderSelector);
				if (!$.support.cssFloat && menuSelector.prev("iframe").length == 0) {
					var mask = $("<iframe frameborder=\"0\"></iframe>");
					menuSelector.before(mask);
					mask.css({
						position: "absolute"
								, width: menuSelector.outerWidth() + "px"
								, height: menuSelector.outerHeight() + "px"
								, left: menuSelector.position().left + "px"
								, top: menuSelector.position().top + "px"
								, opacity: 0
					});
				}
				menuSelector.stop().fadeTo('fast', 1).show(); //Find sub and fade it in
			}

			function hideAll() {
				$controlPanel.find(opts.menuBorderSelector).stop().fadeTo('fast', 0, function () { //Fade to 0 opacity
					if (!$.support.cssFloat) $(this).prev("iframe").remove();
					$(this).hide();  //after fading, hide it
				});
			}

			//Set custom configurations
			//This is the config used for showing the menue
			var config = {
				sensitivity: 2, // number = sensitivity threshold (must be 1 or higher)
				interval: 200, // number = milliseconds for onMouseOver polling interval
				over: megaHoverOver, // function = onMouseOver callback (REQUIRED)
				timeout: 100, // number = milliseconds delay before onMouseOut
				out: function () { return null; } // function = onMouseOut callback (REQUIRED)
			};

			//This is the config used for hiding the menue
			var hideConfig = {
				sensitivity: 2, // number = sensitivity threshold (must be 1 or higher)
				interval: 1, // number = milliseconds for onMouseOver polling interval
				over: function () { return; }, // function = onMouseOver callback (REQUIRED)
				timeout: 300, // number = milliseconds delay before onMouseOut
				out: function () {
					if (canHide) hideAll();
				} // function = onMouseOut callback (REQUIRED)
			};

			$controlPanel.find(opts.menuBorderSelector).css({ 'opacity': '0' }); //Fade sub nav to 0 opacity on default
			$controlPanel.find(opts.menuBorderSelector).find(opts.primaryActionSelector).css({ opacity: 1 }); //Compact IE7

			$controlPanel.find(opts.menuItemSelector).mouseenter(EnableHide); //Hovering over CP will re-enable hiding.

			//hide menu if user double clicks while they are outside of the
			//control panel
			$(document).dblclick(function () {
				if (!canHide)
					hideAll();
			});

			$controlPanel.dblclick(function (e) {
				e.stopPropagation();
			});

			//Hide menu if Esc key is pressed
			$(document).keyup(function (e) {
				if (e.keyCode == 27) {
					hideAll();
				}
			});

			$controlPanel.find(opts.menuItemActionSelector).hoverIntent(config); //Trigger Hover intent with custom configurations
			$controlPanel.find(opts.menuItemSelector).hoverIntent(hideConfig);

			//Hovering over a the dropdown will re-enable autohide
			$controlPanel.find(opts.enableAutohideOnFocusSelector).focus(function () {
				canHide = true;
			});

			//Hovering over a telerik dropdown will disable autohide
			//need this to disable hide when the drop down expands beyond the menu body
			$controlPanel.on('mouseover', opts.persistOnMouseoverSelector, function () {
				canHide = false;
			});

			$controlPanel.on('click', opts.persistOnWaitForChangeSelector, function () {
				forceShow = true;
				canHide = false;

				setTimeout(function () {
					$(document).one("click", function() {
						forceShow = false;
						canHide = true;
					});
				}, 0);
			});

			$controlPanel.on('focus', opts.persistOnFocusSelector, function () {
				canHide = false;
			});

		});
	};

	$.fn.dnnControlPanel.defaultOptions = {
		wrappingHtml: '<div id="dnnCPWrap"><div id="dnnCPWrapLeft"><div id="dnnCPWrapRight"></div></div></div>',
		persistOnFocusSelector: 'select, input',
		persistOnMouseoverSelector: '.rcbSlide li',
		persistOnWaitForChangeSelector: '.RadComboBox',
		enableAutohideOnFocusSelector: '.dnnadminmega .megaborder',
		menuItemActionSelector: '.dnnadminmega > li > a',
		menuItemSelector: '.dnnadminmega > li',
		menuBorderSelector: '.megaborder',
		primaryActionSelector: '.dnnPrimaryAction',
		controlPanelWrapper: '#dnnCPWrap'
	};

	$(document).ready(function () {
		$('.dnnControlPanel').dnnControlPanel();
	});

})(jQuery);