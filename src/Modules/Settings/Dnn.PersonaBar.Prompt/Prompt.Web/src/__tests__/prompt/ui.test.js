import React from "react";
import Enzyme, {shallow, render} from "enzyme";
import {Output} from "../../components/Output";
import reducer from "../../reducers/promptReducers";
import types from "../../constants/actionTypes";
import Adapter from "enzyme-adapter-react-15";

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
    error: null
};

jest.mock("../../components/Prompt.less", () => jest.fn());
jest.mock("../../utils/helpers");
jest.mock("../../localization/Localization");

Enzyme.configure({adapter: new Adapter()});

const getProps = (state) => {
    return {
        output: state.output,
        data: state.data,
        paging: state.pagingInfo,
        isHtml: state.isHtml,
        reload: state.reload,
        style: state.style,
        fieldOrder: state.fieldOrder,
        commandList: state.commandList,
        isError: state.isError,
        clearOutput: state.clearOutput,
        isHelp: state.isHelp,
        name: state.name,
        description: state.description,
        options: state.options,
        resultHtml: state.resultHtml,
        error: state.error,
        nextPageCommand: state.nextPageCommand
    };
};

describe("UI snapshots for state change", () => {

    it("Should handle initial state", () => {

        const props = reducer(undefined, {});

        const out = render(<Output {...props} IsPaging={jest.fn()} busy={jest.fn()} scrollToBottom={jest.fn()}
                                   className="test"/>);

        expect(out).toMatchSnapshot();
    });

    it(`Should handle ${types.RETRIEVED_COMMAND_LIST}`, () => {

        const commands = [
            {
                Key: "CLS",
                Name: "cls",
                Category: "General Commands",
                Description: `Clears the Prompt console. <code>cls</code> is a shortcut for <a href="#clear-screen"><code>clear-screen</code></a>`,
                Version: "1.5.0.0"
            },
            {
                Key: "LIST-COMMANDS",
                Name: "list-commands",
                Category: "General Commands",
                Description: "Lists all the commands.",
                Version: "1.5.0.0"
            }
        ];
        const props = reducer(initialState, {
            type: types.RETRIEVED_COMMAND_LIST,
            data: {
                commands
            }
        });

        const out = render(<Output {...props} IsPaging={jest.fn()} busy={jest.fn()} scrollToBottom={jest.fn()} className="test"/>);

        expect(out).toMatchSnapshot();
    });

    it(`Should handle ${types.EXECUTED_COMMAND}, test data got from list-tasks command`, () => {
        const commands = [
            {
                ScheduleId:20,
                FriendlyName:"Analytics Engage",
                NextStart:"2018-01-03 18:25",
                Enabled:true
            },
            {
                ScheduleId:22,
                FriendlyName:"Content Personalization Data Clean",
                NextStart:"2018-01-04 12:46",
                Enabled:true
            }
        ];

        const data = {
            result: {
                isHelp: false,
                pagingInfo: {},
                isHtml: true,
                isError: false,
                mustReload: false,
                data: commands,
                fieldOrder: null,
                output: `Excuted: ${types.EXECUTED_COMMAND}`,
                nextPageCommand: ""
            }
        };

        const props = reducer(initialState, {
            type: types.EXECUTED_COMMAND,
            style: "cmd",
            data
        });

        const out = render(<Output {...props} IsPaging={jest.fn()} busy={jest.fn()} scrollToBottom={jest.fn()} className="test"/>);

        expect(out).toMatchSnapshot();
    });
});
