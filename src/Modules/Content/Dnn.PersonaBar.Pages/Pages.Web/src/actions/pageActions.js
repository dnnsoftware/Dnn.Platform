import ActionTypes from "../constants/actionTypes/pageActionTypes";
import PagesService from "../services/pageService";

const pageActions = {
    loadPage(pageId) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_PAGE
            });    

            PagesService.getPage(pageId).then(response => {
                dispatch({
                    type: ActionTypes.LOADED_PAGE,
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

export default pageActions;