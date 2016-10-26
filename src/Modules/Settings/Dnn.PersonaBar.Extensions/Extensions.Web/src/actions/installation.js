import { installation as ActionTypes, extension as ExtensionActionTypes } from "constants/actionTypes";
import { InstallationService } from "services";
const installationActions = {
    parsePackage(file, callback, errorCallback) {
        return (dispatch) => {
            InstallationService.parsePackage(file, (data) => {
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
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    setInstallingAvailablePackage(FileName, PackageType, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.INSTALLING_AVAILABLE_PACKAGE,
                payload: {
                    PackageType,
                    FileName
                }
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    notInstallingAvailablePackage(callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.NOT_INSTALLING_AVAILABLE_PACKAGE
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    installExtension(file, newExtension, callback, addToList) {
        let _newExtension = JSON.parse(JSON.stringify(newExtension));
        return (dispatch) => {
            InstallationService.installPackage(file, (data) => {
                dispatch({
                    type: ActionTypes.INSTALLED_EXTENSION_LOGS,
                    payload: JSON.parse(data)
                });
                if (addToList) {
                    _newExtension.packageId = JSON.parse(data).newPackageId;
                    dispatch({
                        type: ExtensionActionTypes.INSTALLED_EXTENSION,
                        payload: {
                            PackageInfo: _newExtension,
                            logs: JSON.parse(data).logs
                        }
                    });
                }
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
