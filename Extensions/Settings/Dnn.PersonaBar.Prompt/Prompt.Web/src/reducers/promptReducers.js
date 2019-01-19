import { util } from "../utils/helpers";
import actionTypes from "../constants/actionTypes";

export default function promptReducers(state = {
    commandList: null,
    pagingInfo: null,
    nextPageCommand: null,
    reload: false,
    fieldOrder: null,
    output: null,
    isHtml: false,
    isError: false,
    isBusy: false,
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
        case actionTypes.RETRIEVED_COMMAND_LIST:
            return {
                ...state,
                commandList: action.data.commands,
                pagingInfo: null,
                nextPageCommand: null,
                reload: false,
                fieldOrder: null,
                output: null,
                isHtml: false,
                isError: false,
                data: null,
                clearOutput: false,
                style: null,
                isHelp: true,
                name: null,
                description: null,
                options: null,
                resultHtml: null,
                error: null,
                isBusy: false
            };
        case actionTypes.END_PAGING:
            return {
                ...state,
                commandList: null,
                pagingInfo: null,
                nextPageCommand: null,
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
                error: null,
                isBusy: false
            };
        case actionTypes.EXECUTED_COMMAND:
            if (action.data.result.Message) {
                return {
                    ...state,
                    isHelp: false,
                    isError: true,
                    output: action.data.result.Message,
                    style: action.style,
                    isBusy: false
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
                    nextPageCommand: action.data.result.nextPageCommand,
                    style: action.style,
                    isBusy: false
                };
            }
        case actionTypes.EXECUTED_HELP_COMMAND:
            if (action.data.result.Message) {
                return {
                    ...state,
                    isHelp: false,
                    isError: true,
                    output: action.data.result.Message,
                    style: action.style,
                    isBusy: false
                };
            } else {
                return {
                    ...state,
                    commandList: null,
                    isHelp: true,
                    name: action.data.result.name,
                    description: action.data.result.description,
                    options: action.data.result.options,
                    resultHtml: action.data.result.resultHtml,
                    error: action.data.result.error,
                    isError: action.data.result.error !== undefined && action.data.result.error !== null && action.data.result.error !== "",
                    isBusy: false
                };
            }
        case actionTypes.EXECUTED_LOCAL_COMMAND:
            switch (action.command) {
                case "CLS":
                case "CLEAR-SCREEN":
                    return {
                        ...state,
                        isHelp: false,
                        pagingInfo: null,
                        output: action.output,
                        clearOutput: true,
                        style: action.style,
                        isBusy: false
                    };
                case "RELOAD":
                    return {
                        ...state,
                        isHelp: false,
                        pagingInfo: null,
                        reload: true,
                        output: action.output,
                        clearOutput: false,
                        style: action.style,
                        isBusy: false
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
                        style: action.style,
                        isBusy: false
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
                        style: action.style,
                        isBusy: false
                    };
                case "EXIT":
                    util.utilities.closePersonaBar();
                    return {
                        ...state,
                        isBusy: false
                    };
                default:
                    return {
                        ...state,
                        isBusy: false
                    };
            }
        default:
            return {
                ...state,
                isBusy: false
            };
    }
}
