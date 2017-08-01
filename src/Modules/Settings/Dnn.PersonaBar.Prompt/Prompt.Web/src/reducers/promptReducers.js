import { prompt as ActionTypes } from "../constants/actionTypes";
import util from "../utils";
export default function logList(state = {
    commandList: [],
    pagingInfo: null,
    reload: false,
    fieldOrder: null,
    output: null,
    isHtml: false,
    isError: false,
    data: null,
    clearOutput: false,
    style: null,
    isHelp: false,
    name: null,
    description: null,
    options: null,
    resultHtml: null,
    error: null
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_COMMAND_LIST:
            return {
                ...state,
                commandList: action.data.commands
            };
        case ActionTypes.EXECUTED_COMMAND:
            if (action.data.result.Message) {
                return {
                    ...state,
                    isHelp: false,
                    isError: true,
                    output: action.data.result.Message,
                    style: action.style
                };
            } else {
                return {
                    ...state,
                    isHelp: false,
                    pagingInfo: action.data.result.pagingInfo,
                    isHtml: action.data.result.isHtml,
                    isError: action.data.result.isError,
                    reload: action.data.result.mustReload,
                    data: action.data.result.data,
                    fieldOrder: action.data.result.fieldOrder,
                    output: action.data.result.output,
                    style: action.style
                };
            }
        case ActionTypes.EXECUTED_HELP_COMMAND:
            if (action.data.result.Message) {
                return {
                    ...state,
                    isHelp: false,
                    isError: true,
                    output: action.data.result.Message,
                    style: action.style
                };
            } else {
                return {
                    ...state,
                    isHelp: true,
                    name: action.data.result.name,
                    description: action.data.result.description,
                    options: action.data.result.options,
                    resultHtml: action.data.result.resultHtml,
                    error: action.data.result.error,
                    isError: action.data.result.error !== undefined && action.data.result.error !== null && action.data.result.error !== ""
                };
            }
        case ActionTypes.EXECUTED_LOCAL_COMMAND:
            switch (action.command) {
                case "CLS":
                case "CLEAR-SCREEN":
                    return {
                        ...state,
                        isHelp: false,
                        pagingInfo: null,
                        output: action.output,
                        clearOutput: true,
                        style: action.style
                    };
                case "RELOAD":
                    return {
                        ...state,
                        isHelp: false,
                        pagingInfo: null,
                        reload: true,
                        output: action.output,
                        clearOutput: false,
                        style: action.style
                    };
                case "ERROR":
                    return {
                        ...state,
                        isHelp: false,
                        pagingInfo: null,
                        reload: false,
                        output: action.output,
                        clearOutput: false,
                        isError: true,
                        data: null,
                        isHtml: false,
                        style: action.style
                    };
                case "CLH":
                case "CLEAR-HISTORY":
                case "INFO":
                    return {
                        ...state,
                        isHelp: false,
                        pagingInfo: null,
                        reload: false,
                        output: action.output,
                        clearOutput: false,
                        isError: false,
                        data: null,
                        isHtml: false,
                        style: action.style
                    };
                case "EXIT":
                    util.utilities.closePersonaBar();
                    return {
                        ...state
                    };
                default:
                    return {
                        ...state
                    };
            }
        default:
            return {
                ...state
            };
    }
}
