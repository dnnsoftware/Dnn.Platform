import ActionTypes from "../constants/actionTypes/pageSettings";
import PagesService from "../services/pageService";

const pageSettingsActions = {
    loadPage(pageId) {
        return (dispatch) => {
            PagesService.getPage(pageId).then(response => {
                dispatch({
                    type: ActionTypes.LOAD_PAGE,
                    data: {
                        page: response
                    }
                });  
            }).catch(() => {
                dispatch({
                    type: ActionTypes.ERROR_LOADING_PAGE
                });
            });     
        };
    }
};

export default pageSettingsActions;