import {
    pageActionTypes as PageActionTypes,
    addPagesActionTypes as AddPagesActionTypes,
    templateActionTypes as TemplateActionTypes,
    visiblePageSettings as VisiblePageSettingsActionTypes,
    visiblePanelActionTypes as VisiblePanelActionTypes
} from "../constants/actionTypes";
import panels  from "../constants/panels";

export default function visiblePanel(state = {
    selectedPage: panels.MAIN_PANEL
}, action) {
    
    switch (action.type) {

        case PageActionTypes.SAVED_PAGE:
        case PageActionTypes.CANCEL_PAGE:
        case AddPagesActionTypes.CANCEL_ADD_MULTIPLE_PAGES:
        case AddPagesActionTypes.VALIDATE_MULTIPLE_PAGES:
        case AddPagesActionTypes.SAVED_MULTIPLE_PAGES:
        case VisiblePageSettingsActionTypes.HIDE_CUSTOM_PAGE_SETTINGS:
        case VisiblePanelActionTypes.HIDE_PANEL:
            return { ...state,
                selectedPage: panels.MAIN_PANEL
            };

        case AddPagesActionTypes.LOAD_ADD_MULTIPLE_PAGES:
            return { ...state,
                selectedPage: panels.ADD_MULTIPLE_PAGES_PANEL
            };
            
        case TemplateActionTypes.LOAD_SAVE_AS_TEMPLATE:
            return { ...state,
                selectedPage: panels.SAVE_AS_TEMPLATE_PANEL
            };
        case VisiblePageSettingsActionTypes.SHOW_CUSTOM_PAGE_SETTINGS:
            return {...state,selectedPage: panels.CUSTOM_PAGE_DETAIL_PANEL};

        case PageActionTypes.LOAD_PAGE:
        case TemplateActionTypes.CANCEL_SAVE_AS_TEMPLATE:
        case TemplateActionTypes.SAVED_TEMPLATE:
            return { ...state,
                selectedPage: panels.PAGE_SETTINGS_PANEL
            };

        case VisiblePanelActionTypes.SHOW_PANEL:
            return { ...state,
                selectedPage: action.data.panelId
            };
        
        default:
            return state;
    }
}
