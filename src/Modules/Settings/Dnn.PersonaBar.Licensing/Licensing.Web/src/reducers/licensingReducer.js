import { licensing as ActionTypes } from "../constants/actionTypes";

export default function licensingSettings(state = {
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_LICENSING_PRODUCT:
            return { ...state,
                productName: action.data.productName
            };
        default:
            return { ...state
            };
    }
}
