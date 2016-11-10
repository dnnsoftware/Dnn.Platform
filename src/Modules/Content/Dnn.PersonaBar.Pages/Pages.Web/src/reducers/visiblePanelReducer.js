import ActionTypes  from "../constants/actionTypes/pageActionTypes";
import panels  from "../constants/panels";

export default function visiblePanel(state = {
    selectedPage: panels.MAIN_PANEL
}, action) {
    
    switch (action.type) {

        case ActionTypes.PAGE_SAVED:
            return { ...state,
                selectedPage: panels.MAIN_PANEL
            };
        
        case ActionTypes.CANCEL_PAGE:
            return { ...state,
                selectedPage: panels.MAIN_PANEL
            };
        
        case ActionTypes.LOAD_PAGE:
            return { ...state,
                selectedPage: panels.PAGE_SETTINGS_PANEL
            };

        case ActionTypes.LOAD_ADD_MULTIPLE_PAGES:
            return { ...state,
                selectedPage: panels.ADD_MULTIPLE_PAGES_PANEL
            };

        case ActionTypes.CANCEL_ADD_MULTIPLE_PAGES:
            return { ...state,
                selectedPage: panels.MAIN_PANEL
            };
        
        default:
            return state;
    }
}
