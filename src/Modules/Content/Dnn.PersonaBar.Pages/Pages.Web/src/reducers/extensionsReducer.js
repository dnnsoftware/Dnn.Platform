import ActionTypes from "../constants/actionTypes/extensionsActionTypes";

export default function extensionsReducer(state = { 
    toolbarComponents: [],
    multiplePagesComponents: [],
    pageDetailsFooterComponents: []
}, action) {

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

        default:
            return state;
    }
}