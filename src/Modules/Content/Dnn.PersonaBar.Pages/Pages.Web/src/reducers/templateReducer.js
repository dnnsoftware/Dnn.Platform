import ActionTypes from "../constants/actionTypes/templateActionTypes";

function changeField(state, field, value) {
    const template = {
        ...state.template
    };  
    template[field] = value;
    
    return template;
}

function getEmptyTemplateModel() {
    return {
        name: "",
        description: "",
        includeContent: false,
        folder: null
    };
}

export default function templateReducer(state = { 
    template: getEmptyTemplateModel(),
    errors: []
}, action) {

    switch (action.type) {
        case ActionTypes.LOAD_SAVE_AS_TEMPLATE:
            return { ...state,                
                template: getEmptyTemplateModel(),
                errors: []
            };
        
        case ActionTypes.CHANGE_TEMPLATE_FIELD_VALUE:
            return { ...state,
                template: changeField(state, action.field, action.value)           
            };

        default:
            return state;
    }
}