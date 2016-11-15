import ActionTypes from "../constants/actionTypes/templateActionTypes";
import TemplateService from "../services/templateService";

const templateActions = {
    
    loadSavePageAsTemplate() {
        return {
            type: ActionTypes.LOAD_SAVE_AS_TEMPLATE,
            data: {}
        };
    },

    cancelSavePageAsTemplate() {
        return {
            type: ActionTypes.CANCEL_SAVE_AS_TEMPLATE,
            data: {}
        };
    },

    savePageAsTemplate() {
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.SAVING_TEMPLATE
            });    

            const state = getState();
            const template = state.template.template;
            TemplateService.savePageAsTemplate(template).then(() => {
                dispatch({
                    type: ActionTypes.SAVED_TEMPLATE,
                    data: { }
                });  
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_SAVING_TEMPLATE,
                    data: {error}
                });
            });     
        };
    },

    changeTemplateField(key, value) {
        return {
            type: ActionTypes.CHANGE_TEMPLATE_FIELD_VALUE,
            field: key,
            value
        };
    }
};
export default templateActions;