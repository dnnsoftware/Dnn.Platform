import {visiblePanel as ActionTypes}  from "../constants/actionTypes";
export default function visiblePanel(state = {
    selectedPage: 0,
    selectedPageVisibleIndex: 0
}, action) {
    switch (action.type) {
        case ActionTypes.SELECT_PANEL:
            return { ...state,
                selectedPage: action.payload.selectedPage,
                selectedPageVisibleIndex: action.payload.selectedPageVisibleIndex
            };
        default:
            return { ...state
            };
    }
}
