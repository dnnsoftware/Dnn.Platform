import { installation as ActionTypes } from "../constants/actionTypes";

export default function installation(state = {
    parsedInstallationPackage: null,
    installWizardStep: 0,
    installationLogs: []
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
        default:
            return { ...state
            };
    }
}
