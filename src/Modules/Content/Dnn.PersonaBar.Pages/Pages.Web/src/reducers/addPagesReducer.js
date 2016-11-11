import ActionTypes from "../constants/actionTypes/addPagesActionTypes";

function getEmptyBulkPage() {
    return {
        bulkPages: "",
        parentId: null,
        keywords: "",
        tags: "",
        includeInMenu: true,
        startDate: null,
        endDate: null
    };
}

function changeField(state, field, value) {
    const newBulkPage = {
        ...state.bulkPage
    };  
    newBulkPage[field] = value;
    
    return newBulkPage;
}

export default function addPagesReducer(state = { 
    bulkPage: getEmptyBulkPage(),
    bulkPageResponse: null
}, action) {

    switch (action.type) {
        case ActionTypes.LOAD_ADD_MULTIPLE_PAGES:
            return { ...state,                
                bulkPage: getEmptyBulkPage()
            };

        case ActionTypes.SAVED_MULTIPLE_PAGES:
            return { ...state,                
                bulkPageResponse: action.data.response
            };
        
        case ActionTypes.CHANGE_MULTIPLE_PAGE_VALUE:
            return { ...state,
                bulkPage: changeField(state, action.field, action.value)           
            };

        default:
            return state;
    }
}