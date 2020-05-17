import { licensing as ActionTypes } from "../constants/actionTypes";

export default function licensingSettings(state = {
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_LICENSING_PRODUCT:
            return { ...state,
                productName: action.data.productName
            };
        case ActionTypes.RETRIEVED_SERVER_INFO:
            return { ...state,
                productVersion: action.data.productVersion
            };
        default:
            return { ...state
            };
    }
}
