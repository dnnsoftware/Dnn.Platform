import reducer from "../../reducers/promptReducers";
import types from "../../constants/actionTypes";

const initialState = {
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

describe("Prompt reducers", () => {

    it("Should return initial state", () => {
        expect(reducer(undefined, {})).toEqual(initialState);
    });

    it(`Should handle ${types.RETRIEVED_COMMAND_LIST}`, () => {

        const commands = {
            CMD_1: "FAKE 1",
            CMD_2: "FAKE 2"
        };
        const state = reducer(initialState, {
            type: types.RETRIEVED_COMMAND_LIST,
            data: {
                commands
            }
        });
        expect(state.commandList).toEqual(commands);
    });

    it(`Should handle ${types.EXECUTED_COMMAND}`, () => {

        const data = {
            result: {
                isHelp: false,
                pagingInfo: {},
                isHtml: true,
                isError: false,
                mustReload: false,
                data: {fld1: "fld1", fld2: "fld2"},
                fieldOrder: null,
                output: `Excuted: ${types.EXECUTED_COMMAND}`,
                nextPageCommand: ""
            }
        };
        const expected = {
            isHelp: false,
            pagingInfo: {},
            isHtml: true,
            isError: false,
            reload: false,
            data: {fld1: "fld1", fld2: "fld2"},
            fieldOrder: null,
            output: `Excuted: ${types.EXECUTED_COMMAND}`,
            nextPageCommand: "",
            style: ""
        };
        const state = reducer(initialState, {
            type: types.EXECUTED_COMMAND,
            style: "",
            data
        });

        expect(state).toEqual({...state, ...expected});
    });

    it(`Should handle ${types.EXECUTED_HELP_COMMAND}`, () => {
        const data = {
            result: {
                commandList: null,
                isHelp: true,
                name: "name",
                description: "description",
                options: {},
                resultHtml: "<h1>HELP</h1>",
                error: null,
                isError: false
            }
        };

        const expected = {
            commandList: null,
            isHelp: true,
            name: "name",
            description: "description",
            options: {},
            resultHtml: "<h1>HELP</h1>",
            error: null,
            isError: false
        };

        const state = reducer(initialState, {
            type: types.EXECUTED_HELP_COMMAND,
            data
        });

        expect(state).toEqual({...initialState, ...expected});
    });

    it(`Should handle ${types.EXECUTED_LOCAL_COMMAND}: CLS, CLEAR-SCREEN`, () => {

        const data = {
            type: types.EXECUTED_LOCAL_COMMAND,
            style: "",
            isHelp: false,
            pagingInfo: null,
            clearOutput: true,
            output: "",
            command: "CLS"
        };

        const expected = {
            isHelp: false,
            pagingInfo: null,
            clearOutput: true,
            style: "",
            output: ""
        };

        let state = reducer(initialState, data);

        expect(state).toEqual({...initialState, ...expected});

        data.command = "CLEAR-SCREEN";

        state = reducer(initialState, data);

        expect(state).toEqual({...initialState, ...expected});
    });

    it(`Should handle ${types.EXECUTED_LOCAL_COMMAND}: RELOAD`, () => {

        const data = {
            type: types.EXECUTED_LOCAL_COMMAND,
            style: "",
            output: "",
            command: "RELOAD"
        };

        const expected = {
            isHelp: false,
            pagingInfo: null,
            clearOutput: false,
            style: "",
            output: "",
            reload: true
        };

        const state = reducer(initialState, data);

        expect(state).toEqual({...initialState, ...expected});

    });

    it(`Should handle ${types.EXECUTED_LOCAL_COMMAND}: CLH, CLEAR-HISTORY, INFO`, () => {

        const data = {
            type: types.EXECUTED_LOCAL_COMMAND,
            style: "",
            output: "",
            command: "CLH"
        };

        const expected = {
            isHelp: false,
            pagingInfo: null,
            clearOutput: false,
            style: "",
            output: "",
            reload: false
        };

        const state = reducer(initialState, data);

        expect(state).toEqual({...initialState, ...expected});

    });
});