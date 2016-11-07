import ActionTypes from "../constants/actionTypes/pageActionTypes";
import validateFields from "../validation";

export default function pagesReducer(state = {
    selectedPage: null,
    errors: {},
    cacheProviderList: null,
    doingOperation: false,
    editingSettingModuleId: null
}, action) {    

    const changeField = function changeField(field, value) {
        const newSelectedPage = {
            ...state.selectedPage
        };  
        newSelectedPage[field] = value;
        
        return newSelectedPage;
    };

    switch (action.type) {
        case ActionTypes.LOAD_PAGE:
            return { ...state,                
                doingOperation: true,
                selectedPage: null,
                editingSettingModuleId: null
            };

        case ActionTypes.LOADED_PAGE:
            return { ...state,
                doingOperation: false,
                selectedPage: action.data.page,
                errors: {}
            };

        case ActionTypes.ERROR_LOADING_PAGE:
            return { ...state,
                doingOperation: false           
            };
        
        case ActionTypes.SAVE_PAGE:
            return { ...state,                
                doingOperation: true
            };

        case ActionTypes.SAVED_PAGE:
            return { ...state,
                doingOperation: false
            };
            
        case ActionTypes.ERROR_SAVING_PAGE:
            return { ...state,
                doingOperation: false           
            };
        
        case ActionTypes.CHANGE_FIELD_VALUE:
            return { ...state,
                selectedPage: changeField(action.field, action.value), 
                errors: {
                    ...(state.errors),
                    ...validateFields(action.field, action.value)
                }          
            };

        case ActionTypes.CHANGE_PERMISSIONS:
            return { ...state,
                selectedPage: { ...state.selectedPage,
                    permissions: action.permissions
                }           
            };

        case ActionTypes.FETCH_CACHE_PROVIDER_LIST:
            return state;
            
        case ActionTypes.FETCHED_CACHE_PROVIDER_LIST:
            return { ...state,
                cacheProviderList: action.data.cacheProviderList                           
            };

        case ActionTypes.ERROR_FETCHING_CACHE_PROVIDER_LIST:
            return state;
        
        case ActionTypes.TOGGLE_EDIT_PAGE_MODULE: {
            const editingSettingModuleId = state.editingSettingModuleId !== action.data.module.id ? action.data.module.id : null;
            return { ...state,
                editingSettingModuleId
            };
        }

        case ActionTypes.DELETED_PAGE_MODULE: {
            const modules = [...state.selectedPage.modules.filter(f => f.id !== action.data.module.id)];
            return { ...state,
                selectedPage: {
                    ...state.selectedPage, 
                    modules
                }
            };
        }        

        default:
            return state;
    }
}