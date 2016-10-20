import { extension as ActionTypes } from "constants/actionTypes";
import { ExtensionService } from "services";
import utilities from "utils";

function errorCallback(message) {
    utilities.utilities.notifyError(message);
}
function valueMapExtensionBeingEdited(extensionBeingEdited) {
    let _extensionBeingEdited = Object.assign({}, extensionBeingEdited);
    Object.keys(_extensionBeingEdited).forEach((key) => {
        _extensionBeingEdited[key] = _extensionBeingEdited[key].value;
    });
    return _extensionBeingEdited;
}
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
            }, errorCallback);
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
            }, errorCallback);
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
            }, errorCallback);
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
            }, errorCallback);
        };
    },
    updateExtension(updatedExtension, index, callback) {
        return (dispatch) => {
            ExtensionService.updateExtension(updatedExtension, () => {
                dispatch({
                    type: ActionTypes.UPDATED_EXTENSION,
                    payload: {
                        index,
                        updatedExtension: valueMapExtensionBeingEdited(updatedExtension)
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    downloadPackage(packageType, packageName, callback) {
        return (dispatch) => {
            ExtensionService.downloadPackage(packageType, packageName, (data) => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
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
            }, errorCallback);
        };
    },
    createNewModule(payload, shouldAppend, callback) {
        return (dispatch) => {
            ExtensionService.createNewModule(payload, (data) => {
                if (shouldAppend) {
                    dispatch({
                        type: ActionTypes.CREATED_NEW_MODULE,
                        payload: data
                    });
                }
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    editExtension(parameters, extensionBeingEditedIndex, callback) {
        return (dispatch) => {
            ExtensionService.getPackageSettings(parameters, (data) => {
                dispatch({
                    type: ActionTypes.EDIT_EXTENSION,
                    payload: {
                        extensionBeingEdited: data,
                        extensionBeingEditedIndex: extensionBeingEditedIndex
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    toggleTriedToSave() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.TOGGLE_TRIED_TO_SAVE
            });
        };
    },
    updateExtensionBeingEdited(extensionBeingEdited, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_EXTENSION_BEING_EDITED,
                payload: extensionBeingEdited
            });
            if (callback) {
                setTimeout(() => {  //let JS propagate
                    callback();
                }, 0);
            }
        };
    },
    toggleTabError(tabIndex, action) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.TOGGLE_TAB_ERROR,
                payload: {
                    tabIndex,
                    action
                }
            });
        };
    }
};

export default extensionActions;
