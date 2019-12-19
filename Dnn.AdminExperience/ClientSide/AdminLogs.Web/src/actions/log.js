import {log as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";

const logActions = {
    getLogList(searchParameters, callback) {
        return (dispatch) => {
            ApplicationService.getLogList(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_LOG_LIST,
                    data: {
                        logList: data.Results,
                        selectedRowIds: [],
                        excludedRowIds: data.Results.map((row) => row.LogGUID),
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getPortalList(addAll, callback) {
        return (dispatch) => {
            ApplicationService.getPortalList(addAll, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTAL_LIST,
                    data: {
                        portalList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getLogTypeList(callback) {
        return (dispatch) => {
            ApplicationService.getLogTypes(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_LOGTYPE_LIST,
                    data: {
                        logTypeList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    deleteLogItems(payload, callback) {
        return (dispatch) => {
            ApplicationService.deleteLogItems(payload, data => {
                dispatch({
                    type: ActionTypes.DELETED_LOG_ITEMS,
                    data: {
                        selectedRowIds: [],
                        excludedRowIds: []
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    clearLog(callback) {
        return (dispatch) => {
            ApplicationService.clearLog(data => {
                dispatch({
                    type: ActionTypes.CLEARED_LOG,
                    data: {
                        selectedRowIds: [],
                        excludedRowIds: []
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    emailLogItems(payload, callback) {
        return (dispatch) => {
            payload.Email = payload.Email.split(/[ ,;]+/).filter((v) => { return v.trim().length > 0; }).join();
            ApplicationService.emailLogItems(payload, data => {
                dispatch({
                    type: ActionTypes.EMAILED_LOG_ITEMS,
                    data: {
                        logList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    selectRow(rowId) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECTED_ROW,
                data: {
                    rowId: rowId
                }
            });
        };
    },
    deselectRow(rowId) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.DESELECTED_ROW,
                data: {
                    rowId: rowId
                }
            });
        };
    },
    selectAll() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECTED_ALL,
                data: {}
            });
        };
    },
    deselectAll() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.DESELECTED_ALL,
                data: {}
            });
        };
    }
};

export default logActions;
