(function (k, e) {
    k.d = {
        load: function () {
            try {
                if (void 0 !== parent.location.href) {
                    var a = parent; if ("undefined" != typeof a.parent.l) if (-1 == location.href.indexOf("popUp") || -1 < a.location.href.indexOf("popUp")) { var d = a.e("#iPopUp"), b = d.a("option", "refresh"), e = d.a("option", "closingUrl"), g = d.a("option", "minWidth"), h = d.a("option", "minHeight"), c = d.a("option", "showReturn"); e || (e = location.href); !0 === d.a("isOpen") && d.a("option", { close: function () { dnnModal.j({ url: e, width: g, height: h, f: c, refresh: b }) } }).a("close") } else a.e("#iPopUp").a({
                        g: !1,
                        title: document.title
                    })
                } return !0
            } catch (l) { return !1 }
        }, show: function (a, d, b, f, g, h) {
            var c = e("#iPopUp"); 0 == c.length ? (c = e('<iframe id="iPopUp" src="about:blank" scrolling="auto" frameborder="0"></iframe>'), e(document.body).append(c)) : c.n("src", "about:blank"); e(document).find("html").c("overflow", "hidden"); c.a({ r: !0, g: !0, p: "dnnFormPopup", position: "center", minWidth: f, minHeight: b, maxWidth: 1920, maxHeight: 1080, s: !0, o: !0, refresh: g, f: d, i: h, close: function () { dnnModal.b(g, h) } }).width(f - 11).height(b - 11); b = dnn.m(e(".ui-widget-overlay")[0]);
            null != b && (b.style.zIndex = 1); 0 === c.parent().find(".ui-dialog-title").next("a.dnnModalCtrl").length && (b = e('<a class="dnnModalCtrl"></a>'), c.parent().find(".ui-dialog-titlebar-close").t(b), b = e('<a href="#" class="dnnToggleMax"><span>Max</span></a>'), c.parent().find(".ui-dialog-titlebar-close").h(b), b.click(function (a) {
                a.preventDefault(); var b = e(k); c.data("isMaximized") ? (a = c.data("height"), b = c.data("width"), c.data("isMaximized", !1)) : (c.data("height", c.a("option", "minHeight")).data("width", c.a("option",
                "minWidth")).data("position", c.a("option", "position")), a = b.height() - 46, b = b.width() - 40, c.data("isMaximized", !0)); c.a({ height: a, width: b }); c.a({ position: "center" })
            })); (function () { var a = e('<div class="dnnLoading"></div>'); a.c({ width: c.width(), height: c.height() }); c.h(a) })(); c[0].src = a; if ("true" == d.toString()) return !1
        }, b: function (a, d) {
            var b = parent, f = b.e("#iPopUp"); if ("undefined" === typeof a || null == a) a = !0; if ("true" == a.toString()) {
                if ("undefined" === typeof d || "" == d) d = b.location.href; b.location.href = d; 0 < e(".ui-widget-overlay").length &&
                dnn.k(e(".ui-widget-overlay")[0]); f.q()
            } else 0 < e(".ui-widget-overlay").length && dnn.k(e(".ui-widget-overlay")[0]), f.a("option", "close", null).a("close"); e(b.document).find("html").c("overflow", "")
        }, j: function (a) { var d = parent, b = d.parent; d.location.href !== b.location.href && d.location.href !== a.url ? b.d.show(a.url, a.f, a.height, a.width, a.refresh, a.i) : dnnModal.b(a.refresh, a.url) }
    }; k.d.load()
})(window, jQuery);