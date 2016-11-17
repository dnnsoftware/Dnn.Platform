import ActionTypes from "../constants/actionTypes/extensionsActionTypes";

export default function extensionsReducer(state = { 
    toolbarComponents: [],
    multiplePagesComponents: [],
    pageDetailsFooterComponents: [],
    settingsButtonComponents: {}
}, action) {

    function addComponent(component) {
        const newSettingsButtonComponents = { ...state.settingsButtonComponents };
        newSettingsButtonComponents[component.id] = component.component;
        return newSettingsButtonComponents;
    }

    switch (action.type) {
        case ActionTypes.REGISTER_TOOLBAR_COMPONENT:
            return { ...state,                
                toolbarComponents: [...state.toolbarComponents, action.data.component]
            };
        case ActionTypes.REGISTER_MULTIPLE_PAGES_COMPONENT:
            return { ...state,                
                multiplePagesComponents: [...state.multiplePagesComponents, action.data.component]
            };
        case ActionTypes.REGISTER_PAGE_DETAILS_FOOTER_COMPONENT:
            return { ...state,                
                pageDetailsFooterComponents: [...state.pageDetailsFooterComponents, action.data.component]
            };
        case ActionTypes.REGISTER_SETTINGS_BUTTON_COMPONENT:
            return { ...state,                
                settingsButtonComponents: addComponent(action.data.component)
            };

        default:
            return state;
    }
}