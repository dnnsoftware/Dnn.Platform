import ActionTypes from "../constants/actionTypes/pageActionTypes";
import validateFields from "../validation";

export default function pagesReducer(state = {
    selectedPage: null,
    errors: {},
    cacheProviderList: null,
    editingSettingModuleId: null,
    dirtyPage: false,
    selectedPageSettingTab: 0,
    workflowList: [],
    dirtyCustomDetails: false
}, action) {



    const changeField = function changeField(field, value) {
        const newSelectedPage = {
            ...state.selectedPage
        };
        newSelectedPage[field] = value;
        return newSelectedPage;
    };

    const changeModuleCopy = function changeModuleCopy(id, key, value) {
        const modules = [...state.selectedPage.modules];
        return modules.map((m) => {
            if (m.id === id) {                
                return {
                    ...m,
                    [key]: typeof(value) === "boolean" ? value : parseInt(value)
                };
            }
            return m;
        }); 
    };

    switch (action.type) {
        case ActionTypes.GET_WORKFLOW_LIST:
        {
            return {
                ...state,
                workflowList: action.data.workflowList
            };
        }
        case ActionTypes.SELECT_PAGE_SETTING_TAB:
            return { ...state,
                selectedPageSettingTab: action.selectedPageSettingTab
            };
        case ActionTypes.LOAD_PAGE:
            return { ...state,                
                selectedPage: null,
                editingSettingModuleId: null
            };

        case ActionTypes.LOADED_PAGE:
            return { ...state,
                selectedPage: action.data.page,
                errors: {},
                dirtyPage: false,
                cachedPageCount: null,
                selectedPageSettingTab: action.selectedPageSettingTab
            };

        case ActionTypes.CHANGE_FIELD_VALUE:
            return { ...state,
                selectedPage: changeField(action.field, action.value),
                errors: {
                    ...(state.errors),
                    ...validateFields(action.field, action.value)
                },
                dirtyPage: true
            };

        case ActionTypes.CHANGE_PERMISSIONS:
            return { ...state,
                selectedPage: { ...state.selectedPage,
                    permissions: action.permissions
                },
                dirtyPage: true
            };

        case ActionTypes.FETCH_CACHE_PROVIDER_LIST:
            return state;
            
        case ActionTypes.FETCHED_CACHE_PROVIDER_LIST:
            return { ...state,
                cacheProviderList: action.data.cacheProviderList                           
            };

        case ActionTypes.ERROR_FETCHING_CACHE_PROVIDER_LIST:
            return state;
        
        case ActionTypes.EDITING_PAGE_MODULE: {
            const editingSettingModuleId = action.data.module.id;
            return { ...state,
                editingSettingModuleId
            };
        }
        case ActionTypes.CANCEL_PAGE:
            return { ...state,                
                selectedPage: null,
                errors: {},
                editingSettingModuleId: null,
                dirtyPage: false,
                selectedPageSettingTab: 0
            };

        case ActionTypes.SAVE_PAGE:
            if (action.data) {
                return {...state, 
                    selectedPage: { ...state.selectedPage,
                        tags: action.data.tags.join()
                    },
                    dirtyPage:false
                };
            }
            else {
                return {...state, dirtyPage:false};
            }

        case ActionTypes.CANCEL_EDITING_PAGE_MODULE: {
            const editingSettingModuleId = null;
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
        
        case ActionTypes.UPDATED_PAGE_MODULE_COPY: {
            const modules = changeModuleCopy(action.data.id, action.data.key, action.data.event) ;
            return { ...state,
                selectedPage: {
                    ...state.selectedPage, 
                    modules
                },
                dirtyPage: true                   
            };
        }
        
        case ActionTypes.ADD_CUSTOM_URL:
        case ActionTypes.REPLACE_CUSTOM_URL:
        case ActionTypes.DELETE_CUSTOM_URL: {
            const pageUrls = action.payload.pageUrls;
            
            return { ...state,
                selectedPage: {
                    ...state.selectedPage, 
                    pageUrls
                }
            };
        }

        case ActionTypes.RETRIEVED_CACHED_PAGE_COUNT:
            return { ...state,                
                cachedPageCount: action.data.cachedPageCount
            };

        case ActionTypes.CLEARED_CACHED_PAGE:
            return { ...state,                
                cachedPageCount: 0
            };

        case ActionTypes.CLEAR_SELECTED_PAGE:
            return {
                ...state,
                selectedPage: null
            };

        case ActionTypes.CUSTOM_PAGE_DETAILS_UPDATED:
            return {
                ...state,
                dirtyCustomDetails: true
            };

        case ActionTypes.GET_CURRENT_SELECTED_PAGE:
        default:
            return state;
    }
}
