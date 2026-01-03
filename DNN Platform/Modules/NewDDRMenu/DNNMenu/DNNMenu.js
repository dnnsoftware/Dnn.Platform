if (!DDR.Menu.Providers.DNNMenu) {
    DDRjQuery(function ($) {
        DDR.Menu.Providers.DNNMenu = function (jqContainer, dnnNavParams) {
            var me = this;

            me.baseConstructor(jqContainer, dnnNavParams);
        }
        DDR.Menu.Providers.DNNMenu.prototype = new DDR.Menu.Providers.BaseRenderer();

        DDR.Menu.Providers.DNNMenu.prototype.createRootMenu = function () {
            var me = this;

            var outerContainer = $("<span />");
            var dnnNavContainer = me.createRenderedMenu(me.rootMenu);
            dnnNavContainer.addClass(me.dnnNavParams.CSSControl);
            outerContainer.append(dnnNavContainer);

            me.subMenus.each(function (m) {
                dnnNavContainer.append(me.createRenderedMenu(m));
            });

            me.jqContainer.replaceWith(outerContainer);

            me.jqContainer.show(1);
            me.jqContainer.queue(function () {
                me.addCovering();
                me.prepareHideAndShow();

                $(this).dequeue();
            });
        }

        DDR.Menu.Providers.DNNMenu.prototype.createRenderedMenu = function (menu) {
            var me = this;

            var level = menu.level;
            var childItems = menu.childItems;

            if (level == 0) {
                menu.flyout = false;
                menu.layout = me.orientHorizontal ? "horizontal" : "vertical";
                var result = $("<span />");
                childItems.each(function (i) {
                    result.append(me.createRenderedItem(i));
                });
            }
            else {
                menu.flyout = true;
                menu.layout = "vertical";
                var parentItem = menu.parentItem;
                var parentMenu = parentItem.parentMenu;

//                var result = $("<table cellspacing='0' cellpadding='0' border='0' />").css({ "position": "absolute", "left": "-1000px" });
                var result = $("<div><table cellspacing='0' cellpadding='0' border='0' /></div>").css({ "position": "absolute", "left": "-1000px" });
                var table = result.children("table");
                table.addClass(this.dnnNavParams.CSSContainerSub);
                table.addClass("m");
                table.addClass("m" + (menu.level - 1));
                table.addClass("mid" + menu.id);
                childItems.each(function (i) {
                    table.append(me.createRenderedItem(i));
                });
            }

            menu.rendered = result;

            return result;
        };

        DDR.Menu.Providers.DNNMenu.prototype.createRenderedItem = function (item) {
            var me = this;

            var level = item.level;
            var title = item.title;
            var image = item.image;
            var href = item.href;
            var separator = item.separator;

            var result;

            if (level == 0) {
                result = me.orientHorizontal ? $("<span><span /><span /></span>") : $("<div><span /><span /></div>");
                var spanImg = result.children("span:eq(0)");
                var spanText = result.children("span:eq(1)");

                result.addClass("root");

                if (href && !item.isSeparator) {
                    item.coveringHere = function () { return item.rendered; };
                }

                spanImg.addClass("icn");
                if (image) {
                    spanImg.append($("<img />").attr("src", image));
                }

                spanText.addClass("txt");
                spanText.css("cursor", "pointer").text(title);

                var nodeLeftHTML = me.dnnNavParams.NodeLeftHTMLRoot || "";
                var nodeRightHTML = me.dnnNavParams.NodeRightHTMLRoot || "";
                var separatorLeftHTML = me.dnnNavParams.SeparatorLeftHTML || "";
                var separatorRightHTML = me.dnnNavParams.SeparatorRightHTML || "";
                var cssClass = this.dnnNavParams.CSSNodeRoot;

                if (item.isBreadcrumb) {
                    if ((me.dnnNavParams.CSSBreadCrumbRoot || "") != "")
                        cssClass = me.dnnNavParams.CSSBreadCrumbRoot;
                    nodeLeftHTML = me.dnnNavParams.NodeLeftHTMLBreadCrumbRoot || nodeLeftHTML;
                    nodeRightHTML = me.dnnNavParams.NodeRightHTMLBreadCrumbRoot || nodeRightHTML;
                    separatorLeftHTML = me.dnnNavParams.SeparatorLeftHTMLBreadCrumb || separatorLeftHTML;
                    separatorRightHTML = me.dnnNavParams.SeparatorRightHTMLBreadCrumb || separatorRightHTML;
                }

                if (item.isSelected) {
                    if ((me.dnnNavParams.CSSNodeSelectedRoot || "") != "")
                        cssClass = me.dnnNavParams.CSSNodeSelectedRoot;
                    separatorLeftHTML = me.dnnNavParams.SeparatorLeftHTMLActive || separatorLeftHTML;
                    separatorRightHTML = me.dnnNavParams.SeparatorRightHTMLActive || separatorRightHTML;
                }

                result.addClass(cssClass);

                if (!item.first) {
                    separatorLeftHTML = (me.dnnNavParams.SeparatorHTML || "") + separatorLeftHTML;
                }
                separatorLeftHTML = separatorLeftHTML + nodeLeftHTML;
                separatorRightHTML = nodeRightHTML + separatorRightHTML;

                if (separatorLeftHTML != "") {
                    result.prepend($("<span />").append(separatorLeftHTML));
                }
                if (separatorRightHTML != "") {
                    result.append($("<span />").append(separatorRightHTML));
                }

                if (item.childMenu && me.dnnNavParams.IndicateChildren)
                    result.css({
                        "background-image": "url(" + me.dnnNavParams.PathSystemImage + me.dnnNavParams.IndicateChildImageRoot + ")",
                        "background-repeat": "no-repeat",
                        "background-position": "right"
                    });
            }
            else {
                result = $("<tr><td style='position:relative'><span /></td><td style='position:relative'><span /></td><td style='position:relative'/></tr>");
                var tdImg = result.find("td:eq(0)");
                var spanImg = result.find("span:eq(0)");
                var spanText = result.find("span:eq(1)");
                var tdArrow = result.find("td:eq(2)");

                if (href) {
                    item.coveringHere = function () { return item.rendered.find("td"); };
                }

                tdImg.addClass("icn");
                if (image) {
                    spanImg.append($("<img />").attr("src", image));
                }

                spanText.addClass("txt");
                if (!item.isSeparator) {
                    spanText.text(title);
                }
                spanText.css("white-space", "nowrap");

                if (item.childMenu && me.dnnNavParams.IndicateChildren)
                    tdArrow.append($("<img />").attr("src", me.dnnNavParams.PathSystemImage + me.dnnNavParams.IndicateChildImageSub));

                tdImg.addClass(me.dnnNavParams.CSSIcon);
                tdArrow.addClass(me.dnnNavParams.CSSIndicateChildSub);
                result.css("cursor", "pointer");

                var nodeLeftHTML = me.dnnNavParams.NodeLeftHTMLSub || "";
                var nodeRightHTML = me.dnnNavParams.NodeRightHTMLSub || "";
                var cssClass = me.dnnNavParams.CSSNode;

                if (item.isBreadcrumb) {
                    if ((me.dnnNavParams.CSSBreadCrumbSub || "") != "")
                        cssClass = me.dnnNavParams.CSSBreadCrumbSub;
                    nodeLeftHTML = me.dnnNavParams.NodeLeftHTMLBreadCrumbSub || nodeLeftHTML;
                    nodeRightHTML = me.dnnNavParams.NodeRightHTMLBreadCrumbSub || nodeRightHTML;
                }

                if (item.isSelected) {
                    if ((me.dnnNavParams.CSSNodeSelectedSub || "") != "")
                        cssClass = me.dnnNavParams.CSSNodeSelectedSub;
                }

                if (item.isSeparator) {
                    cssClass = (me.dnnNavParams.CSSBreak || "");
                }

                result.addClass(cssClass);

                if (nodeLeftHTML)
                    tdImg.prepend($("<span />").append(nodeLeftHTML));
                if (nodeRightHTML)
                    tdArrow.append($("<span />").append(nodeRightHTML));
            }

            if (item.isSelected) {
                result.addClass("sel");
            }
            if (item.isBreadcrumb) {
                result.addClass("bc");
            }
            if (item.isSeparator) {
                result.addClass("break");
            }
            result.addClass("mi");
            result.addClass("mi" + item.path);
            result.addClass("id" + item.id);
            if (item.first) {
                result.addClass("first");
            }
            if (item.last) {
                result.addClass("last");
            }
            if (item.first && item.last) {
                result.addClass("firstlast");
            }

            item.rendered = result;

            return result;
        };

        DDR.Menu.Providers.DNNMenu.prototype.menuItemHover = function (item) {
            var me = this;

            if (item.level == 0) {
                item.rendered.setHoverClass("hov " + (me.dnnNavParams.CSSNodeHoverRoot || (me.dnnNavParams.CSSNodeHover || "")));
            }
            else {
                item.rendered.setHoverClass("hov " + (me.dnnNavParams.CSSNodeHoverSub || (me.dnnNavParams.CSSNodeHover || "")));
            }
        };
    });
}
