import { logSettings as ActionTypes }  from "../constants/actionTypes";
import {
    updateLogSettingList,
    removeLogSetting,
    createKeepMostRecentOptions,
    createOccurrenceOptions
} from "../reducerHelpers";

export default function logSettings(state = {
    logSettingList: [],
    keepMostRecentOptions: [],
    thresholdsOptions: [],
    notificationTimesOptions: [],
    notificationTimeTypesOptions: [],
    totalCount: 0,
    logSettingDetail: {}
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_KEEPMOSTRECENT_OPTIONS:
            return { ...state,
                keepMostRecentOptions: createKeepMostRecentOptions(action.data.keepMostRecent)
            };
        case ActionTypes.RETRIEVED_OCCURRENCE_OPTIONS:
        {
            let occurrenceOptions = createOccurrenceOptions(action.data);
            return { ...state,
                thresholdsOptions: occurrenceOptions.thresholdsOptions,
                notificationTimesOptions: occurrenceOptions.notificationTimesOptions,
                notificationTimeTypesOptions: occurrenceOptions.notificationTimeTypesOptions
            };
        }
        case ActionTypes.RETRIEVED_LOGSETTING_LIST:
            return { ...state,
                logSettingList: action.data.logSettingList
            };
        case ActionTypes.RETRIEVED_LOGSETTING_BY_ID:
            return { ...state,
                logSettingDetail: action.data.logSettingDetail
            };
        case ActionTypes.UPDATED_LOGSETTING:
        {
            let logSettingList = Object.assign([], state.logSettingList);
            return { ...state,
                logSettingList: updateLogSettingList(logSettingList, action.data.logSettingDetail)
            };
        }
        case ActionTypes.DELETED_LOGSETTING:
        {
            let logSettingList = Object.assign([], state.logSettingList);
            return { ...state,
                logSettingList: removeLogSetting(logSettingList, action.data.LogSettingId)
            };
        }
        case ActionTypes.ADDED_LOGSETTING:
        {
            let logSettingList = Object.assign([], state.logSettingList);
            return { ...state,
                logSettingList: updateLogSettingList(logSettingList, action.data.logSettingDetail)
            };
        }
        default:
            return { ...state
            };
    }
}