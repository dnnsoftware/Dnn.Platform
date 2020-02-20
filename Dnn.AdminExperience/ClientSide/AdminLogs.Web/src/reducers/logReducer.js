import { log as ActionTypes }  from "../constants/actionTypes";

export default function logList(state = {
    logList: [],
    portalList: [],
    logTypeList: [],    
    selectedRowIds: [],
    excludedRowIds: [],
    totalCount: 0
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_LOG_LIST:
            return { ...state,
                logList: action.data.logList, 
                selectedRowIds: action.data.selectedRowIds,               
                excludedRowIds: action.data.excludedRowIds,
                totalCount: action.data.totalCount
            };
        case ActionTypes.RETRIEVED_PORTAL_LIST:
            return { ...state,
                portalList: action.data.portalList
            };
        case ActionTypes.RETRIEVED_LOGTYPE_LIST:
            return { ...state,
                logTypeList: action.data.logTypeList
            };
        /*eslint-disable eqeqeq*/
        case ActionTypes.SELECTED_ROW:
            return {...state,
                selectedRowIds: state.selectedRowIds.concat(action.data.rowId),
                excludedRowIds: state.excludedRowIds.filter((id) => id != action.data.rowId)
            };
        case ActionTypes.DESELECTED_ROW:
            return {...state,
                excludedRowIds: state.excludedRowIds.concat(action.data.rowId),
                selectedRowIds: state.selectedRowIds.filter((id) => id != action.data.rowId)
            };
        case ActionTypes.SELECTED_ALL:
            return { ...state,
                selectedRowIds: state.selectedRowIds.concat(state.excludedRowIds),
                excludedRowIds: []
            };
        case ActionTypes.DESELECTED_ALL:
            return { ...state,
                selectedRowIds: [],
                excludedRowIds: state.excludedRowIds.concat(state.selectedRowIds)
            };
        case ActionTypes.DELETED_LOG_ITEMS:
            return {...state,
                selectedRowIds: action.data.selectedRowIds,
                excludedRowIds: action.data.excludedRowIds
            };
        case ActionTypes.CLEARED_LOG:
            return {...state,
                selectedRowIds: action.data.selectedRowIds,
                excludedRowIds: action.data.excludedRowIds
            };
        default:
            return { ...state
            };
    }
}
