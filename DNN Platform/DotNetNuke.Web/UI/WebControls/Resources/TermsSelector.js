(function ($, sys) {
	if (typeof webcontrols === "undefined" || webcontrols === null) { webcontrols = {}; };
	webcontrols.termsSelector = {
		OnClientDropDownOpened: function (sender, e) {
			webcontrols.termsSelector.fire(sender);
		},
		OnClientNodeChecked: function (sender, e) {
			webcontrols.termsSelector.update(sender);
		},
		itemDataLoaded: function(result, context) {
			var itemsData = eval("(" + result + ")");
			var clientId = itemsData[0].clientId;
			var selectedTerms = $("#" + clientId).attr("selectedterms").split(',');
			var selectedObj = {};
			for (var i = 0; i < selectedTerms.length; i++) {
				selectedObj["t_" + selectedTerms[i]] = true;
			}
			var tree = $find($("div[id^=" + clientId + "][id$=_TreeView]").attr("id"));
			tree.trackChanges();
			for (var i = 1; i < itemsData.length; i++) {
				var data = itemsData[i];
				var node = new Telerik.Web.UI.RadTreeNode();
				node.set_text(data.name);
				node.set_value(data.termId);
				if (itemsData[i].termId < 0) {
					node.set_checkable(false);
				} else {
					node.set_checked(selectedObj["t_" + data.termId]);
				}

				var parentNode = tree.findNodeByValue(data.parentTermId);
				if (parentNode && data.termId > 0) {
					parentNode.get_nodes().add(node);
					if (!parentNode.get_expanded()) {
						parentNode.set_expanded(true);
					}
				} else {
					tree.get_nodes().add(node);
				}
			}
			tree.commitChanges();
			webcontrols.termsSelector.update(tree);
			$("div[id^=" + clientId + "][id$=_TreeView] input[type=checkbox]").dnnCheckbox();
		},
		itemDataLoadError: function(result, context) {

		},
		update: function (tree) {
			var treeDiv = $("#" + tree.get_id());
			var comboBox = treeDiv.data("combo");
			var nodes = tree.get_checkedNodes();
			var text = '', value = '';
			for (var i = 0; i < nodes.length; i++) {
				text += nodes[i].get_text() + ", ";
				value += nodes[i].get_value() + ", ";
			}
			if (text != '' && text.substr(text.length - 2, 2) == ', ') {
				text = text.substr(0, text.length - 2);
			}
			if (value != '' && value.substr(value.length - 2, 2) == ', ') {
				value = value.substr(0, value.length - 2);
			}

			comboBox.trackChanges();
			var valueItem;
			if (comboBox.get_items().get_count() == 1) {
				valueItem = new Telerik.Web.UI.RadComboBoxItem();
				comboBox.get_items().add(valueItem);
				valueItem.set_visible(false);
			} else {
				valueItem = comboBox.get_items().getItem(1);
			}
			valueItem.set_text(text);
			valueItem.set_value(value);
			valueItem.select();
			comboBox.commitChanges();
		},
		fire: function(combobox) {
			var $this = $("#" + combobox.get_id());
			var treeDiv = $("div[id^=" + combobox.get_id() + "][id$=_TreeView]");
			if (treeDiv.data("combo")) {
				return;
			}
			treeDiv.data("combo", combobox);

			treeDiv.click(function (e) {
				if (!$(e.srcElement).is(":checkbox")) {
					return false;
				}
			});
		}
	};

	var updateTerms = function() {
		setTimeout(function() {
			$("div[class*=TermsSelector]").each(function() {
				var clientId = $(this).attr("id");
				var includeSystemVocabularies = $(this).attr("includesystemvocabularies");
				var includeTags = $(this).attr("includetags");
				var portalId = $(this).attr("portalid");
				
				var combo = $find(clientId);
				if (combo != null) {
					webcontrols.termsSelector.fire(combo);
					combo.set_text("Loading...");
					eval(dnn.getVar('TermsSelectorCallback').replace('[PARAMS]', clientId + '-' + portalId + '-' + includeTags + '-' + includeSystemVocabularies));
				}
			});
		}, 0);
	};

	$().ready(function () {
		updateTerms();
		if (typeof sys != "undefined") {
			sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
				updateTerms();
			});
		}
	});
}(jQuery, window.Sys));

