if (!window.DDR)
	window.DDR = {};
if (!DDR.Menu)
	DDR.Menu = {};
if (!DDR.Menu.Providers)
	DDR.Menu.Providers = {};

DDRjQuery(function ($) {
	DDR.Menu.Providers.BaseRenderer = function () {
	}

	DDR.Menu.Providers.BaseRenderer.prototype.baseConstructor = function (jqContainer, dnnNavParams) {
		var me = this;

		me.jqContainer = jqContainer;
		me.dnnNavParams = dnnNavParams;
		me.menus = jqContainer.find("ul").toDDRObjectArray();
		me.items = jqContainer.find("li").toDDRObjectArray();
		me.subMenus = me.menus.filter(function (m) { return (m.level > 0); });
		me.rootItems = me.items.filter(function (i) { return (i.level == 0); });

		me.jqRootMenu = jqContainer.children("ul");
		me.rootMenu = me.jqRootMenu.toDDRObject();
		me.clientID = jqContainer[0].id;

		me.showEffect = dnnNavParams.effect || "slide";
		me.showEffectOptions = eval("(function(){ return " + (dnnNavParams.effectOptions || "{}") + ";})()");
		me.showEffectSpeed = dnnNavParams.effectSpeed || 200;
		me.orientHorizontal = (dnnNavParams.ControlOrientation != "Vertical");

		me.useShim = $.browser.msie && dnnNavParams.shim;
	}

	DDR.Menu.Providers.BaseRenderer.prototype.mapToRendered = function (jq) {
		return $($.map(jq.get(), function (elt) { return $(elt).ddrData().rendered.get(0); }));
	}

	DDR.Menu.Providers.BaseRenderer.prototype.addCovering = function () {
		var me = this;

		var resizeCoverings = new Array();
		var coveringBase = $("<a style='position:absolute;display:block;border:0;padding:0;margin:0;top:0;left:0;bottom:0;right:0;text-decoration:none'><span style='visibility:hidden'/></a>").css("background", $.browser.msie ? "url(" + me.dnnNavParams.PathSystemImage + "spacer.gif)" : "transparent");
		var overrideDimensions = ($.browser.msie && $.browser.version.startsWith("6.")) || !$.support.boxModel;
		me.menus.each(function (m) {
			m.coverings = new Array();
			m.childItems.each(function (i) {
				if (i.coveringHere) {
					var itemText = i.rendered.text();
					i.coveringHere().each(function () {
						var coveringHere = $(this);
						coveringHere.css("position", "relative");
						var covering = coveringBase.clone();
						covering.attr("href", i.href || "javascript:void(0)").children("span").text(itemText);
						coveringHere.prepend(covering);
						covering = coveringHere.children("a:first");
						var parent = covering.offsetParent();
						var chElt = coveringHere[0];

						if (chElt === parent[0] || chElt.offsetParent === null) {
							if (overrideDimensions) {
								covering.css({
									"left": "",
									"right": "",
									"width": coveringHere.outerWidth(false) + "px",
									"height": coveringHere.outerHeight(false) + "px"
								});
							}
						} else {
							chElt.moveCovering = true;
							chElt.covering = covering;
							chElt.coveringHere = coveringHere;
							covering.css({ "width": "0", "height": "0" });
							coveringHere.on("mouseenter", function () {
								m.coverings.each(function (chElt) {
									if (chElt.moveCovering) {
										var chOffset = chElt.coveringHere.offset();
										chElt.covering.css({ "top": "0", "left": "0" });
										var cOffset = chElt.covering.offset();
										chElt.covering.css({
											"top": (chOffset.top - cOffset.top) + "px",
											"left": (chOffset.left - cOffset.left) + "px",
											"width": chElt.coveringHere.outerWidth(false) + "px",
											"height": chElt.coveringHere.outerHeight(false) + "px"
										});
										chElt.moveCovering = false;
									}
								});
							});
							m.coverings.push(chElt);
							resizeCoverings.push(chElt);
						}
					});
				}
			});
		});

		if (resizeCoverings.length > 0) {
			$(window).resize(function () {
				resizeCoverings.each(function (c) {
					c.covering.css({ width: "0", height: "0" });
					c.moveCovering = true;
				});
			});
		}
	}

	DDR.Menu.Providers.BaseRenderer.prototype.prepareHideAndShow = function () {
		var me = this;

		me.hideAllMenus = me.menus.filter(function (m) { return m.flyout; });
		me.setItemsHideAndShow();
		me.attachEvents();
		me.closeUp();
	}

	DDR.Menu.Providers.BaseRenderer.prototype.setItemsHideAndShow = function () {
		var me = this;

		me.items.each(function (i) {
			var allParents = i.allParentMenus;
			var allChildren = i.allChildMenus;
			var childMenu = i.childMenu;

			var hideThese = me.subMenus.filter(function (m) {
				return (m.flyout && !(allParents.contains(m) || allChildren.contains(m)));
			});

			i.hideThese = hideThese;
			i.showThese = new Array();
			if (childMenu && childMenu.flyout)
				i.showThese[0] = childMenu;
		});
	}

	DDR.Menu.Providers.BaseRenderer.prototype.attachEvents = function () {
		var me = this;

		me.menus.each(function (menu) {
			if (!menu.flyout) {
				menu.childItems.each(function (i) {
					if (me.menuItemHover) {
						me.menuItemHover(i);
					}
					i.rendered.hover(
							function () {
								i.hideThese.allRendered().each(function () { this.hideMenu() });
								i.showThese.allRendered().each(function () { this.showMenu() });
							},
							function () {
							}
						);
					i.rendered.focus(function () {
						i.hideThese.allRendered().each(function () { this.hideMenu() });
						i.showThese.allRendered().each(function () { this.showMenu(true) });
					});
				});
			} else {
				var jqMenu = menu.rendered;
				var menuElt = jqMenu[0];

				menuElt.hideMenu = function () {
					jqMenu.stop(true, true);
					if (this.style.display != "none") {
						jqMenu.hide();
						if (menu.shim)
							menu.shim.hide();
					}
				};
				menuElt.showMenu = function (instant) {
					if (this.style.display == "none") {
						jqMenu.stop(true, true);
						menu.childItems.allRendered().stop(true, true).off('mouseenter mouseleave');
						me.positionMenu(menu);
						if (instant || (me.showEffectSpeed == 0)) {
							jqMenu.show();
						} else {
							if (me.showEffect == "none") {
								jqMenu.queue(function () {
									setTimeout(function () { jqMenu.dequeue(); }, me.showEffectSpeed);
								});
								jqMenu.show(1);
							} else if (me.showEffect == "fade") {
								jqMenu.fadeIn(me.showEffectSpeed);
							} else {
								if ((me.showEffect == "slide") || (me.showEffect == "drop")) {
									me.showEffectOptions.direction = menu.slideDirection;
								}
								else {
									me.showEffectOptions.direction = menu.blindDirection;
								}
								jqMenu.show(me.showEffect, me.showEffectOptions, me.showEffectSpeed);
							}
						}
						jqMenu.queue(function () {
							jqMenu.css("display", "none").css("display", "block");
							menu.childItems.each(function (i) {
								if (me.menuItemHover) {
									me.menuItemHover(i);
								}
								i.rendered.hover(
										function () {
											i.hideThese.allRendered().each(function () { this.hideMenu() });
											i.showThese.allRendered().each(function () { this.showMenu() });
										},
										function () {
										}
									);
								i.rendered.focus(function () {
									i.hideThese.allRendered().each(function () { this.hideMenu() });
									i.showThese.allRendered().each(function () { this.showMenu(true) });
								});
							});
							$(this).dequeue();
						});
					}
				};
			}

			menu.rendered.mouseover(function () {
				if (me.timeoutID) {
					window.clearTimeout(me.timeoutID);
					me.timeoutID = null;
				}
			});
			menu.rendered.mouseout(function () {
				if (!me.timeoutID) {
					me.timeoutID = window.setTimeout(function () { me.closeUp(); }, 400);
				}
			});
			menu.rendered.mouseover(function () {
				if (me.timeoutID) {
					window.clearTimeout(me.timeoutID);
					me.timeoutID = null;
				}
			});
			menu.rendered.mouseout(function () {
				if (!me.timeoutID) {
					me.timeoutID = window.setTimeout(function () { me.closeUp(); }, 400);
				}
			});
		});
	}

	DDR.Menu.Providers.BaseRenderer.prototype.positionMenu = function (menu) {
		if (menu.childItems && menu.childItems.length > 0) {
			var me = this;

			var level = menu.level;
			var parentMenu = menu.parentItem.parentMenu;
			var parentItem = menu.parentItem;
			var parentRendered = menu.layout.match(/,menu$/) ? parentMenu.rendered : parentItem.rendered;
			var r = menu.rendered;

			var windowObj = $(window);
			var windowLeft = windowObj.scrollLeft();
			var windowRight = windowLeft + windowObj.width();
			var windowTop = windowObj.scrollTop();
			var windowBottom = windowTop + windowObj.height();

			var prevDisplay = r.css("display");
			r.css({
				"display": "block",
				"width": "auto",
				"height": "auto",
				"overflow-x": "visible",
				"overflow-y": "visible",
				"z-index": 10000 + level * 3
			});
			r.width(r.width());
			r.height(r.height());

			if (parentMenu.layout.match(/^horizontal/)) {
				menu.slideDirection = "up";
				menu.blindDirection = "vertical";

				r.alignElement(
					function () { return menu.childItems[0].rendered.getLeft(2); }, parentRendered.getLeft(2),
					function () { return r.getTop(1); }, parentRendered.getBottom(1)
				);
				if (r.getRight(3) > windowRight) {
					r.alignHorizontal(
						function () { return r.getRight(3); }, windowRight
					);
				}
				if (r.getBottom(3) > windowBottom) {
					r.alignVertical(
						function () { return r.getBottom(1); }, parentRendered.getTop(1)
					);
					menu.slideDirection = "down";
				}
				if (r.getTop(3) < windowTop) {
					r.alignVertical(
						function () { return r.getTop(1); }, parentRendered.getBottom(1)
					);
					r.css({
						"height": windowBottom - r.getTop(4),
						"overflow-x": "visible",
						"overflow-y": "scroll"
					});
					if (r.getRight(3) > windowRight) {
						r.alignHorizontal(
							function () { return r.getRight(3); }, windowRight
						);
					}
					menu.slideDirection = "up";
				}
				else {
					r.alignElement(
						function () { return menu.childItems[0].rendered.getLeft(2); }, parentRendered.getLeft(2),
						function () { return r.getTop(1); }, parentRendered.getBottom(1)
					);
					if (r.getRight(3) > windowRight) {
						r.alignHorizontal(
							function () { return r.getRight(3); }, windowRight
						);
					}
					if (r.getBottom(3) > windowBottom) {
						r.alignVertical(
							function () { return r.getBottom(1); }, parentRendered.getTop(1)
						);
						menu.slideDirection = "down";
					}
					if (r.getTop(3) < windowTop) {
						r.alignVertical(
							function () { return r.getTop(1); }, parentRendered.getBottom(1)
						);
						r.css({
							"height": windowBottom - r.getTop(4),
							"overflow-x": "visible",
							"overflow-y": "scroll"
						});
						if (r.getRight(3) > windowRight) {
							r.alignHorizontal(
								function () { return r.getRight(3); }, windowRight
							);
						}
						menu.slideDirection = "up";
					}
				}
			}
			else {
				menu.slideDirection = "left";
				menu.blindDirection = "horizontal";
				r.alignElement(
					function () { return r.getLeft(1); }, parentRendered.getRight(1),
					function () { return menu.childItems[0].rendered.getTop(2); }, parentRendered.getTop(2)
				);
				if (r.getBottom(3) > windowBottom) {
					r.alignVertical(
						function () { return r.getBottom(3); }, windowBottom
					);
				}
				if (r.getRight(3) > windowRight) {
					r.alignHorizontal(
						function () { return r.getRight(1); }, parentRendered.getLeft(1)
					);
					menu.slideDirection = "right";
				}
				if (r.getLeft(3) < windowLeft) {
					r.alignHorizontal(
						function () { return r.getLeft(1); }, parentRendered.getRight(1)
					);
					menu.slideDirection = "left";
				}
			}

			if (me.useShim) {
				if (!menu.shim) {
					menu.shim = $("<iframe src='javascript:false' frameBorder='0' scrolling='no' />").css({
						"position": "absolute",
						"z-index": 9999 + level * 3,
						"background-color": "transparent",
						"filter": "progid:DXImageTransform.Microsoft.Alpha(style=0,opacity=0)"
					}).appendTo($(document.body));
				}

				menu.shim.css({
					"top": r.css("top"),
					"left": r.css("left"),
					"width": r.outerWidth(true) + "px",
					"height": r.outerHeight(true) + "px",
					"display": "block"
				});
			}

			r.css("display", prevDisplay);
		}
	}

	DDR.Menu.Providers.BaseRenderer.prototype.closeUp = function () {
		var me = this;
		me.hideAllMenus.allRendered().each(function () { this.hideMenu(); });
	}

	DDR.Menu.getCSS = function (elt, css) {
	    return parseFloat("0" + $(elt).css(css));
	};

	if (!Array.prototype.each)
		Array.prototype.each = function (fn) {
			for (var n = 0; n < this.length; n++)
				fn(this[n]);
		}

	if (!Array.prototype.filter)
		Array.prototype.filter = function (fn) {
			var result = new Array();
			for (var n = 0; n < this.length; n++) {
				if (fn(this[n])) {
					result[result.length] = this[n];
				}
			}
			return result;
		}

	if (!Array.prototype.contains)
		Array.prototype.contains = function (o) {
			for (var n = 0; n < this.length; n++) {
				if (this[n] == o) {
					return true;
				}
			}
			return false;
		}

	if (!Array.prototype.allRendered)
		Array.prototype.allRendered = function () {
			return $($.map(this, function (o) { return o.rendered.get(0); }));
		}

	$.fn.extend({
		getLeft: function (index) {
			var o = this;
			//            if (this[0].tagName == "TR")
			//                o = o.children("td:first");
			var result = o.offset().left;
			var htmlThis = o[0];

			if (index == 1)
				result -= DDR.Menu.getCSS(htmlThis, "marginLeft");
			if (index >= 3)
				result += DDR.Menu.getCSS(htmlThis, "borderLeftWidth");
			if (index >= 4)
				result += DDR.Menu.getCSS(htmlThis, "paddingLeft");
			return result;
		},
		getTop: function (index) {
			var o = this;
			//            if (this[0].tagName == "TR")
			//                o = o.children("td:first");
			var result = o.offset().top;
			var htmlThis = o[0];

			if (index == 1)
				result -= DDR.Menu.getCSS(htmlThis, "marginTop");
			if (index >= 3)
				result += DDR.Menu.getCSS(htmlThis, "borderTopWidth");
			if (index >= 4)
				result += DDR.Menu.getCSS(htmlThis, "paddingTop");
			return result;
		},
		getRight: function (index) {
			var result = this.getLeft(4) + this.width();
			var htmlThis = this[0];
			if (index < 4)
				result += DDR.Menu.getCSS(htmlThis, "paddingRight");
			if (index < 3)
				result += DDR.Menu.getCSS(htmlThis, "borderRightWidth");
			if (index < 2)
				result += DDR.Menu.getCSS(htmlThis, "marginRight");
			return result;
		},
		getBottom: function (index) {
			var result = this.getTop(4) + this.height();
			var htmlThis = this[0];
			if (index < 4)
				result += DDR.Menu.getCSS(htmlThis, "paddingBottom");
			if (index < 3)
				result += DDR.Menu.getCSS(htmlThis, "borderBottomWidth");
			if (index < 2)
				result += DDR.Menu.getCSS(htmlThis, "marginBottom");
			return result;
		},
		alignElement: function (horizontalFn, horizontalVal, verticalFn, verticalVal) {
			return this.each(function () {
				var jqThis = $(this);
				var prevDisplay = jqThis.css("display");
				jqThis.css({ "left": "-999px", "top": "-999px", "position": "absolute", "display": "block" });
				horizontalVal -= (999 + horizontalFn());
				verticalVal -= (999 + verticalFn());
				if (prevDisplay != "block")
					jqThis.css("display", prevDisplay);
				jqThis.css({ "left": horizontalVal + "px", "top": verticalVal + "px" });
			});
		},
		alignHorizontal: function (horizontalFn, horizontalVal) {
			return this.each(function () {
				var jqThis = $(this);
				var prevDisplay = jqThis.css("display");
				jqThis.css({ "left": "-999px", "position": "absolute", "display": "block" });
				horizontalVal -= (999 + horizontalFn());
				if (prevDisplay != "block")
					jqThis.css("display", prevDisplay);
				jqThis.css({ "left": horizontalVal + "px" });
			});
		},
		alignVertical: function (verticalFn, verticalVal) {
			return this.each(function () {
				var jqThis = $(this);
				var prevDisplay = jqThis.css("display");
				jqThis.css({ "top": "-999px", "position": "absolute", "display": "block" });
				verticalVal -= (999 + verticalFn());
				if (prevDisplay != "block")
					jqThis.css("display", prevDisplay);
				jqThis.css({ "top": verticalVal + "px" });
			});
		},
		sizeElement: function (horizontalFn, horizontalVal, verticalFn, verticalVal) {
			return this.each(function () {
				var jqThis = $(this);
				var prevDisplay = jqThis.css("display");
				jqThis.css({ "width": "9999px", "height": "9999px", "display": "block" });
				horizontalVal += (9999 - horizontalFn());
				verticalVal += (9999 - verticalFn());
				if (prevDisplay != "block")
					jqThis.css("display", prevDisplay);
				jqThis.css({ "width": horizontalVal + "px", "height": verticalVal + "px" });
			});
		},
		setContentWidth: function (width) {
			return this.each(function () {
				var jqThis = $(this);
				jqThis.width($.support.boxModel ? width : width + jqThis.fullWidth() - jqThis.width());
			});
		},
		setContentHeight: function (height) {
			return this.each(function () {
				var jqThis = $(this);
				jqThis.height($.support.boxModel ? height : height + jqThis.fullHeight() - jqThis.height());
			});
		},
		setFullWidth: function (width) {
			return this.each(function () {
				var jqThis = $(this);
				var delta = jqThis.width() - jqThis.fullWidth();
				jqThis.width(width + delta);
				for (var n = 0; n < 100; n++) {
					var curWidth = jqThis.fullWidth();
					if (curWidth == width)
						return;
					delta += (width - curWidth);
					jqThis.width(width + delta);
				}
			});
		},
		setFullHeight: function (height) {
			return this.each(function () {
				var jqThis = $(this);
				var delta = jqThis.height() - jqThis.fullHeight();
				jqThis.height(height + delta);
				for (var n = 0; n < 100; n++) {
					var curHeight = jqThis.fullHeight();
					if (curHeight == height)
						return;
					delta += (height - curHeight);
					jqThis.height(height + delta);
				}
			});
		},
		fullWidth: function () {
			var result = 0;
			this.each(function () {
				var max = $(this).outerWidth(true);
				if (max > result)
					result = max;
			});
			return result;
		},
		fullHeight: function () {
			var result = 0;
			this.each(function () {
				var max = $(this).outerHeight(true);
				if (max > result)
					result = max;
			});
			return result;
		},
		totalWidth: function () {
			var result = 0;
			this.each(function () {
				result += $(this).outerWidth(true);
			});
			return result;
		},
		totalHeight: function () {
			var result = 0;
			this.each(function () {
				result += $(this).outerHeight(true);
			});
			return result;
		},
		matchWidths: function () {
			return this.setFullWidth(this.fullWidth());
		},
		matchHeights: function () {
			return this.setFullHeight(this.fullHeight());
		},
		lineUpHorizontal: function (columns) {
			var firstElt = $(this[0]);
			var left0 = left = firstElt.getLeft(1);
			var top = firstElt.getTop(2);
			var column = 0;
			return this.each(function () {
				var jqThis = $(this);
				jqThis.alignElement(
						function () { return jqThis.getLeft(1); }, left,
						function () { return jqThis.getTop(2); }, top
					);
				left += jqThis.fullWidth();
				column++;
				if (column == columns) {
					column = 0;
					left = left0;
					top += jqThis.fullHeight();
				}
			});
		},
		lineUpVertical: function () {
			var firstElt = $(this[0]);
			var left = firstElt.getLeft(2);
			var top = firstElt.getTop(1);
			return this.each(function () {
				var jqThis = $(this);
				jqThis.alignElement(
						function () { return jqThis.getLeft(2); }, left,
						function () { return jqThis.getTop(1); }, top
					);
				top += jqThis.fullHeight();
			});
		},
		fitToContent: function () {
			return this.each(function () {
				var maxRight = -999999;
				var maxBottom = -999999;

				var jqThis = $(this);
				var prevDisplay = jqThis.css("display");
				jqThis.css({ "display": "block" });
				jqThis.children().each(function () {
					var jqThis = $(this);
					var right = jqThis.getRight(1);
					var bottom = jqThis.getBottom(1);
					if (right > maxRight)
						maxRight = right;
					if (bottom > maxBottom)
						maxBottom = bottom;
				});

				jqThis.sizeElement(
						function () { return jqThis.getRight(4); }, maxRight,
						function () { return jqThis.getBottom(4); }, maxBottom
					);
				if (prevDisplay != "block")
					jqThis.css("display", prevDisplay);
			});
		},
		stretchBlockHyperlinks: function () {
			this.find("a").each(function () {
				var jqThis = $(this);
				if (jqThis.css("display") == "block") {
					jqThis.setFullWidth(jqThis.parent().width());
					jqThis.setFullHeight(jqThis.parent().height());
				}
			});
		},
		setHoverClass: function (cssClass, alter, prevClass) {
			return this.each(function () {
				if (cssClass) {
					var jqThis = $(this);
					var jqAlter = alter || jqThis;
					jqThis.hover(
									function () {
										jqAlter.each(function () {
											if (!this.hoverClass) {
												this.hoverClass = this.className;
											}
										});
										jqAlter.addClass(cssClass);
										if (prevClass)
											jqAlter.removeClass(prevClass);
									},
									function () {
										jqAlter.each(function () {
											if (this.hoverClass) {
												this.className = this.hoverClass;
											}
										});
									}
								);
				}
			});
		},
		ddrData: function () {
			if (!this.data("ddrData"))
				this.data("ddrData", {});
			return this.data("ddrData");
		},
		setMenuData: function (level, parentItem) {
			return this.each(function () {
				var jqThis = $(this);
				var childItems = jqThis.children("li");
				var id = jqThis.attr("nid");
				var path = "";
				if (parentItem != null) {
					path = parentItem.data("ddrData").path + "-";
				}

				jqThis.data("ddrData", {
					isMenu: true,
					id: id,
					level: level,
					path: path,
					childItems: childItems,
					parentItem: parentItem,
					rendered: jqThis,
					itemIndex: 0
				});
				childItems.setItemData(level, jqThis);
				if (childItems.length > 0) {
					jqThis.children("li:first").ddrData().first = true;
					jqThis.children("li:last").ddrData().last = true;
				}
			});
		},
		setItemData: function (level, parentMenu) {
			this.each(function () {
				var jqThis = $(this);
				var childMenu = jqThis.children("ul");
				var allChildMenus = jqThis.find("ul");
				var allParentMenus = jqThis.parents("ul");
				var jqLink = jqThis.children("a");
				var jqParent = jqLink.length ? jqLink : jqThis;
				var jqImage = jqParent.children("img");
				var jqText = jqParent.children("span");
				var id = jqThis.attr("nid");
				var path = parentMenu.data("ddrData").path + parentMenu.data("ddrData").itemIndex++;

				jqThis.data("ddrData", {
					isItem: true,
					id: id,
					level: level,
					path: path,
					first: false,
					last: false,
					href: jqLink.attr("href"),
					image: jqImage.attr("src"),
					title: jqText.text(),
					isBreadcrumb: jqThis.hasClass("breadcrumb"),
					isSelected: jqThis.hasClass("selected"),
					isSeparator: jqThis.hasClass("separator"),
					childMenu: childMenu,
					parentMenu: parentMenu,
					allChildMenus: allChildMenus,
					allParentMenus: allParentMenus,
					rendered: jqThis
				});
				childMenu.setMenuData(level + 1, jqThis);
			});
		},
		toDDRObject: function () {
			if (this.data("ddrObject"))
				return this.data("ddrObject");

			var data = this.ddrData();
			var result = {};
			this.data("ddrObject", result);
			var n = 0;
			for (var member in data) {
				if ((member != "rendered") && data[member] && data[member].jquery) {
					if (member.substr(member.length - 1) == "s") {
						result[member] = data[member].toDDRObjectArray();
					}
					else if (data[member].length == 0) {
						result[member] = null;
					}
					else {
						result[member] = data[member].toDDRObject();
					}
				}
				else {
					result[member] = data[member];
				}
				n++;
			}

			return result;
		},
		toDDRObjectArray: function () {
			return $.map(this, function (elt) { return $(elt).toDDRObject(); });
		}
	});

	DDR.Menu.createTable = function () {
		return $("<table />").attr("cellpadding", 0).attr("cellspacing", 0).attr("border", 0);
	}
	DDR.Menu.addTableCell = function (table, tdContents) {
		var tr = table.find("tr:first");
		if (tr.length == 0)
			table.append(tr = $("<tr />"));
		var result = $("<td />").css({ "padding": 0, "margin": 0 }).append(tdContents);
		tr.append(result);
		return result;
	}
	DDR.Menu.setTableColumns = function (table, columns) {
		var tbody = table.children("tbody");
		if (tbody.length == 0)
			tbody = table;
		var trs = tbody.children("tr");
		var tds = trs.children("td");
		var tr = $("<tr />");
		tbody.append(tr);
		tds.each(function () {
			var td = $(this);
			tr.append(td);
			if (tr.children("td").length == columns) {
				tr = $("<tr />");
				tbody.append(tr);
			}
		});
		if (tr.children("td").length == 0) {
			tr.remove();
		}
		trs.remove();
	}
});


DDR.Menu.registerMenu = function (clientID, dnnNavParams) {
	document.write(
		"<style type='text/css'>" +
		"	#" + clientID + " {" +
		"		display: none;" +
		"	}" +
		"</style>"
		);

	DDRjQuery(function ($) {
		var jqContainer = $("#" + clientID);
		var rootMenu = jqContainer.children("ul");

		jqContainer.hide();
		while ((rootMenu.children("li").length == 0) && (rootMenu.children().length > 0)) {
			rootMenu.html(rootMenu.children().html());
		}
		if (rootMenu.children().length > 0) {
			rootMenu.setMenuData(0, null);

			new DDR.Menu.Providers[dnnNavParams.MenuStyle](jqContainer, dnnNavParams).createRootMenu();

			jqContainer.css({ "display": "block" });
		}
	});
}
