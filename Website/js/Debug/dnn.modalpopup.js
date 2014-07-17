(function(window, $) {
	function parseUri (str) {
		var	o   = parseUri.options,
			m   = o.parser[o.strictMode ? "strict" : "loose"].exec(str),
			uri = {},
			i   = 14;

		while (i--) uri[o.key[i]] = m[i] || "";

		uri[o.q.name] = {};
		uri[o.key[12]].replace(o.q.parser, function ($0, $1, $2) {
			if ($1) uri[o.q.name][$1] = $2;
		});

		return uri;
	};
	
	parseUri.options = {
		strictMode: false,
		key: ["source","protocol","authority","userInfo","user","password","host","port","relative","path","directory","file","query","anchor"],
		q:   {
			name:   "queryKey",
			parser: /(?:^|&)([^&=]*)=?([^&]*)/g
		},
		parser: {
			strict: /^(?:([^:\/?#]+):)?(?:\/\/((?:(([^:@]*)(?::([^:@]*))?)?@)?([^:\/?#]*)(?::(\d*))?))?((((?:[^?#\/]*\/)*)([^?#]*))(?:\?([^#]*))?(?:#(.*))?)/,
			loose:  /^(?:(?![^:@]+:[^:@\/]*@)([^:\/?#.]+):)?(?:\/\/)?((?:(([^:@]*)(?::([^:@]*))?)?@)?([^:\/?#]*)(?::(\d*))?)(((\/(?:[^?#](?![^?#\/]*\.[^?#\/.]+(?:[?#]|$)))*\/?)?([^?#\/]*))(?:\?([^#]*))?(?:#(.*))?)/
		}
	};
				
    window.dnnModal = {

        load: function () {
            
            try {
                if (parent.location.href !== undefined) {

                    var windowTop = parent;
                    var parentTop = windowTop.parent;

                    if (typeof(parentTop.$find) != "undefined") {
                        if (location.href.indexOf('popUp') == -1 || windowTop.location.href.indexOf("popUp") > -1) {

                            var popup = windowTop.jQuery("#iPopUp");
                            var refresh = popup.dialog("option", "refresh");
                            var closingUrl = popup.dialog("option", "closingUrl");
                            var width = popup.dialog("option", "minWidth");
                            var height = popup.dialog("option", "minHeight");
                            var showReturn = popup.dialog("option", "showReturn");

                            if (!closingUrl) {
                                closingUrl = location.href;
                            }

                            if (popup.dialog('isOpen') === true) {

                                popup.dialog("option", {
                                    close: function(event, ui) {
                                        dnnModal.refreshPopup({
                                            url: closingUrl,
                                            width: width,
                                            height: height,
                                            showReturn: showReturn,
                                            refresh: refresh
                                        });
                                    }
                                }).dialog('close');
                            }
                        } else {
                            windowTop.jQuery("#iPopUp").dialog({ autoOpen: false, title: document.title });
                        }
                    }
                }
                return false;
            } catch(err) {
                return true;
            }
        },

        show: function(url, showReturn, height, width, refresh, closingUrl) {
            var $modal = $("#iPopUp");
			
            if ($modal.length) {
				// for ie9+
				$modal[0].src = 'about:blank';
                $modal.remove();
            }

            $modal = $("<iframe id=\"iPopUp\" src=\"about:blank\" scrolling=\"auto\" frameborder=\"0\"></iframe>");
            $(document.body).append($modal);
            $(document).find('html').css('overflow', 'hidden'); 
			
			var ss = document.styleSheets;
			var isAdmin = false;
			for(var i = 0, max = ss.length; i < max; i++){
				if(ss[i].href.indexOf('admin.css') > -1){
					isAdmin = true;
					break;
				}
			}			
			var isMobile = !isAdmin && ($(window).width() < 601 || "ontouchstart" in document.documentElement);
			if (isMobile) $('html').addClass('mobileView');	else $('html').removeClass('mobileView');
			
			var mobileWidth = 0;
			var showLoading = function() {
                var loading = $("<div class=\"dnnLoading\"></div>");
                loading.css({
                    width: $modal.width(),
                    height: $modal.height()
                });
                $modal.before(loading);
            };
            var hideLoading = function() {
                $modal.prev(".dnnLoading").remove();
            };			
			var dialogOpened = function(){				
				$modal.bind("load", function() {
					hideLoading();
					var iframe = document.getElementById("iPopUp");
					var currentHost = window.location.hostname.toLowerCase();
					var currentPort = window.location.port.toLowerCase();
					
					var uri = parseUri(url);
					var iframeHost = uri.host.toLowerCase();
					var iframePort = uri.port.toLowerCase();
					iframeHost = iframeHost? iframeHost : currentHost;	
					iframePort = iframePort? iframePort : currentPort;
					var isSameDomain = currentHost === iframeHost && currentPort === iframePort;
					
					if(isSameDomain){
						try{
							if (isMobile) {		
								var iframeBody = iframe.contentDocument.body,
									iframeHtml = iframe.contentDocument.documentElement;
								iframeHtml.style.width = mobileWidth + 'px';
								iframeBody.className += 'mobileView dnnFormPopup dnnFormPopupMobileView';	
								var iframeHeight = Math.max(iframeBody.scrollHeight, iframeBody.offsetHeight, iframeHtml.clientHeight, iframeHtml.scrollHeight, iframeHtml.offsetHeight);
								$modal.css('height', iframeHeight + 100)
									  .dialog('option', 'position', 'top');
							}
							
							iframe.contentWindow.dnnModal.show = function (sUrl, sShowReturn, sHeight, sWidth, sRefresh, sClosingUrl) {
								var windowTop = parent;
								var popup = windowTop.jQuery("#iPopUp");
								if (!sClosingUrl) {
									sClosingUrl = location.href;
								}

								if (popup.dialog('isOpen')) {
									popup.dialog("option", {
										close: function () {
											parent.dnnModal.show(sUrl, sShowReturn, sHeight, sWidth, sRefresh, sClosingUrl);
										}
									}).dialog('close');
								}
							};
						}
						catch(e){
						}
					}
				});
				
				$modal[0].src = url;
				
			};
			
            if (!isMobile) {
                $modal.dialog({
                    modal: true,
                    autoOpen: true,
                    dialogClass: "dnnFormPopup",
                    position: "center",
                    minWidth: width,
                    minHeight: height,
                    maxWidth: 1920,
                    maxHeight: 1080,
                    resizable: true,
                    closeOnEscape: true,
                    refresh: refresh,
                    showReturn: showReturn,
                    closingUrl: closingUrl,
					open: dialogOpened,
                    close: function() { window.dnnModal.closePopUp(refresh, closingUrl); }
                })
                    .width(width - 11)
                    .height(height - 11);

                if ($modal.parent().find('.ui-dialog-title').next('a.dnnModalCtrl').length === 0) {
                    var $dnnModalCtrl = $('<a class="dnnModalCtrl"></a>');
                    $modal.parent().find('.ui-dialog-titlebar-close').wrap($dnnModalCtrl);
                    var $dnnToggleMax = $('<a href="#" class="dnnToggleMax"><span>Max</span></a>');
                    $modal.parent().find('.ui-dialog-titlebar-close').before($dnnToggleMax);

                    $dnnToggleMax.click(function(e) {
                        e.preventDefault();

                        var $window = $(window),
                            newHeight,
                            newWidth;

                        if ($modal.data('isMaximized')) {
                            newHeight = $modal.data('height');
                            newWidth = $modal.data('width');
                            $modal.data('isMaximized', false);
                        } else {
                            $modal.data('height', $modal.dialog("option", "minHeight"))
                                .data('width', $modal.dialog("option", "minWidth"))
                                .data('position', $modal.dialog("option", "position"));

                            newHeight = $window.height() - 46;
                            newWidth = $window.width() - 40;
                            $modal.data('isMaximized', true);
                        }

                        $modal.dialog({ height: newHeight, width: newWidth });
                        $modal.dialog({ position: 'center' });
                    });
                }
            } else {
                mobileWidth = $(window).width() - 100;
				var originalHeightCss = $('body').css('height');
                $modal.dialog({
                    modal: true,
                    autoOpen: true,
                    dialogClass: "dnnFormPopup dnnFormPopupMobileView",
                    resizable: false,
                    closeOnEscape: true,
                    refresh: refresh,
                    showReturn: showReturn,
                    closingUrl: closingUrl,
                    position: "top",
                    draggable: false,
					open: function() { 
							$('#Form').hide();
							$('body').css('height', 'auto');
							$modal.parent().css({ 'width': 'auto', 'left': '0', 'right': '0', 'top': '0', 'box-shadow': 'none' });
							window.scrollTo(0, 0);
							dialogOpened();
						
						},
                    close: function() { 
							$('#Form').show();
							if(originalHeightCss)
								$('body').css('height', originalHeightCss);
							window.scrollTo(0, 0);
							window.dnnModal.closePopUp(refresh, closingUrl); 
						}
                });
            }
			
            showLoading();
			
            if (showReturn.toString() === "true") {
                return false;
            }
        },

        closePopUp: function(refresh, url) {
            var windowTop = parent; //needs to be assign to a varaible for Opera compatibility issues.
            var popup = windowTop.jQuery("#iPopUp");

            if (typeof refresh === "undefined" || refresh == null) {
                refresh = true;
            }
            
            if (refresh.toString() == "true") {
                if (typeof url === "undefined" || url == "") {
                    url = windowTop.location.href;
                }
                
                windowTop.location.href = url;
                popup.hide();
            } else {
                popup.dialog('option', 'close', null).dialog('close');
            }
            $(windowTop.document).find('html').css('overflow', '');
        },

        refreshPopup: function(options) {
            var windowTop = parent;
            var windowTopTop = windowTop.parent;
            if (windowTop.location.href !== windowTopTop.location.href &&
                windowTop.location.href !== options.url) {
                windowTopTop.dnnModal.show(options.url, options.showReturn, options.height, options.width, options.refresh, options.closingUrl);
            } else {
                dnnModal.closePopUp(options.refresh, options.url);
            }
        }
    };
    
    window.dnnModal.load();
}(window, jQuery));