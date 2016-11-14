import {
    pageActionTypes as PageActionTypes,
    addPagesActionTypes as AddPagesActionTypes,
    templateActionTypes as TemplateActionTypes
} from "../constants/actionTypes";
import panels  from "../constants/panels";

export default function visiblePanel(state = {
    selectedPage: panels.MAIN_PANEL
}, action) {
    
    switch (action.type) {

        case PageActionTypes.SAVED_PAGE:
            return { ...state,
                selectedPage: panels.MAIN_PANEL
            };
        
        case PageActionTypes.CANCEL_PAGE:
            return { ...state,
                selectedPage: panels.MAIN_PANEL
            };
        
        case PageActionTypes.LOAD_PAGE:
            return { ...state,
                selectedPage: panels.PAGE_SETTINGS_PANEL
            };

        case AddPagesActionTypes.LOAD_ADD_MULTIPLE_PAGES:
            return { ...state,
                selectedPage: panels.ADD_MULTIPLE_PAGES_PANEL
            };

        case AddPagesActionTypes.CANCEL_ADD_MULTIPLE_PAGES:
            return { ...state,
                selectedPage: panels.MAIN_PANEL
            };
        
        case AddPagesActionTypes.SAVED_MULTIPLE_PAGES:
            return { ...state,
                selectedPage: panels.MAIN_PANEL
            };

        case TemplateActionTypes.LOAD_SAVE_AS_TEMPLATE:
            return { ...state,
                selectedPage: panels.SAVE_AS_TEMPLATE_PANEL
            };

        case TemplateActionTypes.CANCEL_SAVE_AS_TEMPLATE:
            return { ...state,
                selectedPage: panels.PAGE_SETTINGS_PANEL
            };
        
        default:
            return state;
    }
}
