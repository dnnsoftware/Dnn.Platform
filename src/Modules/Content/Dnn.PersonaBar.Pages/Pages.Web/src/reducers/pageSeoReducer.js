import ActionTypes from "../constants/actionTypes/pageSeoTypes";

const statusCodes = [
    {
        label: "Active (200)",
        value: 200
    },
    {
        label: "Redirect (301)",
        value: 301
    }
];

export default function  pageSeoReducer(state = {
    newFormOpened: false,
    addingNewUrl: false,
    editingUrl: false,
    editedUrl: null
}, action) {
    const changeField = function changeField(key, value) {
        const newEditedUrl = {
            ...state.editedUrl
        };  
        
        if (key === "statusCode") {
            newEditedUrl.statusCode = {
                ...state.editedUrl.statusCode
            };
            newEditedUrl[key].Key = value;
            newEditedUrl[key].Value = statusCodes.filter(statusCode => statusCode.value === value)[0].label;
        } else if (key === "siteAlias") { 
            newEditedUrl.siteAlias = {
                ...state.editedUrl.siteAlias
            };
            newEditedUrl[key].Key = value;
        } else {
            if (key === "path" && 
                    (!value || !value.startsWith("/"))) {
                value = "/" + value;
            } else if (key === "queryString" && 
                    (!value || !value.startsWith("?"))) {
                value = "?" + value;
            }
            
            newEditedUrl[key] = value;
        }
        
        return newEditedUrl;
    };
    switch (action.type) {
        case ActionTypes.SEO_OPEN_NEW_FORM:
            return { ...state,
                newFormOpened: true,
                editedUrl: {
                    statusCode: { Key: 200, Value: "Active (200)" },
                    path: "/",
                    siteAlias: { Key: 1 },
                    locale: { Key: 1 },
                    queryString: "",
                    siteAliasUsage: 0
                }
            };
        case ActionTypes.SEO_CLOSE_NEW_FORM:
            return { ...state,
                newFormOpened: false,
                editedUrl: null
            };
        case ActionTypes.SEO_CHANGE_URL:
            return { ...state,
                editedUrl: changeField(action.payload.key, action.payload.value)
            };
        case ActionTypes.SEO_ADD_URL:
            return { ...state,
                addingNewUrl: true
            };
        case ActionTypes.SEO_ADDED_URL:
            return { ...state,
                addingNewUrl: false,
                newFormOpened: false,
                editedUrl: null
            };
        case ActionTypes.ERROR_SEO_ADDING_URL:
            return { ...state,
                addingNewUrl: false
            };
        case ActionTypes.SEO_OPEN_EDIT_FORM:
            return { ...state,
                editedUrl: action.payload.url,
                newFormOpened: false
            };
        case ActionTypes.SEO_CLOSE_EDIT_FORM:
            return { ...state,
                editedUrl: null
            };
        case ActionTypes.SEO_SAVE_URL:
            return { ...state,
                editingUrl: true
            };
        case ActionTypes.SEO_SAVED_URL:
            return { ...state,
                editingUrl: false,
                editedUrl: null
            };
        case ActionTypes.ERROR_SEO_SAVING_URL:
            return { ...state,
                editingUrl: false
            };
            
        default:
            return state;
    }
}