if (!DDR.Menu.Providers.SolPart) {
    DDRjQuery(function($) {
        DDR.Menu.Providers.SolPart = function(jqContainer, dnnNavParams) {
            var me = this;

            me.baseConstructor(jqContainer, dnnNavParams);

            me.spacer = (navigator.userAgent.toLowerCase().indexOf('msie') == -1) ? "<img src='" + dnnNavParams.PathSystemImage + "spacer.gif' />" : "&nbsp;";

            dnnNavParams.CSSControl = dnnNavParams.CSSControl || "MainMenu_MenuBar";
            dnnNavParams.CSSContainerRoot = dnnNavParams.CSSContainerRoot || "MainMenu_MenuContainer";
            dnnNavParams.CSSContainerSub = dnnNavParams.CSSContainerSub || "MainMenu_SubMenu";
            dnnNavParams.CSSBreak = dnnNavParams.CSSBreak || "MainMenu_MenuBreak";
            dnnNavParams.CSSIndicateChildSub = dnnNavParams.CSSIndicateChildSub || "MainMenu_MenuArrow";
            dnnNavParams.CSSIndicateChildRoot = dnnNavParams.CSSIndicateChildRoot || "MainMenu_RootMenuArrow";
            dnnNavParams.CSSNode = dnnNavParams.CSSNode || "MainMenu_MenuItem";
            dnnNavParams.CSSNodeHover = dnnNavParams.CSSNodeHover || "MainMenu_MenuItemSel";
            dnnNavParams.CSSIcon = dnnNavParams.CSSIcon || "MainMenu_MenuIcon";
            dnnNavParams.ControlAlignment = dnnNavParams.ControlAlignment.toLowerCase();
        }
        DDR.Menu.Providers.SolPart.prototype = new DDR.Menu.Providers.BaseRenderer();

        DDR.Menu.Providers.SolPart.prototype.createRootMenu = function() {
            var me = this;

            var dnnNavContainer = $("<span />");
            me.jqContainer.append(dnnNavContainer);

            me.menus.each(function(m) {
                dnnNavContainer.append(me.createRenderedMenu(m));
            });

            me.items.each(function(i) {
                i.rendered.html(i.rendered.html()
                    .replace(/#NodeLeftHTMLRoot#/g, me.dnnNavParams.NodeLeftHTMLRoot || "")
                    .replace(/#NodeRightHTMLRoot#/g, me.dnnNavParams.NodeRightHTMLRoot || "")
                    .replace(/#NodeLeftHTMLSub#/g, me.dnnNavParams.NodeLeftHTMLSub || "")
                    .replace(/#NodeRightHTMLSub#/g, me.dnnNavParams.NodeRightHTMLSub || "")
                    .replace(/#NodeLeftHTMLBreadCrumbRoot#/g, me.dnnNavParams.NodeLeftHTMLBreadCrumbRoot || "")
                    .replace(/#NodeRightHTMLBreadCrumbRoot#/g, me.dnnNavParams.NodeRightHTMLBreadCrumbRoot || "")
                    .replace(/#NodeLeftHTMLBreadCrumbSub#/g, me.dnnNavParams.NodeLeftHTMLBreadCrumbSub || "")
                    .replace(/#NodeRightHTMLBreadCrumbSub#/g, me.dnnNavParams.NodeRightHTMLBreadCrumbSub || "")
                    .replace(/#NodeRightHTMLRoot#/g, me.dnnNavParams.SeparatorRightHTML || "")
                );
            });

            dnnNavContainer.find("td:contains('#Separator')").each(function() {
                var jqThis = $(this);
                jqThis.html(jqThis.html()
                    .replace(/#SeparatorHTML#/g, me.dnnNavParams.SeparatorHTML || "")
                    .replace(/#SeparatorLeftHTML#/g, me.dnnNavParams.SeparatorLeftHTML || "")
                    .replace(/#SeparatorRightHTML#/g, me.dnnNavParams.SeparatorRightHTML || "")
                );
            });

            dnnNavContainer.find("td").attr("unselectable", "on");

            me.jqContainer.replaceWith(dnnNavContainer);

            me.jqContainer.show(1);
            me.jqContainer.queue(function() {
                me.addCovering();
                me.prepareHideAndShow();

                $(this).dequeue();
            });
        }

        DDR.Menu.Providers.SolPart.prototype.createRenderedMenu = function(menu) {
            var me = this;

            var level = menu.level;
            var childItems = menu.childItems;

            if (level == 0) {
                menu.flyout = false;
                menu.layout = me.orientHorizontal ? "horizontal" : "vertical";
                var result = $("<table style='vertical-align: middle' border='0' cellspacing='0' cellpadding='0' />").addClass(me.dnnNavParams.CSSContainerRoot);

                if (me.orientHorizontal) {
                    result.attr("width", "100%");
                    result.append(me.menuBar = $("<tr />"));
                }
                else {
                    result.attr("height", "100%");
                    me.menuBar = result;
                }

                if (me.orientHorizontal && me.dnnNavParams.ControlAlignment == 'right') {
                    me.menuBar.append("<td width='100%'>" + me.spacer + "</td>");
                }

                me.separator = $("<td><table border='0' cellspacing='0' cellpadding='0'><tr /></table></td>").addClass(me.dnnNavParams.CSSBreak);
                me.firstItem = true;

                childItems.each(function(i) {
                    me.createRenderedItem(i);
                });

                if (me.separator.find("tr").children("td").length > 0) {
                    me.menuBar.append(me.orientHorizontal ? me.separator.clone() : $("<tr />").append(me.separator.clone()));
                }

                if (me.orientHorizontal) {
                    if (me.dnnNavParams.ControlAlignment == 'left') {
                        me.menuBar.append("<td width='100%'>" + me.spacer + "</td>");
                    }
                }
                else {
                    me.menuBar.append("<tr><td height='100%'>" + me.spacer + "</td></tr>");
                }
            }
            else {
                menu.flyout = true;
                menu.layout = "vertical";
                var parentItem = menu.parentItem;
                var parentMenu = parentItem.parentMenu;

                var table = $("<table cellspacing='0' cellpadding='0' border='0' />");
                var result = $("<div />").css({ "position": "absolute", "left": "-1000px" }).append(table).addClass(me.dnnNavParams.CSSContainerSub);

                childItems.each(function(i) {
                    table.append(me.createRenderedItem(i));
                });
            }

            menu.rendered = result;

            return result;
        };

        DDR.Menu.Providers.SolPart.prototype.createRenderedItem = function(item) {
            var me = this;

            var level = item.level;
            var title = item.title;
            var image = item.image;
            var href = item.href;

            var result;

            if (level == 0) {
                if (me.dnnNavParams.SeparatorHTML && !me.firstItem)
                    me.separator.find("tr").append($("<td />").addClass(me.dnnNavParams.CSSSeparator).text("#SeparatorHTML#"));

                if (me.dnnNavParams.SeparatorLeftHTML)
                    me.separator.find("tr").append($("<td />").addClass(me.dnnNavParams.CSSLeftSeparator).text("#SeparatorLeftHTML#"));

                if (me.separator.find("tr").children("td").length > 0) {
                    me.menuBar.append(me.orientHorizontal ? me.separator.clone() : $("<tr />").append(me.separator.clone()));
                    me.separator.find("tr").empty();
                }

                result = $("<td><table border='0' cellspacing='0' cellpadding='0' width='100%'><tr><td nowrap='nowrap' /></tr></table></td>");
                var tr = result.find("tr");
                var td = tr.children("td");

                if (!me.orientHorizontal) {
                    td.attr("align", me.dnnNavParams.ControlAlignment);
                }

                tr.addClass(me.dnnNavParams.CSSControl).addClass(me.dnnNavParams.CSSNode);
                tr.addClass(me.dnnNavParams.CSSNodeRoot);
                if (item.isBreadcrumb) {
                    tr.addClass(me.dnnNavParams.CSSBreadCrumbRoot);
                }
                if (item.isSelected) {
                    tr.addClass(me.dnnNavParams.CSSNodeSelectedRoot);
                }

                if (href) {
                    item.coveringHere = function() { return item.rendered.find("table").closest("td"); };
                }

                td.text(title);

                var leftHtml = (item.isBreadcrumb && me.dnnNavParams.NodeLeftHTMLBreadCrumbRoot)
                    ? (me.dnnNavParams.NodeLeftHTMLBreadCrumbRoot ? "#NodeLeftHTMLBreadCrumbRoot#" : "&nbsp;")
                    : (me.dnnNavParams.NodeLeftHTMLRoot ? "#NodeLeftHTMLRoot#" : "&nbsp;")
                td.prepend(leftHtml);
                var rightHtml = (item.isBreadcrumb && me.dnnNavParams.NodeRightHTMLBreadCrumbRoot)
                    ? (me.dnnNavParams.NodeRightHTMLBreadCrumbRoot ? "#NodeRightHTMLBreadCrumbRoot#" : "")
                    : (me.dnnNavParams.NodeRightHTMLRoot ? "#NodeRightHTMLRoot#" : "")
                td.append(rightHtml);

                td.prepend(image ? $("<img />").attr("src", image) : me.spacer);

                if (item.childMenu && me.dnnNavParams.IndicateChildImageRoot) {
                    var tdArrow = $("<td align='right' />");
                    tdArrow.append($("<img />").attr("src", me.dnnNavParams.PathSystemImage + me.dnnNavParams.IndicateChildImageRoot)).addClass(me.dnnNavParams.CSSIndicateChildRoot);
                    tr.append(tdArrow);
                }
                else {
                    td.append("&nbsp;");
                }

                if (!me.orientHorizontal)
                    result = $("<tr />").append(result);

                me.menuBar.append(result);

                if (me.dnnNavParams.SeparatorRightHTML)
                    me.separator.find("tr").append($("<td />").addClass(me.dnnNavParams.CSSRightSeparator).text("#SeparatorRightHTML#"));

                me.firstItem = false;
            }
            else {
                result = $("<tr><td /><td /><td width='15px' /></tr>");

                var tdImg = result.find("td:eq(0)");
                var tdText = result.find("td:eq(1)");
                var tdArrow = result.find("td:eq(2)");

                if (href) {
                    item.coveringHere = function() { return item.rendered.find("td"); };
                }

                if (image)
                    tdImg.append($("<img />").attr("src", image));
                else
                    tdImg.append(me.spacer);

                tdText.text(title);
                tdText.attr("nowrap", "nowrap");

                var leftHtml = (item.isBreadcrumb && me.dnnNavParams.NodeLeftHTMLBreadCrumbSub)
                    ? (me.dnnNavParams.NodeLeftHTMLBreadCrumbSub ? "#NodeLeftHTMLBreadCrumbSub#" : "")
                    : (me.dnnNavParams.NodeLeftHTMLSub ? "#NodeLeftHTMLSub#" : "")
                tdText.prepend(leftHtml);
                var rightHtml = (item.isBreadcrumb && me.dnnNavParams.NodeRightHTMLBreadCrumbSub)
                    ? (me.dnnNavParams.NodeRightHTMLBreadCrumbSub ? "#NodeRightHTMLBreadCrumbSub#" : "")
                    : (me.dnnNavParams.NodeRightHTMLSub ? "#NodeRightHTMLSub#" : "")
                tdText.append(rightHtml);

                tdImg.addClass(me.dnnNavParams.CSSIcon);
                tdText.addClass(me.dnnNavParams.CSSNode);
                tdArrow.addClass(me.dnnNavParams.CSSIndicateChildSub).append(
                    item.childMenu ? $("<img />").attr("src", me.dnnNavParams.PathSystemImage + me.dnnNavParams.IndicateChildImageSub) : me.spacer);
                result.addClass(me.dnnNavParams.CSSNode);

                if (item.isBreadcrumb) {
                    tdText.addClass(me.dnnNavParams.CSSBreadCrumbSub);
                    result.addClass(me.dnnNavParams.CSSBreadCrumbSub);
                }
                if (item.isSelected) {
                    tdText.addClass(me.dnnNavParams.CSSNodeSelectedSub);
                    result.addClass(me.dnnNavParams.CSSNodeSelectedSub);
                }
            }

            item.rendered = result;

            return result;
        };

        DDR.Menu.Providers.SolPart.prototype.menuItemHover = function(item) {
            var me = this;

            if (item.level == 0) {
                item.rendered.setHoverClass((me.dnnNavParams.CSSNodeHover || "") + " " + (me.dnnNavParams.CSSNodeHoverRoot || ""), item.rendered.find("tr"), me.dnnNavParams.CSSNode);
            }
            else {
                item.rendered.setHoverClass((me.dnnNavParams.CSSNodeHover || "") + " " + (me.dnnNavParams.CSSNodeHoverSub || ""), item.rendered.find("td"), me.dnnNavParams.CSSNode);
            }
        };
    });
}
