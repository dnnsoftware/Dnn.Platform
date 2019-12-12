import { task as ActionTypes } from "../constants/actionTypes";

export default function taskList(state = {
    taskStatusList: [],
    totalCount: 0
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_TASK_STATUS_LIST:
            return { ...state,
                serverTime: action.data.serverTime,
                schedulingEnabled: action.data.schedulingEnabled,
                status: action.data.status,
                freeThreads: action.data.freeThreads,
                activeThreads: action.data.activeThreads,
                maxThreads: action.data.maxThreads,
                taskProcessingList: action.data.taskProcessingList,
                taskStatusList: action.data.taskStatusList,
                totalCount: action.data.totalCount
            };
        case ActionTypes.RETRIEVED_SCHEDULE_SETTINGS:
            return { ...state,
                schedulerModeOptions: action.data.schedulerModeOptions,
                schedulerMode: action.data.schedulerMode,
                schedulerDelay: action.data.schedulerDelay
            };
        case ActionTypes.UPDATED_SCHEDULE_SETTINGS:
            return { ...state,
                schedulerMode: action.data.schedulerMode,
                schedulerDelay: action.data.schedulerDelay,
                settingsClientModified: action.data.settingsClientModified
            };
        case ActionTypes.RETRIEVED_SCHEDULE_HISTORY:
            return { ...state,
                taskHistoryList: action.data.taskHistoryList,
                totalHistoryCount: action.data.totalHistoryCount
            };
        case ActionTypes.RETRIEVED_SCHEDULE_ITEMS:
        case ActionTypes.DELETED_SCHEDULE_ITEM:
            return { ...state,
                schedulerItemList: action.data.schedulerItemList,
                totalCount: action.data.totalCount
            };
        case ActionTypes.RETRIEVED_SERVER_LIST:
            return { ...state,
                serverList: action.data.serverList
            };
        case ActionTypes.RETRIEVED_SCHEDULE_ITEM:
            return { ...state,
                scheduleItemDetail: action.data.scheduleItemDetail,
                settingsClientModified: action.data.settingsClientModified
            };
        case ActionTypes.CREATED_SCHEDULE_ITEM:
        case ActionTypes.UPDATED_SCHEDULE_ITEM:
        case ActionTypes.CANCELED_SCHEDULE_ITEM_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                settingsClientModified: action.data.settingsClientModified
            };
        case ActionTypes.SCHEDULE_ITEM_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                scheduleItemDetail: action.data.scheduleItemDetail,
                settingsClientModified: action.data.settingsClientModified
            };
        case ActionTypes.UPDATED_SCHEDULE_SETTINGS_PENDING:
            return { ...state,
                schedulerMode: action.data.schedulerMode
            };
        default:
            return { ...state
            };
    }
}
