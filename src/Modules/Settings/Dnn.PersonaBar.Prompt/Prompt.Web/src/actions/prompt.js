import { prompt as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";

const promptActions = {
    getCommandList(callback) {
        return (dispatch) => {
            ApplicationService.getCommandList(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_COMMAND_LIST,
                    data: {
                        commands: data
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    runCommand(payload, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.runCommand(payload, data => {
                dispatch({
                    type: ActionTypes.EXECUTED_COMMAND,
                    data: {
                        result: data
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (errorCallback) {
                    errorCallback(data);
                }
            });
        };
    },
    runHelpCommand(payload, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.runCommand(payload, data => {
                dispatch({
                    type: ActionTypes.EXECUTED_HELP_COMMAND,
                    data: {
                        result: data
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (errorCallback) {
                    errorCallback(data);
                }
            });
        };
    },
    runLocalCommand(command, output, style) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.EXECUTED_LOCAL_COMMAND,
                command: command,
                output: output,
                style: style
            });
        };
    },
    endPaging() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.END_PAGING
            });
        };
    },
    changeUserMode(payload, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.changeUserMode(payload, data => {
                dispatch({
                    type: ActionTypes.CHANGE_USER_MODE,
                    data: {
                        result: data
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (errorCallback) {
                    errorCallback(data);
                }
            });
        };
    }
};

export default promptActions;
