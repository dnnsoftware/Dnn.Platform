import {vocabularyTermList as ActionTypes}  from "../constants/actionTypes";

export default function vocabularyTermList(state = {
    vocabularyTerms: [],
    totalCount: 0
}, action) {
    switch (action.type) {
        case ActionTypes.UPDATED_VOCABULARY_TERM:
            return {...state,
                vocabularyTerms: [...state.vocabularyTerms.slice(0, action.payload.index), action.payload.updatedTerm, ...state.vocabularyTerms.slice(action.payload.index + 1)]
            };
        case ActionTypes.ADDED_VOCABULARY_TERM:
            return { ...state,
                vocabularyTerms: [...state.vocabularyTerms, action.payload.addedTerm],
                totalCount: action.payload.totalCount
            };
        case ActionTypes.RETRIEVED_VOCABULARY_TERMS:
            return { ...state,
                vocabularyTerms: action.data.vocabularyTerms,
                totalCount: action.data.totalCount
            };
        case ActionTypes.DELETED_VOCABULARY_TERM:
            return { ...state,
                vocabularyTerms: action.payload.vocabularyTerms,
                totalCount: action.payload.totalCount
            };
        case ActionTypes.SET_TERM_SELECTED:
            return {...state,
                vocabularyTerms: [...state.vocabularyTerms.slice(0, action.payload.index), action.payload.updatedTerm, ...state.vocabularyTerms.slice(action.payload.index + 1)]
            };
        case ActionTypes.CLEAR_TERMS_SELECTED:
            return { ...state,
                vocabularyTerms: action.payload.vocabularyTerms
            };
        default:
            return { ...state
            };
    }
}
