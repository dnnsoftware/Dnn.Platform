import ActionTypes from "../constants/actionTypes/pageActionTypes";

export default function visualizersReducer(state = {
    selectedPage: null,
    doingOperation: false
}, action) {    
    switch (action.type) {
        case ActionTypes.LOAD_PAGE:
            return { ...state,                
                doingOperation: true,
                selectedPage: null
            };

        case ActionTypes.LOADED_PAGE:
            return { ...state,
                doingOperation: false,
                selectedPage: action.data.page
            };
            
        case ActionTypes.ERROR_LOADING_PAGE:
            return { ...state,
                doingOperation: false           
            };
        
        default:
            return state;
    }
}