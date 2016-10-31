import ActionTypes from "../constants/actionTypes/pageActionTypes";

export default function pagesReducer(state = {
    selectedPage: null,
    doingOperation: false
}, action) {    

    const changeField = function changeField(field, value) {
        const newSelectedPage = {
            ...state.selectedPage
        };  
        newSelectedPage[field] = value;
        
        return newSelectedPage;
    };

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
        
        case ActionTypes.SAVE_PAGE:
            return { ...state,                
                doingOperation: true
            };

        case ActionTypes.SAVED_PAGE:
            return { ...state,
                doingOperation: false
            };
            
        case ActionTypes.ERROR_SAVING_PAGE:
            return { ...state,
                doingOperation: false           
            };
        
        case ActionTypes.CHANGE_FIELD_VALUE:
            return { ...state,
                selectedPage: changeField(action.field, action.value)           
            };

        case ActionTypes.CHANGE_PERMISSIONS:
            return { ...state,
                selectedPage: { ...state.selectedPage,
                    permissions: action.permissions
                }           
            };
        
        default:
            return state;
    }
}