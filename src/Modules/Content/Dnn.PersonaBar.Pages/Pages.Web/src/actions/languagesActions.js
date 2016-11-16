import { languagesActionTypes as ActionTypes } from "../constants/actionTypes";
import LanguageService from "services/languageService";

const languagesActions = {
    getLanguages(tabId, callback) {
        return (dispatch) => {
            LanguageService.getLanguages(tabId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PAGE_LANGUAGES,
                    data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    }
};

export default languagesActions;
