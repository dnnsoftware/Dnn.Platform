import ActionTypes from "../constants/actionTypes/addPagesActionTypes";
import responseStatus from "../constants/responseStatus";
import PagesService from "../services/pageService";
import utils from "../utils";
import Localization from "../localization";

const addPagesActions = {

    loadAddMultiplePages() {
        return {
            type: ActionTypes.LOAD_ADD_MULTIPLE_PAGES,
            data: {}
        };
    },

    addPages(bulkPage) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SAVE_ADD_MULTIPLE_PAGES
            });    

            PagesService.addPages(bulkPage).then(response => {
                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(Localization.get("Error_" + response.Message), 3000);
                    return;
                }

                dispatch({
                    type: ActionTypes.SAVED_MULTIPLE_PAGES,
                    data: {
                        response: response.Response 
                    }
                });  
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_SAVING_MULTIPLE_PAGES,
                    data: {error}
                });
            });     
        };
    },

    cancelAddMultiplePages() {
        return {
            type: ActionTypes.CANCEL_ADD_MULTIPLE_PAGES,
            data: {}
        };
    },

    changeAddMultiplePagesField(key, value) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_MULTIPLE_PAGE_VALUE,
                field: key,
                value
            });
        };
    }
};

export default addPagesActions;