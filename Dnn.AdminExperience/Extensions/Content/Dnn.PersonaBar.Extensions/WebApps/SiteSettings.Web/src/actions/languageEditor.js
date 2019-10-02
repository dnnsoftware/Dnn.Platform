import { languageEditor as ActionTypes } from "constants/actionTypes";
import LanguageEditorService from "services/languageEditorService";
const languageEditorActions = {
    setLanguageBeingEdited(language) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SET_LANGUAGE_BEING_EDITED,
                payload: language
            });
        };
    },
    getRootResourcesFolder(portalId, mode, callback) {
        return (dispatch) => {
            LanguageEditorService.getRootResourcesFolder(portalId, mode, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_ROOT_RESOURCES_FOLDER,
                    payload: data
                });
                if (callback) {
                    callback();
                }
            });
        };
    },
    getSubRootResources(folder, callback) {
        return (dispatch) => {
            LanguageEditorService.getSubRootResources(folder, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SUBROOT_RESOURCES_FOLDER,
                    payload: data
                });
                if (callback) {
                    callback();
                }
            });
        };
    },
    getResxEntries(parameters, callback) {
        return (dispatch) => {
            let _parameters = Object.assign({}, parameters);
            delete _parameters.resourceFileActual;
            LanguageEditorService.getResxEntries(_parameters, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_RESX_ENTRIES,
                    payload: Object.assign(parameters, data)
                });
                if (callback) {
                    callback();
                }
            });
        };
    },
    updateResxEntry(updatedList, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_RESX_ENTRIES,
                payload: updatedList
            });
            if (callback) {
                callback();
            }
        };
    },
    saveTranslations(payload, callback) {
        return () => {
            LanguageEditorService.saveTranslations(payload, (data) => {
                if (callback) {
                    callback(data);
                }
            });
        };
    }
};

export default languageEditorActions;