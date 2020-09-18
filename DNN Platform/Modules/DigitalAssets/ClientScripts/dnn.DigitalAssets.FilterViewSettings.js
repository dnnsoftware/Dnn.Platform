; if (typeof window.dnn === "undefined" || window.dnn === null) { window.dnn = {}; };

dnn.DigitalAssetsFilterViewSettingsController = function (serviceSettings, serviceControls) {
    var controls = serviceControls,

        initFilterOptionsRadioInput = function () {
            $("input[id^=" + controls.FilterOptionGroupID + "]:radio").change(function () {
                checkFilterOption($(this).val());
            });
            checkFilterOption($("input[id^=" + controls.FilterOptionGroupID + "][checked=checked]:radio").val());
        },
        
        checkFilterOption = function (checkValue) {
            if (checkValue == "FilterByFolder") {
                $("#FilterByFolderOptions").show();
            } else {
                $("#FilterByFolderOptions").hide();
            }
        },

        ValidateFolderIsSelected = function (sender, args) {
            if ($("input[id^=" + controls.FilterOptionGroupID + "][checked=checked]:radio").val() == 'FilterByFolder') {
                if (dnn[controls.FolderDropDownList[0].id].selectedItem() == null) {
                    args.IsValid = false;
                    return;
                }
            }
            
            args.IsValid = true;        
        };
    
    return {
        initFilterOptionsRadioInput: initFilterOptionsRadioInput,
        ValidateFolderIsSelected: ValidateFolderIsSelected,
    };
};
