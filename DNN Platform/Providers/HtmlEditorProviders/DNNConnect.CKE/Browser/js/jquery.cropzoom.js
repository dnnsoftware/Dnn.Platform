(function (n) {
    function t(n) {
        return n.nodeType == 1 && n.namespaceURI == "http://www.w3.org/2000/svg"
    }
    var r, u, f, e, i, o;
    n.fn.cropzoom = function (t) {
        return this.each(function () {
            function p(n) {
                n.position.top > 0 && (i("image").posY = 0), n.position.left > 0 && (i("image").posX = 0);
                var t = -(i("image").h - n.helper.parent().parent().height()),
                    r = -(i("image").w - n.helper.parent().parent().width());
                n.position.top < t && (i("image").posY = t), n.position.left < r && (i("image").posX = r), s()
            }
            function d() {
                var n = r.image.source.split(".");
                return n[n.length - 1]
            }
            function w() {
                i("image").scaleX = r.width / i("image").w, i("image").scaleY = r.height / i("image").h
            }
            function g() {
                var u, f, n, t;
                r.image.startZoom != 0 ? (u = r.image.width * Math.abs(r.image.startZoom) / 100, f = r.image.height * Math.abs(r.image.startZoom) / 100, i("image").h = f, i("image").w = u, i("image").posY != 0 && i("image").posX != 0 && (i("image").posY = i("image").h > r.height ? Math.abs(r.height / 2 - i("image").h / 2) : r.height / 2 - i("image").h / 2, i("image").posX = i("image").w > r.width ? Math.abs(r.width / 2 - i("image").w / 2) : r.width / 2 - i("image").w / 2)) : (n = i("image").scaleX, t = i("image").scaleY, t < n ? (i("image").h = r.height, i("image").w = Math.round(i("image").w * t)) : (i("image").h = Math.round(i("image").h * n), i("image").w = r.width)), i("image").w < r.width && i("image").h < r.height && (r.image.snapToContainer = !1), s()
            }
            function s() {
                var t = "",
                    r = "";
                n(function () {
                    ft(), n.support.opacity ? (t = "rotate(" + i("image").rotation + "," + (i("image").posX + i("image").w / 2) + "," + (i("image").posY + i("image").h / 2) + ")", r = " translate(" + i("image").posX + "," + i("image").posY + ")", t += r, n(f).attr("transform", t)) : n.support.leadingWhitespace ? (t = "rotate(" + i("image").rotation + "deg)", n(f).css({
                        msTransform: t,
                        top: i("image").posY,
                        left: i("image").posX
                    })) : (t = i("image").rotation, n(f).css({
                        rotation: t,
                        top: i("image").posY,
                        left: i("image").posX
                    }))
                })
            }
            function nt() {
                var t = n("<div />").attr("id", "rotationContainer").mouseover(function () {
                    n(this).css("opacity", 1)
                }).mouseout(function () {
                    n(this).css("opacity", .6)
                }),
                    e = n("<div />").attr("id", "rotationMin").html("0"),
                    o = n("<div />").attr("id", "rotationMax").html("360"),
                    f = n("<div />").attr("id", "rotationSlider"),
                    c = "vertical",
                    h = Math.abs(360 - r.image.rotation);
                r.expose.slidersOrientation == "horizontal" && (c = "horizontal", h = r.image.rotation), f.slider({
                    orientation: c,
                    value: h,
                    range: "max",
                    min: 0,
                    max: 360,
                    step: r.rotationSteps > 360 || r.rotationSteps < 0 ? 1 : r.rotationSteps,
                    slide: function (n, t) {
                        if (i("image").rotation = h == 360 ? Math.abs(360 - t.value) : Math.abs(t.value), s(), r.image.onRotate != null) r.image.onRotate(f, i("image").rotation)
                    }
                }), t.append(e), t.append(f), t.append(o), r.expose.rotationElement != "" ? (f.addClass(r.expose.slidersOrientation), t.addClass(r.expose.slidersOrientation), e.addClass(r.expose.slidersOrientation), o.addClass(r.expose.slidersOrientation), n(r.expose.rotationElement).empty().append(t)) : (f.addClass("vertical"), t.addClass("vertical"), e.addClass("vertical"), o.addClass("vertical"), t.css({
                    position: "absolute",
                    top: 5,
                    left: 5,
                    opacity: .6
                }), u.append(t))
            }
            function tt() {
                var t = n("<div />").attr("id", "zoomContainer").mouseover(function () {
                    n(this).css("opacity", 1)
                }).mouseout(function () {
                    n(this).css("opacity", .6)
                }),
                    o = n("<div />").attr("id", "zoomMin").html("<b>-<\/b>"),
                    h = n("<div />").attr("id", "zoomMax").html("<b>+<\/b>"),
                    e = n("<div />").attr("id", "zoomSlider");
                e.slider({
                    orientation: r.expose.zoomElement != "" ? r.expose.slidersOrientation : "vertical",
                    value: r.image.startZoom != 0 ? r.image.startZoom : it(i("image")),
                    min: r.image.useStartZoomAsMinZoom ? r.image.startZoom : r.image.minZoom,
                    max: r.image.maxZoom,
                    step: r.zoomSteps > r.image.maxZoom || r.zoomSteps < 0 ? 1 : r.zoomSteps,
                    slide: function (t, u) {
                        var l = r.expose.slidersOrientation == "vertical" ? r.image.maxZoom - u.value : u.value,
                            e = r.image.width * Math.abs(l) / 100,
                            o = r.image.height * Math.abs(l) / 100;
                        n.support.opacity ? (n(f).attr("width", e + "px"), n(f).attr("height", o + "px")) : n(f).css({
                            width: e + "px",
                            height: o + "px"
                        });
                        var h = i("image").w / 2 - e / 2,
                            c = i("image").h / 2 - o / 2,
                            a = h > 0 ? i("image").posX + Math.abs(h) : i("image").posX - Math.abs(h),
                            v = c > 0 ? i("image").posY + Math.abs(c) : i("image").posY - Math.abs(c);
                        if (i("image").posX = a, i("image").posY = v, i("image").w = e, i("image").h = o, w(), s(), r.image.onZoom != null) r.image.onZoom(f, i("image"))
                    }
                }), r.slidersOrientation == "vertical" ? (t.append(h), t.append(e), t.append(o)) : (t.append(o), t.append(e), t.append(h)), r.expose.zoomElement != "" ? (o.addClass(r.expose.slidersOrientation), h.addClass(r.expose.slidersOrientation), e.addClass(r.expose.slidersOrientation), t.addClass(r.expose.slidersOrientation), n(r.expose.zoomElement).empty().append(t)) : (o.addClass("vertical"), h.addClass("vertical"), e.addClass("vertical"), t.addClass("vertical"), t.css({
                    position: "absolute",
                    top: 5,
                    right: 5,
                    opacity: .6
                }), u.append(t))
            }
            function it() {
                return i("image").w > i("image").h ? r.image.maxZoom - i("image").w * 100 / r.image.width : r.image.maxZoom - i("image").h * 100 / r.image.height
            }
            function rt() {
                r.selector.centered && (i("selector").y = r.height / 2 - i("selector").h / 2, i("selector").x = r.width / 2 - i("selector").w / 2), e = n("<div/>").attr("id", u[0].id + "_selector").css({
                    width: i("selector").w,
                    height: i("selector").h,
                    top: i("selector").y + "px",
                    left: i("selector").x + "px",
                    border: "1px dashed " + r.selector.borderColor,
                    position: "absolute",
                    cursor: "move"
                }).mouseover(function () {
                    n(this).css({
                        border: "1px dashed " + r.selector.borderColorHover
                    })
                }).mouseout(function () {
                    n(this).css({
                        border: "1px dashed " + r.selector.borderColor
                    })
                }), e.draggable({
                    containment: "parent",
                    iframeFix: !0,
                    refreshPositions: !0,
                    drag: function (n, t) {
                        if (i("selector").x = t.position.left, i("selector").y = t.position.top, a(t), l(), r.selector.onSelectorDrag != null) r.selector.onSelectorDrag(e, i("selector"))
                    },
                    stop: function () {
                        if (r.selector.hideOverlayOnDragAndResize && b(), r.selector.onSelectorDragStop != null) r.selector.onSelectorDragStop(e, i("selector"))
                    }
                }), e.resizable({
                    aspectRatio: r.selector.aspectRatio,
                    maxHeight: r.selector.maxHeight,
                    maxWidth: r.selector.maxWidth,
                    minHeight: r.selector.h,
                    minWidth: r.selector.w,
                    containment: "parent",
                    resize: function (n, t) {
                        if (i("selector").w = e.width(), i("selector").h = e.height(), a(t), l(), r.selector.onSelectorResize != null) r.selector.onSelectorResize(e, i("selector"))
                    },
                    stop: function () {
                        if (r.selector.hideOverlayOnDragAndResize && b(), r.selector.onSelectorResizeStop != null) r.selector.onSelectorResizeStop(e, i("selector"))
                    }
                }), l(e), u.append(e)
            }
            function l() {
                var t = null,
                    u = !1;
                t = e.find("#infoSelector").length > 0 ? e.find("#infoSelector") : n("<div />").attr("id", "infoSelector").css({
                    position: "absolute",
                    top: 0,
                    left: 0,
                    background: r.selector.bgInfoLayer,
                    opacity: .6,
                    "font-size": r.selector.infoFontSize + "px",
                    "font-family": "Arial",
                    color: r.selector.infoFontColor,
                    width: "100%"
                }), r.selector.showPositionsOnDrag && (t.html("X:" + Math.round(i("selector").x) + "px - Y:" + Math.round(i("selector").y) + "px"), u = !0), r.selector.showDimetionsOnDrag && (u ? t.html(t.html() + " | W:" + i("selector").w + "px - H:" + i("selector").h + "px") : t.html("W:" + i("selector").w + "px - H:" + i("selector").h + "px")), e.append(t)
            }
            function ut() {
                n.each(["t", "b", "l", "r"], function () {
                    var t = n("<div />").attr("id", this).css({
                        overflow: "hidden",
                        background: r.overlayColor,
                        opacity: .6,
                        position: "absolute",
                        "z-index": 2,
                        visibility: "visible"
                    });
                    u.append(t)
                })
            }
            function a(n) {
                u.find("#t").css({
                    display: "block",
                    width: r.width,
                    height: n.position.top,
                    left: 0,
                    top: 0
                }), u.find("#b").css({
                    display: "block",
                    width: r.width,
                    height: r.height,
                    top: n.position.top + e.height() + "px",
                    left: 0
                }), u.find("#l").css({
                    display: "block",
                    left: 0,
                    top: n.position.top,
                    width: n.position.left,
                    height: e.height()
                }), u.find("#r").css({
                    display: "block",
                    top: n.position.top,
                    left: n.position.left + e.width() + "px",
                    width: r.width,
                    height: e.height() + "px"
                })
            }
            function b() {
                u.find("#t").hide(), u.find("#b").hide(), u.find("#l").hide(), u.find("#r").hide()
            }
            function v(n, t) {
                u.data(n, t)
            }
            function i(n) {
                return u.data(n)
            }
            function ft() {
                var r = i("image").rotation * Math.PI / 180,
                    n = Math.sin(r),
                    t = Math.cos(r),
                    u = t * i("image").w,
                    f = n * i("image").w,
                    e = -n * i("image").h,
                    o = t * i("image").h,
                    s = t * i("image").w - n * i("image").h,
                    h = n * i("image").w + t * i("image").h,
                    c = Math.min(0, u, e, s),
                    a = Math.max(0, u, e, s),
                    l = Math.min(0, f, o, h),
                    v = Math.max(0, f, o, h);
                i("image").rotW = a - c, i("image").rotH = v - l, i("image").rotY = l, i("image").rotX = c
            }
            function et() {
                var u = n("<table>                <tr>                <td><\/td>                <td><\/td>                <td><\/td>                <\/tr>                <tr>                <td><\/td>                <td><\/td>                <td><\/td>                <\/tr>                <tr>                <td><\/td>                <td><\/td>                <td><\/td>                <\/tr>                <\/table>"),
                    t = [],
                    i;
                for (t.push(n("<div />").addClass("mvn_no mvn")), t.push(n("<div />").addClass("mvn_n mvn")), t.push(n("<div />").addClass("mvn_ne mvn")), t.push(n("<div />").addClass("mvn_o mvn")), t.push(n("<div />").addClass("mvn_c")), t.push(n("<div />").addClass("mvn_e mvn")), t.push(n("<div />").addClass("mvn_so mvn")), t.push(n("<div />").addClass("mvn_s mvn")), t.push(n("<div />").addClass("mvn_se mvn")), i = 0; i < t.length; i++) t[i].mousedown(function () {
                        k(this)
                    }).mouseup(function () {
                        clearTimeout(h)
                    }).mouseout(function () {
                        clearTimeout(h)
                    }), u.find("td:eq(" + i + ")").append(t[i]), n(r.expose.elementMovement).empty().append(u)
            }
            function k(t) {
                if (n(t).hasClass("mvn_no") ? (i("image").posX = i("image").posX - r.expose.movementSteps, i("image").posY = i("image").posY - r.expose.movementSteps) : n(t).hasClass("mvn_n") ? i("image").posY = i("image").posY - r.expose.movementSteps : n(t).hasClass("mvn_ne") ? (i("image").posX = i("image").posX + r.expose.movementSteps, i("image").posY = i("image").posY - r.expose.movementSteps) : n(t).hasClass("mvn_o") ? i("image").posX = i("image").posX - r.expose.movementSteps : n(t).hasClass("mvn_c") ? (i("image").posX = r.width / 2 - i("image").w / 2, i("image").posY = r.height / 2 - i("image").h / 2) : n(t).hasClass("mvn_e") ? i("image").posX = i("image").posX + r.expose.movementSteps : n(t).hasClass("mvn_so") ? (i("image").posX = i("image").posX - r.expose.movementSteps, i("image").posY = i("image").posY + r.expose.movementSteps) : n(t).hasClass("mvn_s") ? i("image").posY = i("image").posY + r.expose.movementSteps : n(t).hasClass("mvn_se") && (i("image").posX = i("image").posX + r.expose.movementSteps, i("image").posY = i("image").posY + r.expose.movementSteps), r.image.snapToContainer) {
                    i("image").posY > 0 && (i("image").posY = 0), i("image").posX > 0 && (i("image").posX = 0);
                    var f = -(i("image").h - u.height()),
                        e = -(i("image").w - u.width());
                    i("image").posY < f && (i("image").posY = f), i("image").posX < e && (i("image").posX = e)
                }
                s(), h = setTimeout(function () {
                    k(t)
                }, 100)
            }
            var u = null,
                h = null,
                e = null,
                f = null,
                o = null,
                r = n.extend(!0, {
                    width: 500,
                    height: 375,
                    bgColor: "#000",
                    overlayColor: "#000",
                    selector: {
                        x: 0,
                        y: 0,
                        w: 229,
                        h: 100,
                        aspectRatio: !1,
                        centered: !1,
                        borderColor: "yellow",
                        borderColorHover: "red",
                        bgInfoLayer: "#FFF",
                        infoFontSize: 10,
                        infoFontColor: "blue",
                        showPositionsOnDrag: !0,
                        showDimetionsOnDrag: !0,
                        maxHeight: null,
                        maxWidth: null,
                        startWithOverlay: !1,
                        hideOverlayOnDragAndResize: !0,
                        onSelectorDrag: null,
                        onSelectorDragStop: null,
                        onSelectorResize: null,
                        onSelectorResizeStop: null
                    },
                    image: {
                        source: "",
                        rotation: 0,
                        width: 0,
                        height: 0,
                        minZoom: 10,
                        maxZoom: 150,
                        startZoom: 0,
                        x: 0,
                        y: 0,
                        useStartZoomAsMinZoom: !1,
                        snapToContainer: !1,
                        onZoom: null,
                        onRotate: null,
                        onImageDrag: null
                    },
                    enableRotation: !0,
                    enableZoom: !0,
                    zoomSteps: 1,
                    rotationSteps: 5,
                    expose: {
                        slidersOrientation: "vertical",
                        zoomElement: "",
                        rotationElement: "",
                        elementMovement: "",
                        movementSteps: 5
                    }
                }, t),
                c, y;
            if (!n.isFunction(n.fn.draggable) || !n.isFunction(n.fn.resizable) || !n.isFunction(n.fn.slider)) {
                alert("You must include ui.draggable, ui.resizable and ui.slider to use cropZoom");
                return
            }
            if (r.image.source == "" || r.image.width == 0 || r.image.height == 0) {
                alert("You must set the source, witdth and height of the image element");
                return
            }
            return u = n(this), v("options", r), u.empty(), u.css({
                width: r.width,
                height: r.height,
                "background-color": r.bgColor,
                overflow: "hidden",
                position: "relative",
                border: "2px solid #333"
            }), v("image", {
                h: r.image.height,
                w: r.image.width,
                posY: r.image.y,
                posX: r.image.x,
                scaleX: 0,
                scaleY: 0,
                rotation: r.image.rotation,
                source: r.image.source,
                bounds: [0, 0, 0, 0],
                id: "image_to_crop_" + u[0].id
            }), w(), g(), v("selector", {
                x: r.selector.x,
                y: r.selector.y,
                w: r.selector.maxWidth != null ? r.selector.w > r.selector.maxWidth ? r.selector.maxWidth : r.selector.w : r.selector.w,
                h: r.selector.maxHeight != null ? r.selector.h > r.selector.maxHeight ? r.selector.maxHeight : r.selector.h : r.selector.h
            }), n.support.opacity ? (o = u[0].ownerDocument.createElementNS("http://www.w3.org/2000/svg", "svg"), o.setAttribute("id", "k"), o.setAttribute("width", r.width), o.setAttribute("height", r.height), o.setAttribute("preserveAspectRatio", "none"), f = u[0].ownerDocument.createElementNS("http://www.w3.org/2000/svg", "image"), f.setAttributeNS("http://www.w3.org/1999/xlink", "href", r.image.source), f.setAttribute("width", i("image").w), f.setAttribute("height", i("image").h), f.setAttribute("preserveAspectRatio", "none"), n(f).attr("x", 0), n(f).attr("y", 0), o.appendChild(f)) : (u[0].ownerDocument.namespaces.add("v", "urn:schemas-microsoft-com:vml", "#default#VML"), n(window).load(function () {
                u[0].ownerDocument.namespaces.add("v", "urn:schemas-microsoft-com:vml", "#default#VML")
            }), o = n("<div />").attr("id", "k").css({
                width: r.width,
                height: r.height,
                position: "absolute"
            }), f = n.support.leadingWhitespace ? document.createElement("img") : document.createElement("v:image"), f.setAttribute("src", r.image.source), f.setAttribute("class", "vml"), f.setAttribute("gamma", "0"), n(f).css({
                position: "absolute",
                left: i("image").posX,
                top: i("image").posY,
                width: i("image").w,
                height: i("image").h
            }), f.setAttribute("coordsize", "21600,21600"), f.outerHTML = f.outerHTML, c = d(), (c == "png" || c == "gif") && (f.style.filter = "progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + r.image.source + "',sizingMethod='scale');"), o.append(f)), u.append(o), s(), n(f).draggable({
                refreshPositions: !0,
                start: function () {},
                drag: function (t, u) {
                    if (n.support.opacity ? (i("image").posY = u.position.top - n(document).scrollTop() + (i("image").rotH - i("image").h) / 2, i("image").posX = u.position.left - n(document).scrollLeft() + (i("image").rotW - i("image").w) / 2) : (i("image").posY = u.position.top, i("image").posX = u.position.left), r.image.snapToContainer ? p(u) : s(), r.image.onImageDrag != null) r.image.onImageDrag(f)
                },
                stop: function (n, t) {
                    r.image.snapToContainer && p(t)
                }
            }), rt(), u.find(".ui-icon-gripsmall-diagonal-se").css({
                background: "#FFF",
                border: "1px solid #000",
                width: 8,
                height: 8
            }), ut(), r.selector.startWithOverlay && (y = {
                position: {
                    top: e.position().top,
                    left: e.position().left
                }
            }, a(y)), r.enableZoom && tt(), r.enableRotation && nt(), r.expose.elementMovement != "" && et(), n.fn.cropzoom.getParameters = function (t, i) {
                var r = t.data("image"),
                    u = t.data("selector"),
                    f = {
                        viewPortW: t.width(),
                        viewPortH: t.height(),
                        imageX: r.posX,
                        imageY: r.posY,
                        imageRotate: r.rotation,
                        imageW: r.w,
                        imageH: r.h,
                        imageSource: r.source,
                        selectorX: u.x,
                        selectorY: u.y,
                        selectorW: u.w,
                        selectorH: u.h
                    };
                return n.extend(f, i)
            }, n.fn.cropzoom.getSelf = function () {
                return u
            }, this
        })
    }, r = n.fn.addClass, n.fn.addClass = function (i) {
        return i = i || "", this.each(function () {
            if (t(this)) {
                var u = this;
                n.each(i.split(/\s+/), function (t, i) {
                    var r = u.className ? u.className.baseVal : u.getAttribute("class");
                    n.inArray(i, r.split(/\s+/)) == -1 && (r += (r ? " " : "") + i, u.className ? u.className.baseVal = r : u.setAttribute("class", r))
                })
            } else r.apply(n(this), [i])
        })
    }, u = n.fn.removeClass, n.fn.removeClass = function (i) {
        return i = i || "", this.each(function () {
            if (t(this)) {
                var r = this;
                n.each(i.split(/\s+/), function (t, i) {
                    var u = r.className ? r.className.baseVal : r.getAttribute("class");
                    u = n.grep(u.split(/\s+/), function (n) {
                        return n != i
                    }).join(" "), r.className ? r.className.baseVal = u : r.setAttribute("class", u)
                })
            } else u.apply(n(this), [i])
        })
    }, f = n.fn.toggleClass, n.fn.toggleClass = function (i, r) {
        return this.each(function () {
            t(this) ? (typeof r != "boolean" && (r = !n(this).hasClass(i)), n(this)[(r ? "add" : "remove") + "Class"](i)) : f.apply(n(this), [i, r])
        })
    }, e = n.fn.hasClass, n.fn.hasClass = function (i) {
        i = i || "";
        var r = !1;
        return this.each(function () {
            if (t(this)) {
                var u = (this.className ? this.className.baseVal : this.getAttribute("class")).split(/\s+/);
                r = n.inArray(i, u) > -1
            } else r = e.apply(n(this), [i]);
            return !r
        }), r
    }, i = n.fn.attr, n.fn.attr = function (r, u, f) {
        var o, e;
        return typeof r == "string" && u === undefined ? (o = i.apply(this, [r, u, f]), o && o.baseVal ? o.baseVal.valueAsString : o) : (e = r, typeof r == "string" && (e = {}, e[r] = u), this.each(function () {
            if (t(this)) for (var o in e) this.setAttribute(o, typeof e[o] == "function" ? e[o]() : e[o]);
            else i.apply(n(this), [r, u, f])
        }))
    }, o = n.fn.removeAttr, n.fn.removeAttr = function (i) {
        return this.each(function () {
            t(this) ? this[i] && this[i].baseVal ? this[i].baseVal.value = "" : this.setAttribute(i, "") : o.apply(n(this), [i])
        })
    }, n.fn.extend({
        setSelector: function (t, i, r, u, f) {
            var e = n(this);
            f != undefined && f == !0 ? e.find("#" + e[0].id + "_selector").animate({
                top: i,
                left: t,
                width: r,
                height: u
            }, "slow") : e.find("#" + e[0].id + "_selector").css({
                top: i,
                left: t,
                width: r,
                height: u
            }), e.data("selector", {
                x: t,
                y: i,
                w: r,
                h: u
            })
        },
        restore: function () {
            var i = n(this),
                t = i.data("options");
            i.empty(), i.data("image", {}), i.data("selector", {}), t.expose.zoomElement != "" && n(t.expose.zoomElement).empty(), t.expose.rotationElement != "" && n(t.expose.rotationElement).empty(), t.expose.elementMovement != "" && n(t.expose.elementMovement).empty(), i.cropzoom(t)
        },
        send: function (t, i, r, u) {
            var f = n(this);
            n.ajax({
                url: t,
                type: i,
                data: f.cropzoom.getParameters(f, r),
                success: function (n) {
                    f.data("imageResult", n), u !== undefined && u != null && u(n)
                }
            })
        },
        PreviewParams: function () {
            var i = n(this),
                t = i.data("image"),
                r = i.data("selector");
            return "viewPortW=" + i.width() + "&viewPortH=" + i.height() + "&imageX=" + t.posX + "&imageY=" + t.posY + "&imageRotate=" + t.rotation + "&imageW=" + t.w + "&imageH=" + t.h + "&imageSource=" + t.source + "&selectorX=" + r.x + "&selectorY=" + r.y + "&selectorW=" + r.w + "&selectorH=" + r.h
        }
    })
})(jQuery)