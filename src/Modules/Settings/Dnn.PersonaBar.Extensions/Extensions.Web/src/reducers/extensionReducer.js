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
    selectedAvailablePackageType: "",
    parsedInstallationPackage: null,
    installWizardStep: 0,
    installationLogs: []
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
        case ActionTypes.PARSED_INSTALLATION_PACKAGE:
            return { ...state,
                parsedInstallationPackage: action.payload
            };
        case ActionTypes.CLEAR_PARSED_INSTALLATION_PACKAGE:
            return { ...state,
                parsedInstallationPackage: null
            };
        case ActionTypes.GO_TO_WIZARD_STEP:
            return { ...state,
                installWizardStep: action.payload.wizardStep
            };
        case ActionTypes.INSTALLED_EXTENSION_LOGS:
            return { ...state,
                installationLogs: action.payload.logs
            };
        default:
            return { ...state
            };
    }
}
