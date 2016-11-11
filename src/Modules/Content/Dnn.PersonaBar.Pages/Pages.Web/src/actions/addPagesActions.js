import ActionTypes from "../constants/actionTypes/addPagesActionTypes";
import responseStatus from "../constants/responseStatus";
import PagesService from "../services/pageService";
import utils from "../utils";
import Localization from "../localization";

function processResponse(response) {
    const totalCount = response.pages.length;
    const totalCreated = response.pages.filter(p => p.created === 0).length;
    const mainMessage = Localization.get("BulkPageResponseTotalMessage")
        .replace("[PAGES_CREATED]", totalCreated)
        .replace("[PAGES_TOTAL]", totalCount) + "<br/><br/>";
    const errors = response.pages
        .filter(p => p.created !== 0)
        .map(p => "<strong>" + p.pageName + "</strong>: " + p.errorMessage + "<br/>");

    return mainMessage + errors;
}

const addPagesActions = {

    loadAddMultiplePages() {
        return {
            type: ActionTypes.LOAD_ADD_MULTIPLE_PAGES,
            data: {}
        };
    },

    addPages() {
        return (dispatch, getState) => {
            const {addPages} = getState();
            dispatch({
                type: ActionTypes.SAVE_ADD_MULTIPLE_PAGES
            });    

            PagesService.addPages(addPages.bulkPage).then(response => {
                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(Localization.get("Error_" + response.Message), 3000);
                    return;
                }

                utils.confirm(processResponse(response.Response), 
                    Localization.get("Confirm"),
                    Localization.get("Cancel"));

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