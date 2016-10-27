import { moduleDefinition as ActionTypes } from "constants/actionTypes";
import { ModuleDefinitionService } from "services";
const moduleDefinitionActions = {
    setFormDirt(value, callback) {
        return dispatch => {
            dispatch({
                type: ActionTypes.SET_FORM_DIRT,
                payload: value
            });
            if (typeof callback === "function") {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    setControlFormDirt(value, callback) {
        return dispatch => {
            dispatch({
                type: ActionTypes.SET_CONTROL_FORM_DIRT,
                payload: value
            });
            if (typeof callback === "function") {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    getSourceFolders(callback) {
        return dispatch => {
            ModuleDefinitionService.getSourceFolders((data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SOURCE_FOLDERS,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getSourceFiles(_root, callback) {
        return dispatch => {
            ModuleDefinitionService.getSourceFiles(_root, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SOURCE_FILES,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getControlIcons(controlPath, callback) {
        return dispatch => {
            ModuleDefinitionService.getControlIcons(controlPath, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_CONTROL_ICONS,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    }
};

export default moduleDefinitionActions;
