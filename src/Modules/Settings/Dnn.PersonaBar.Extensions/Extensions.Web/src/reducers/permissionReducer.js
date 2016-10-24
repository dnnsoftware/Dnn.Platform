import { permission as ActionTypes } from "../constants/actionTypes";
export default function extension(state = {
    desktopModulePermissions: {}
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_DESKTOPMODULE_PERMISSIONS:
            return { ...state,
                desktopModulePermissions: action.payload
            };
        case ActionTypes.UPDATED_DESKTOPMODULE_PERMISSIONS:
            return { ...state
            };
        default:
            return { ...state
            };
    }
}
