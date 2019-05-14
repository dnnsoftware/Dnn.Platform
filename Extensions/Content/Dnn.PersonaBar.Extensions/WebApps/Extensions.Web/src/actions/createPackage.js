import { createPackage as ActionTypes } from "constants/actionTypes";
import { CreatePackageService } from "services";

const createPackageActions = {
    getPackageManifest(packageId, callback) {
        return (dispatch) => {
            CreatePackageService.getPackageManifest(packageId, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PACKAGE_MANIFEST,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updatePackagePayload(packagePayload, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_PACKAGE_PAYLOAD,
                payload: packagePayload
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    updatePackageManifest(packageManifest, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_PACKAGE_MANIFEST,
                payload: packageManifest
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    createManifest(payload, callback) {
        return (dispatch) => {
            CreatePackageService.createManifest(payload, (data) => {
                dispatch({
                    type: ActionTypes.CREATED_PACKAGE_MANIFEST,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    generateManifestPreview(payload, callback) {

        return (dispatch) => {
            CreatePackageService.generateManifestPreview(payload, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_GENERATED_MANIFEST,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    createPackage(payload, callback, errorCallback) {
        return (dispatch) => {
            CreatePackageService.createPackage(payload, (data) => {
                dispatch({
                    type: ActionTypes.CREATED_PACKAGE,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, (data) => {
                if (errorCallback) {
                    errorCallback(data);
                }
            });
        };
    },
    goToWizardStep(step, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.GO_TO_STEP,
                payload: step
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    refreshPackageFiles(payload, callback, errorCallback) {
        return (dispatch) => {
            CreatePackageService.refreshPackageFiles(payload, (data) => {
                dispatch({
                    type: ActionTypes.REFRESH_PACKAGE_FILES,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, (data) => {
                if (errorCallback) {
                    errorCallback(data);
                }
            });
        };
    }
};

export default createPackageActions;
