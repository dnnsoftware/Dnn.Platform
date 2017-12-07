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
        dirtyTemplate:true
    };
}

export default function templateReducer(state = { 
    template: getEmptyTemplateModel(),
    errors: [],
    dirtyTemplate:false
}, action) {

    switch (action.type) {
        case ActionTypes.CANCEL_SAVE_AS_TEMPLATE:
        case ActionTypes.LOAD_SAVE_AS_TEMPLATE:
            
            return { ...state,                
                template: getEmptyTemplateModel(),
                errors: [],
                dirtyTemplate:false
            };
        
        case ActionTypes.CHANGE_TEMPLATE_FIELD_VALUE:
            return { ...state,
                template: changeField(state, action.field, action.value),
                dirtyTemplate:true           
            };

        default:
            return state;
    }
}