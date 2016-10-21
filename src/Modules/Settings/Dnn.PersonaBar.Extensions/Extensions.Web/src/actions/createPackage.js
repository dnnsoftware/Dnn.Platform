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
    createPackage(payload, callback) {
        return (dispatch) => {
            CreatePackageService.createPackage(payload, (data) => {
                dispatch({
                    type: ActionTypes.CREATED_PACKAGE,
                    payload: data
                });
                if (callback) {
                    callback(data);
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
    }
};

export default createPackageActions;
