import { extension as ActionTypes } from "../constants/actionTypes";

function getAvailablePackageTypes(installedTypes) {
    return installedTypes.filter((type) => {
        return type.HasAvailablePackages === true;
    });
}

function addToModuleList(value, list) {
    return list.concat(value).sort(function (a, b) {
        if (a.friendlyName.toLowerCase() < b.friendlyName.toLowerCase()) return -1;
        if (a.friendlyName.toLowerCase() > b.friendlyName.toLowerCase()) return 1;
        return 0;
    });
}

function getValidateRequired(key) {
    switch (key) {
        case "friendlyName":
            return true;
        default:
            return false;
    }
}

function getTabMapping(key) {
    switch (key) {
        case "license":
            return 3;
        case "releaseNotes":
            return 4;
        default:
            return 0;
    }
}

function validationMapExtensionBeingEdited(extensionBeingEdited) {
    let _extensionBeingEdited = Object.assign({}, extensionBeingEdited);
    Object.keys(_extensionBeingEdited).forEach((key) => {
        let validateRequired = getValidateRequired(key);
        let tabMapping = getTabMapping(key);
        _extensionBeingEdited[key] = {
            value: !_extensionBeingEdited[key].hasOwnProperty("value") ? _extensionBeingEdited[key] : _extensionBeingEdited[key].value,
            validateRequired,
            tabMapping,
            error: false
        };
    });
    return _extensionBeingEdited;
}

function removeRecordFromArray(arr, index) {
    return [...arr.slice(0, index), ...arr.slice(index + 1)];
}
export default function extension(state = {
    installedPackages: [],
    availablePackages: [],
    installedPackageTypes: [],
    availablePackageTypes: [],
    packageBeingEditedSettings: {},
    extensionBeingEdited: {},
    extensionBeingEditedIndex: -1,
    selectedInstalledPackageType: "",
    selectedAvailablePackageType: "",
    triedToSave: false,
    tabsWithError: [],
    moduleCategories: [],
    desktopModulePermissions: {}
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_INSTALLED_PACKAGES:
            return { ...state,
                installedPackages: action.payload.Results,
                selectedInstalledPackageType: action.payload.selectedInstalledPackageType
            };
        case ActionTypes.RETRIEVED_AVAILABLE_PACKAGES:
            return { ...state,
                availablePackages: action.payload.Results,
                selectedAvailablePackageType: action.payload.selectedAvailablePackageType
            };
        case ActionTypes.UPDATED_EXTENSION:
            return { ...state,
                installedPackages: [...state.installedPackages.slice(0, action.payload.index), action.payload.updatedExtension, ...state.installedPackages.slice(action.payload.index + 1)]
            };
        case ActionTypes.DELETED_EXTENSION:
            return { ...state,
                installedPackages: removeRecordFromArray(state.installedPackages, action.payload.index)
            };
        case ActionTypes.EDIT_EXTENSION:
            return { ...state,
                extensionBeingEdited: validationMapExtensionBeingEdited(action.payload.extensionBeingEdited),
                extensionBeingEditedIndex: action.payload.extensionBeingEditedIndex
            };
        case ActionTypes.TOGGLE_TRIED_TO_SAVE:
            return { ...state,
                triedToSave: !state.triedToSave
            };
        case ActionTypes.TOGGLE_TAB_ERROR:
            return { ...state,
                tabsWithError: action.payload.action === "remove" ?
                    removeRecordFromArray(state.tabsWithError, state.tabsWithError.indexOf(action.payload.tabIndex)) :
                    (state.tabsWithError.indexOf(action.payload.tabIndex) < 0 ? state.tabsWithError.concat(action.payload.tabIndex) : state.tabsWithError)
            };
        case ActionTypes.UPDATED_EXTENSION_BEING_EDITED:
            return { ...state,
                extensionBeingEdited: action.payload
            };
        case ActionTypes.RETRIEVED_INSTALLED_PACKAGE_TYPES:
            return { ...state,
                installedPackageTypes: action.payload.Results,
                availablePackageTypes: getAvailablePackageTypes(action.payload.Results)
            };
        case ActionTypes.RETRIEVED_AVAILABLE_PACKAGE_TYPES:
            return { ...state,
                availablePackageTypes: action.payload.Results
            };
        case ActionTypes.CREATED_NEW_MODULE:
            return { ...state,
                installedPackages: addToModuleList(action.payload.PackageInfo, state.installedPackages)
            };
        case ActionTypes.RETRIEVED_MODULE_CATEGORIES:
            return { ...state,
                moduleCategories: action.payload
            };
        case ActionTypes.RETRIEVED_DESKTOPMODULE_PERMISSIONS:
            return { ...state,
                desktopModulePermissions: action.payload
            };
        case ActionTypes.UPDATED_DESKTOPMODULE_PERMISSIONS:
            return { ...state
            };
        default:
            return { ...state
            };
    }
}
