import ActionTypes from "../constants/actionTypes/pageListActionTypes";


export default function  pageListReducer(state = { pageList:[] } , action) {
    switch (action.type) {
        case ActionTypes.SAVE:
            return {...state, pageList:JSON.parse(JSON.stringify(action.data.pageList))};

        case ActionTypes.SAVE_CHILDREN:
            console.log(action);
            return state;
        default:
            return state;
    }
}