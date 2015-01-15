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
	
	var mobileBrowser = (function(){
		var check = false;
		(function(a){if(/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino/i.test(a)||/1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0,4)))check = true})(navigator.userAgent||navigator.vendor||window.opera);
		return check; 
	})();
	
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

            $modal = $("<iframe id=\"iPopUp\" name=\"iPopUp\" src=\"about:blank\" scrolling=\"auto\" frameborder=\"0\"></iframe>");
            $(document.body).append($modal);
            $(document).find('html').css('overflow', 'hidden'); 
			
			var ss = document.styleSheets;
			var isAdmin = false;
			for(var i = 0, max = ss.length; i < max; i++){
				var cssHref = ss[i].href;
				if(typeof cssHref == 'string' && cssHref.indexOf('admin.css') > -1){
					isAdmin = true;
					break;
				}
			}			
			var isMobile = !isAdmin && ($(window).width() < 481 || mobileBrowser);
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
            var dialogOpened = function () {
                
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

                if (typeof $.ui.dialog.prototype.options.open === 'function')
                    $.ui.dialog.prototype.options.open();
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