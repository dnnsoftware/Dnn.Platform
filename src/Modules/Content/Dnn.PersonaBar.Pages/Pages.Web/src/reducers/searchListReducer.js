import ActionTypes from "../constants/actionTypes/searchListActionTypes";


export default function searchListReducer(state = { searchList:[] } , action) {
    switch (action.type) {
        case ActionTypes.SAVE_SEARCH_LIST:
            return {...state, searchList: action.data.searchList};

        default:
            return state;
    }
}