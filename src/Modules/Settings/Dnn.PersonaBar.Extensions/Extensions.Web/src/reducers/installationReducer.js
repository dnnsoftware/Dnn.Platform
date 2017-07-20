import { installation as ActionTypes, extension as ExtensionActionTypes } from "../constants/actionTypes";

export default function installation(state = {
    parsedInstallationPackage: null,
    installWizardStep: 0,
    installationLogs: [],
    availablePackage: {},
    licenseAccepted: false,
    installingAvailablePackage: false,
    viewingLog: false
}, action) {
    switch (action.type) {
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
        case ExtensionActionTypes.INSTALLED_EXTENSION:
            return { ...state,
                installationLogs: action.payload.logs
            };
        case ActionTypes.NOT_INSTALLING_AVAILABLE_PACKAGE:
            return { ... state,
                availablePackage: {},
                installingAvailablePackage: false
            };
        case ActionTypes.INSTALLING_AVAILABLE_PACKAGE:
            return { ...state,
                availablePackage: action.payload,
                installingAvailablePackage: true
            };
        case ActionTypes.SET_FAILED_INSTALLATION_LOGS:
            return { ...state,
                installationLogs: ["Oops, something went wrong and the installation failed. Please try the installation again."]
            };
        case ActionTypes.TOGGLE_ACCEPT_LICENSE:
            return { ...state,
                licenseAccepted: action.payload
            };
        case ActionTypes.TOGGLE_VIEWING_LOG:
            return { ...state,
                viewingLog: action.payload
            };
        case ActionTypes.SET_IS_PORTAL_PACKAGE:
            return { ...state,
                isPortalPackage: action.payload
            };
        default:
            return state;
    }
}
