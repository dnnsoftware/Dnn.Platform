import {pagination as ActionTypes}  from "../constants/actionTypes";
import VocabularyService from "../services/vocabularyService";
const paginationActions = {
    loadMore(searchParameters) {
        return (dispatch) => {
            VocabularyService.getVocabularyList(searchParameters, data => {
                dispatch({
                    type: ActionTypes.LOAD_MORE,
                    loadMore: true,
                    payload: searchParameters,
                    data: {
                        vocabularyList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
            });
        };
    },
    loadTab(searchParameters) {
        return (dispatch) => {
            VocabularyService.getVocabularyList(searchParameters, data => {
                dispatch({
                    type: ActionTypes.LOAD_TAB_DATA,
                    loadMore: true,
                    payload: searchParameters,
                    tabIndex: searchParameters.scopeTypeId,
                    data: {
                        vocabularyList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
            });
        };
    }
};

export default paginationActions;
