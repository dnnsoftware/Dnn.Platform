(function ($, t, sys) {
	t.registerNamespace("dnn.controls");
	dnn.controls.termsSelector = {};
	
	dnn.controls.termsSelector.OnClientDropDownOpened = function (sender, e) {
		dnn.controls.termsSelector.fire(sender);
	};
	
	dnn.controls.termsSelector.OnClientNodeChecked = function (sender, e) {
		dnn.controls.termsSelector.update(sender);
	};

	dnn.controls.termsSelector.update = function (tree) {
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
	};

	dnn.controls.termsSelector.fire = function(combobox) {
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
	};

	var updateTerms = function() {
		setTimeout(function() {
			$("div[class*=TermsSelector]").each(function() {
				var clientId = $(this).attr("id");
				var combo = $find(clientId);
				if (combo != null) {
					dnn.controls.termsSelector.fire(combo);
					var tree = $find($("div[id^=" + clientId + "][id$=_TreeView]").attr("id"));
					dnn.controls.termsSelector.update(tree);
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
}(jQuery, Type, window.Sys));

