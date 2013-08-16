(function ($) {
    $.fn.previewEmulator = function (options) {
    	var handler = this;
	    
    	if (handler.data("previewEmulator")) {
    		return handler.data("previewEmulator");
    	}
	    
        var hDimension = this.find(".dimension_h");
        var vDimension = this.find(".dimension_v");
        var viewContainer = this.find(".emulator_c");
        var viewAddr = this.find(".emulator_addr");
        var viewer = this.find("iframe");
        var sendAgent = true;

        var bindElement = function () {
            viewContainer = handler.find(".emulator_c");
            viewer = handler.find("iframe");

            viewContainer.scroll();

            viewContainer.bind("resize", function (e) {
                var width = $(this).width();
                var height = $(this).height();

                hDimension.width(width);
                vDimension.height(height);
                viewAddr.width(width);

                var hContentWidth = width - hDimension.find(".left").width() - hDimension.find(".right").width();
                hDimension.find(".center").width(hContentWidth).html(width + "px");

                var vContentHeight = height - vDimension.find(".top").height() - vDimension.find(".bottom").height();
                vDimension.find(".middle").height(vContentHeight).css("line-height", vContentHeight + "px").html(height + "px");

                if (viewer.width() < viewContainer.width()) {
                    viewer.width(viewContainer.width());
                }

                if (viewer.height() < viewContainer.height()) {
                    viewer.height(viewContainer.height());
                }

                $(this).scroll();

            });

            viewer.bind("load", function () {
                var frameWin = this.contentWindow;
                var frameDoc = frameWin.document;

                var body = frameDoc.documentElement != undefined ? frameDoc.documentElement : frameDoc.body;
                var width = body.scrollWidth;
                var height = body.scrollHeight;

                $(this).width(width).height(height);

                viewAddr.html(frameWin.dnnPreviewUrl);

                viewContainer.trigger("resize");
            });
        };

        bindElement();

        var updateViewer = function (userAgent) {
        	var url = location.href + (location.href.indexOf("?") > -1 ? "&" : "?") + "UserAgent=" + userAgent + "&SendAgent=" + sendAgent;
            viewer.attr("src", url);
        };

        this.setPreview = function (width, height, userAgent) {
            updateViewer(userAgent);
            viewContainer.width(width).height(height);
            viewContainer.trigger("resize");
        };

        this.showDimension = function (show) {
            var visible = show ? "visible" : "hidden";
            hDimension.css("visibility", visible);
            vDimension.css("visibility", visible);
        };

        this.previewWithAgent = function(send) {
            sendAgent = send;
        };

        hDimension.html("<span class=\"left\"></span><span class=\"center\"></span><span class=\"right\"></span>");
        vDimension.html("<span class=\"top\"></span><span class=\"middle\"></span><span class=\"bottom\"></span>");
	    
        handler.data("previewEmulator", this);

        return this;
    };

    $.fn.scroll = function (options) {
        var handler = this;
        var scrollTemplate = "<div class=\"scroll\" unselectable=\"on\"><div class=\"bar\" unselectable=\"on\"></div></div>";
        var mask = $("<div class=\"mask\"></div");
        var hScroll = this.find(".horizontal");
        var vScroll = this.find(".vertical");
        var contentArea = this.find("iframe");

        if (hScroll.length == 0) {
            hScroll = $(scrollTemplate).addClass("horizontal").appendTo(handler);
        };

        if (vScroll.length == 0) {
            vScroll = $(scrollTemplate).addClass("vertical").appendTo(handler);
        };

        var hScrollBar = hScroll.find(".bar");
        var vScrollBar = vScroll.find(".bar");

        var init = function () {
            initVScroll();
            initHScroll();
            initHandler();
        };

        var initHandler = function () {
            handler.attr("unselectable", "on");
            scrollTo(0, 0);

            if (contentArea.width() <= handler.width()) {
                hScroll.hide();
            } else {
                hScroll.show();
            }

            if (contentArea.height() <= handler.height()) {
                vScroll.hide();
            } else {
                vScroll.show();
            }
        };

        var initVScroll = function () {
            vScroll.height(handler.height() - hScroll.height());
            handler.data("viewHeight", handler.height() - hScroll.height());

            vScrollBar.height(parseInt((handler.data("viewHeight") / contentArea.height()) * vScroll.height()));

            vScrollBar.unbind("mousedown").mousedown(function (e) {
                startDragOnY($(this), e);
            });
        };

        var initHScroll = function () {
            hScroll.width(handler.width() - vScroll.width());
            handler.data("viewWidth", handler.width() - vScroll.width());

            hScrollBar.width(parseInt((handler.data("viewWidth") / contentArea.width()) * hScroll.width()));
            if (hScroll.next(".status").length === 0) {
                hScroll.after("<div class=\"status\"></div>");
            }

            hScrollBar.unbind("mousedown").mousedown(function (e) {
                startDragOnX($(this), e);
            });
        };

        var startDragOnY = function (bar, event) {
            initDrag(bar);
            bar.data("position", event.pageY);
            $(document).mousemove(function (e) {
                if (bar.data("position") !== undefined && bar.data("position") !== null) {
                    var offset = e.pageY - bar.data("position");
                    scrollOnY(offset);

                    bar.data("position", e.pageY);
                }
            }).mouseup(function (e) {
                endDrag(bar);
            });
        };

        var startDragOnX = function (bar, event) {
            initDrag(bar);
            bar.data("position", event.pageX);
            $(document).mousemove(function (e) {
                if (bar.data("position") !== undefined && bar.data("position") !== null) {
                    var offset = e.pageX - bar.data("position");
                    scrollOnX(offset);

                    bar.data("position", e.pageX);
                }
            }).mouseup(function (e) {
                endDrag(bar);
            });
        };

        var initDrag = function (bar) {
            mask.appendTo(handler).width(handler.data("viewWidth")).height(handler.data("viewHeight")).css({ opacity: 0 });
        };

        var endDrag = function (bar) {
            bar.data("position", null);
            $(document).unbind();

            mask.remove();
        };

        var scrollOnY = function (offset) {
            var margin = parseInt(vScrollBar.css("top")) + offset;
            if (margin >= 0 && margin <= vScroll.height() - vScrollBar.height()) {
                vScrollBar.css({ top: margin + "px" });

                var percent = margin / (vScroll.height() - vScrollBar.height());
                var contentTop = (contentArea.height() - handler.data("viewHeight")) * percent * -1;

                contentArea.css({ top: contentTop + "px" });
            }
        };

        var scrollOnX = function (offset) {
            var margin = parseInt(hScrollBar.css("left")) + offset;
            if (margin >= 0 && margin <= hScroll.width() - hScrollBar.width()) {
                hScrollBar.css({ left: margin + "px" });

                var percent = margin / (hScroll.width() - hScrollBar.width());
                var contentLeft = (contentArea.width() - handler.data("viewWidth")) * percent * -1;

                contentArea.css({ left: contentLeft + "px" });
            }
        };

        var scrollTo = function (x, y) {
            var offsetX = x - parseInt(hScrollBar.css("margin-left"));
            scrollOnX(offsetX);

            var offsetY = y - parseInt(vScrollBar.css("margin-top"));
            scrollOnY(offsetY);
        };

        init();

        this.data("scroll", "initialed");
        return this;
    };
})(jQuery);