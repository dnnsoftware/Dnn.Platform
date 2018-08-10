import ActionTypes from "../constants/actionTypes/templateActionTypes";
import TemplateService from "../services/templateService";
import utils from "../utils";
import cloneDeep from "lodash/cloneDeep";

const ERROR_STATUS = 1;
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

            const {pages, template} = getState();
            const page = pages.selectedPage;
            const pageTemplate = cloneDeep(template.template);
            pageTemplate.tabId = page.tabId;

            TemplateService.savePageAsTemplate(pageTemplate).then((response) => {
                if (response.Status === ERROR_STATUS) {
                    dispatch({
                        type: ActionTypes.ERROR_SAVING_TEMPLATE,
                        data: {error: {message: response.Message}}
                    });
                    return;
                }
                
                utils.notify(response.Response);
                dispatch({
                    type: ActionTypes.SAVED_TEMPLATE
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