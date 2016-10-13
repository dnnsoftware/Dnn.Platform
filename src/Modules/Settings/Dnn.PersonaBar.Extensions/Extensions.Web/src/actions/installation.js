import { installation as ActionTypes } from "constants/actionTypes";
import { ExtensionService } from "services";
const installationActions = {
    parsePackage(file, callback, errorCallback) {
        return (dispatch) => {
            ExtensionService.parsePackage(file, (data) => {
                dispatch({
                    type: ActionTypes.PARSED_INSTALLATION_PACKAGE,
                    payload: JSON.parse(data)
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    navigateWizard(wizardStep, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.GO_TO_WIZARD_STEP,
                payload: {
                    wizardStep
                }
            });
        };
    },
    installExtension(file, callback) {
        return (dispatch) => {
            ExtensionService.installPackage(file, (data) => {
                dispatch({
                    type: ActionTypes.INSTALLED_EXTENSION_LOGS,
                    payload: JSON.parse(data)
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    clearParsedInstallationPackage(callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CLEAR_PARSED_INSTALLATION_PACKAGE
            });
            if (callback) {
                callback();
            }
        };
    }
};

export default installationActions;
