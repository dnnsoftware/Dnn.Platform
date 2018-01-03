import React from "react";
import { shallow } from "enzyme";
import { Output } from "../../components/Output";
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
    error: null
};

jest.mock("../../components/Prompt.less", () => jest.fn());

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

        const out = shallow(<Output {...props} />);

        expect(out).toMatchSnapshot();
    });

});

