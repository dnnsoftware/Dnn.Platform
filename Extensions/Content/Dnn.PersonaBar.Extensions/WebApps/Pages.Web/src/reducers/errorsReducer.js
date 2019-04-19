export default function errorsReducer(state = {
    error: null
}, action) {
    if (action.type.startsWith("ERROR_")) {
        
        return { ...state,
                error: action.data.error
            };
    }
    return state;
}
