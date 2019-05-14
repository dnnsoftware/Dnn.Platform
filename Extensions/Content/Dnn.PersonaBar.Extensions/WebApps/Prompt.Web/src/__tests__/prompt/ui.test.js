import Command from "../../components/Command";

jest.mock("../../components/Prompt.less", () => jest.fn());
jest.mock("../../utils/helpers");
jest.mock("../../localization/Localization");

import React from "react";
import Enzyme, {mount, shallow, render} from "enzyme";
import {Output} from "../../components/Output";
import reducer from "../../reducers/promptReducers";
import types from "../../constants/actionTypes";
import Adapter from "enzyme-adapter-react-16";
import DataTable from "../../components/DataTable";
import {formatLabel} from "../../utils/helpers";
import Help from "../../components/Help";

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

Enzyme.configure({adapter: new Adapter()});

describe("UI snapshots for state change", () => {

    it("Should handle initial state", () => {

        const props = reducer(undefined, {});

        const out = render(<Output {...props} IsPaging={jest.fn()} busy={jest.fn()} scrollToBottom={jest.fn()}
                                   className="test"/>);

        expect(out).toMatchSnapshot();
    });

    it(`Should render Output component simulating a ${types.RETRIEVED_COMMAND_LIST} action with mocked data`, () => {

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

        const out = render(<Output {...props} IsPaging={jest.fn()} busy={jest.fn()} scrollToBottom={jest.fn()}
                                   className="test"/>);

        expect(out).toMatchSnapshot();
    });

    it(`Should render Output component simultaing a ${types.EXECUTED_COMMAND} action with mocked data`, () => {
        const commands = [
            {
                ScheduleId: 20,
                FriendlyName: "Analytics Engage",
                NextStart: "2018-01-03 18:25",
                Enabled: true
            },
            {
                ScheduleId: 22,
                FriendlyName: "Content Personalization Data Clean",
                NextStart: "2018-01-04 12:46",
                Enabled: true
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

        const out = render(<Output {...props} IsPaging={jest.fn()} busy={jest.fn()} scrollToBottom={jest.fn()}
                                   className="test"/>);

        expect(out).toMatchSnapshot();
    });

    it(`Should render DataTable according to mocked rows and columns`, () => {

        const rows = [
            {
                Email: "host@change.me",
                IsAuthorized: true,
                IsDeleted: false,
                IsLockedOut: false,
                LastLogin: "2018-01-04",
                UserId: 1,
                Username: "host",
                __Email: "get-user 'host@change.me'",
                __UserId: "get-user 1",
                __Username: "get-user 'host'"
            }
        ];

        const columns = ["UserId", "Username", "Email", "LastLogin", "IsDeleted", "IsAuthorized", "IsLockedOut"];

        const cssClass = "test-ui-class";

        const wrapper = render(<DataTable rows={rows} columns={columns} cssClass={cssClass}/>);

        expect(wrapper.find("th")).toHaveLength(7);
        expect(wrapper.find("tr")).toHaveLength(2);
        expect(wrapper.find("th").first().text()).toEqual(formatLabel(columns[0]));
        expect(wrapper).toMatchSnapshot();

    });

    it("Should render Command component according to mocked commandsList", () => {

        const commandsList = [
            {
                Category: "General Commands",
                Description: "Clears the Prompt console. <code>cls</code> is a shortcut for <a href=\"#clear-screen\"><code>clear-screen</code></a>",
                Key: "CLS",
                Name: "cls",
                Version: "1.5.0.0"
            },
            {
                Category: "General Commands",
                Description: "Lists all the commands.",
                Key: "LIST-COMMANDS",
                Name: "list-commands",
                Version: "1.5.0.0"
            },
            {
                Category: "General Commands",
                Description: "Clears history of commands used in current session",
                Key: "CLEAR-HISTORY",
                Name: "clear-history",
                Version: "1.5.0.0"
            },
            {
                Category: "General Commands",
                Description: "Sets the DNN View Mode. This has the same effect as clicking the appropriate options in the DNN Control Bar.",
                Key: "SET-MODE",
                Name: "set-mode",
                Version: "1.5.0.0"
            }
        ];

        const wrapper = render(<Command commandList={commandsList} IsPaging={jest.fn()}/>);

        const commandsTableRows = wrapper.find("table > tbody > tr");

        expect(commandsTableRows).toHaveLength(commandsList.length + 1);
        expect(wrapper).toMatchSnapshot();
    });

    it("Should render Help component according to set properties", () => {

        const props = {
            IsPaging: jest.fn(),
            style: "test-style",
            isError: false,
            error: "",
            name: "test-name",
            description: "test-description",
            options: [
                {
                    Flag: "test-flag",
                    Type: "test-type",
                    Required: false,
                    DefaultValue: "",
                    Description: "test-description"
                },
                {
                    Flag: "test-flag2",
                    Type: "test-type2",
                    Required: false,
                    DefaultValue: "",
                    Description: "test-description2"
                }
            ],
            resultHtml: "<h1>TEST-HTML</h1>"
        };

        const wrapper = mount(<Help {...props}/>);
        const heading = wrapper.find("section > h3");
        const anchor = wrapper.find("section > a").find({name: "test-name"});

        expect(heading.text()).toEqual("test-name");
        expect(anchor).toHaveLength(1);
        expect(wrapper.find(DataTable)).toHaveLength(1);
    });
});
