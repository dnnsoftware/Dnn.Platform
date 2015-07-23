(function ($) {
    if (typeof dnn === "undefined" || dnn === null) { dnn = {}; };
    if (dnn.permissionGridManager) return;

    dnn.permissionGridManager = function (scopeId) {
        var inputBox = $('#' + scopeId + '_txtUser');
        var userIdsField = $('#' + scopeId + '_hiddenUserIds');
        var init = function () {
            var serviceUrl = $.ServicesFramework(-1).getServiceRoot('InternalServices') + 'ItemListService/SearchUser';
            inputBox.tokenInput(serviceUrl, {
				theme: "facebook",
				resultsFormatter: function (item) {
				    return "<li class='user'><img src='" + item.iconfile + "' title='" + item.name + "' style='width:25px;height:25px;' /><span>" + item.name + "</span></li>";
				},
				minChars: 2,
				preventDuplicates: true,
				hintText: '',
				onAdd: function (item) {
					if (userIdsField.val() == '') {
					    userIdsField.val(item.id);
					} else {
					    var array = userIdsField.val().split(','),
						index = $.inArray(item.id, array);
					    if (index == -1) {
					        userIdsField.val(userIdsField.val() + ',' + item.id);
					    }
					}
				},
				onDelete: function (item) {
					var array = userIdsField.val().split(','),
						id = item.id,
						index = $.inArray(id, array);
					if (index !== -1) {
					    array.splice(index, 1);
					    userIdsField.val(array.join(','));
					}
				},
				onError: function (xhr, status) {
					//displayMessage(composeMessageDialog, opts.autoSuggestErrorText + status);
				}
            });

	        var inputBoxId = inputBox.attr('id');
	        $('label[for="' + inputBoxId + '"]').attr('for', 'token-input-' + inputBoxId);

            var roleId = $('#' + scopeId + '_roleField');
            var roleSelector = $('#' + scopeId + '_cboSelectRole');
            roleSelector.change(function () {
                updateHiddenField();
            });
            updateHiddenField();

            function updateHiddenField() {
                var Id = roleSelector.val();
                roleId.val(Id);
            }
        }

        $(document).ready(function() {
            init();
        });
        return this;
    };
})(jQuery);