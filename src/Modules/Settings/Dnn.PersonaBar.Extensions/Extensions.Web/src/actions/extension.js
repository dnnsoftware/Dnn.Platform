import { extension as ActionTypes } from "../constants/actionTypes";
import ExtensionService from "../services/extensionService";
const extensionActions = {
    getInstalledPackages(type, callback) {
        return (dispatch) => {
            ExtensionService.getInstalledPackages(type, payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_INSTALLED_PACKAGES,
                    payload: {
                        Results: payload.Results,
                        selectedInstalledPackageType: type
                    }
                });
                if (callback) {
                    callback();
                }
            });
        };
    },
    getAvailablePackages(type, callback) {
        return (dispatch) => {
            ExtensionService.getAvailablePackages(type, payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_AVAILABLE_PACKAGES,
                    payload: {
                        Results: payload.Results && payload.Results[0] && payload.Results[0].ValidPackages,
                        selectedAvailablePackageType: type
                    }
                });
                if (callback) {
                    callback();
                }
            });
        };
    },
    getPackageTypes(callback) {
        return (dispatch) => {
            ExtensionService.getPackageTypes(payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_INSTALLED_PACKAGE_TYPES,
                    payload: payload
                });
                if (callback) {
                    callback();
                }
            });
        };
    },
    getAvailablePackageTypes(callback) {
        return (dispatch) => {
            ExtensionService.getAvailablePackageTypes(payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_AVAILABLE_PACKAGE_TYPES,
                    payload: payload
                });
                if (callback) {
                    callback();
                }
            });
        };
    },
    updateExtension(updatedExtension, index, callback) {
        return (dispatch) => {
            ExtensionService.updateExtension(updatedExtension, () => {
                dispatch({
                    type: ActionTypes.UPDATED_EXTENSION,
                    payload: {
                        index,
                        updatedExtension
                    }
                });
                if (callback) {
                    callback();
                }
            });
        };
    },
    downloadPackage(packageType, packageName, callback) {
        return (dispatch) => {
            ExtensionService.downloadPackage(packageType, packageName, (data) => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    deletePackage(packageId, index, callback) {
        return (dispatch) => {
            ExtensionService.deletePackage(packageId, (data) => {
                dispatch({
                    type: ActionTypes.DELETED_EXTENSION,
                    payload: {
                        index
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    parsePackage(file, callback) {
        return (dispatch) => {
            ExtensionService.parsePackage(file, (data) => {
                dispatch({
                    type: ActionTypes.PARSED_INSTALLATION_PACKAGE,
                    payload: JSON.parse(data)
                });
                if (callback) {
                    callback(data);
                }
            });
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

export default extensionActions;
