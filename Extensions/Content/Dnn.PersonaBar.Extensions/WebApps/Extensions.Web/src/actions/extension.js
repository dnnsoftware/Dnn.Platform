import { extension as ActionTypes, installation as InstallationActionTypes } from "constants/actionTypes";
import { ExtensionService } from "services";
import { validationMapExtensionBeingEdited } from "utils/helperFunctions";
import Localization from "localization";
import utilities from "utils";

function errorCallback(error) {
    let message = utilities.utilities.getResx("SharedResources", "GenericErrorMessage.Error");
    if (error.Error) {
        message = error.Error;
    } else if (error.Message) {
        message = error.Message;
    } else if (typeof error === "string") {
        message = error;
    }
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
    updateExtension(updatedExtension, editorActions, index, callback) {
        return (dispatch) => {
            ExtensionService.updateExtension(updatedExtension, editorActions, (data) => {
                dispatch({
                    type: ActionTypes.UPDATED_EXTENSION_BEING_EDITED,
                    payload: {
                        extensionBeingEdited: validationMapExtensionBeingEdited(data.PackageDetail)
                    }
                });
                dispatch({
                    type: ActionTypes.UPDATED_EXTENSION,
                    payload: {
                        index,
                        updatedExtension: valueMapExtensionBeingEdited(updatedExtension)
                    }
                });
                utilities.utilities.notify(Localization.get("EditExtension_Notify.Success"));
                if (callback) {
                    utilities.utilities.throttleExecution(callback);
                }
            }, errorCallback);
        };
    },
    downloadPackage(packageType, packageName, callback) {
        return () => {
            ExtensionService.downloadPackage(packageType, packageName, (data) => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    installAvailablePackage(packageType, packageName, newExtension, callback) {
        return (dispatch) => {
            ExtensionService.installAvailablePackage(packageType, packageName, (data) => {
                if (data.success) {
                    let _newExtension = JSON.parse(JSON.stringify(newExtension));
                    _newExtension.packageId = data.newPackageId;
                    dispatch({
                        type: ActionTypes.INSTALLED_EXTENSION,
                        payload: {
                            PackageInfo: _newExtension,
                            logs: data.logs
                        }
                    });
                }
                if (!data.success) {
                    dispatch({
                        type: InstallationActionTypes.SET_FAILED_INSTALLATION_LOGS
                    });
                }
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    parseAvailablePackage(packageType, fileName, callback) {
        return (dispatch) => {
            ExtensionService.parseAvailablePackage(packageType, fileName, (data) => {
                dispatch({
                    type: InstallationActionTypes.PARSED_INSTALLATION_PACKAGE,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    deletePackage(payload, index, callback) {
        return (dispatch) => {
            ExtensionService.deletePackage(payload, (data) => {
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
    setPackageBeingDeleted(extensionBeingDeleted, extensionBeingDeletedIndex, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SET_EXTENSION_BEING_DELETED,
                payload: {
                    extensionBeingDeleted,
                    extensionBeingDeletedIndex
                }
            });
            if (callback) {
                callback();
            }
        };
    },
    toggleDeleteFiles(callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.TOGGLE_DELETE_EXTENSION_FILES
            });
            if (callback) {
                callback();
            }
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
    addExtension(parameters, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.EDIT_EXTENSION,
                payload: {
                    extensionBeingEdited: parameters,
                    extensionBeingEditedIndex: -1
                }
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    createNewExtension(extensionBeingAdded, editorActions, index, callback) {
        return (dispatch) => {
            ExtensionService.createNewExtension(extensionBeingAdded, editorActions, (data) => {
                let _extensionBeingAdded = JSON.parse(JSON.stringify(extensionBeingAdded));
                _extensionBeingAdded.packageId = {};
                _extensionBeingAdded.packageId.value = data.PackageId;
                dispatch({
                    type: ActionTypes.ADDED_NEW_EXTENSION,
                    payload: {
                        PackageInfo: valueMapExtensionBeingEdited(_extensionBeingAdded)
                    }
                });
                if (callback) {
                    callback();
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
                payload: {
                    extensionBeingEdited: extensionBeingEdited
                }
            });
            if (callback) {
                setTimeout(() => {  //let JS propagate
                    callback();
                }, 0);
            }
        };
    },
    toggleTabError(tabIndex, action, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.TOGGLE_TAB_ERROR,
                payload: {
                    tabIndex,
                    action
                }
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    getModuleCategories(callback) {
        return (dispatch) => {
            ExtensionService.getModuleCategories((data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_MODULE_CATEGORIES,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    selectEditingTab(index, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECT_EDITING_TAB,
                payload: index
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    getLocaleList(callback) {
        return (dispatch) => {
            ExtensionService.getLocaleList((data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_LOCALE_LIST,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getLocalePackageList(callback) {
        return (dispatch) => {
            ExtensionService.getLocalePackagesList((data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PACKAGE_LOCALE_LIST,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    deployAvailablePackage(_package, updatedPackageIndex, callback) {
        return () => {
            ExtensionService.deployAvailablePackage(_package.description, (data) => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    GetPackageUsageFilter(callback) {
        return (dispatch) => {
            ExtensionService.getPackageUsageFilter(payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PACKAGE_USAGE_FILTER,
                    payload: payload
                });
                if (callback) {
                    callback();
                }
            }, errorCallback); 
        };
    },
    getPackageUsage(portalId, packageId, callback) {
        return (dispatch) => {
            ExtensionService.getPackageUsage(portalId, packageId, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PACKAGE_USAGE,
                    payload: data
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    }
};

export default extensionActions;
