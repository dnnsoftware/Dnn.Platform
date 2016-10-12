import { extension as ActionTypes } from "../constants/actionTypes";

function getAvailablePackageTypes(installedTypes) {
    return installedTypes.filter((type) => {
        return type.HasAvailablePackages === true;
    });
}

export default function extension(state = {
    installedPackages: [],
    availablePackages: [],
    installedPackageTypes: [],
    availablePackageTypes: [],
    selectedInstalledPackageType: "",
    selectedAvailablePackageType: ""
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
                installedPackages: [...state.installedPackages.slice(0, action.payload.index), ...state.installedPackages.slice(action.payload.index + 1)]
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
        default:
            return { ...state
            };
    }
}
