import ActionTypes from "../constants/actionTypes/searchListActionTypes";


export default function searchListReducer(state = { searchList:[], searchResult:null } , action) {
    switch (action.type) {
        case ActionTypes.SAVE_SEARCH_LIST:
            return {...state, searchList: action.data.searchList};
        case ActionTypes.SAVE_SEARCH_RESULT:
            if (action.data.filtersUpdated) {
                return {
                    ...state, 
                    searchResult: action.data.searchResult,
                    searchList: action.data.searchResult.Results,
                    filtersUpdated : action.data.filtersUpdated,
                    filterPage: action.data.filterPage
                };
            } else {
                return {
                    ...state, 
                    searchResult: action.data.searchResult,
                    searchList: state.searchList.concat(action.data.searchResult.Results),
                    filtersUpdated : action.data.filtersUpdated
                };
            }

            
        default:
            return state;
    }
}