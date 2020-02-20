jest.mock("./style.less", () => jest.fn());
jest.useFakeTimers();

import React from "react";
import Dropdown from "./Dropdown";
import Collapse from "react-collapse";

import {configure} from 'enzyme';
import Adapter from 'enzyme-adapter-react-16';
import toJson from 'enzyme-to-json';

import {shallow, mount} from "enzyme";

configure({adapter: new Adapter()});

const options = [
    {value: "first", label: "First Label", searchableValue: "first"},
    {value: "second", label: "Second Label", searchableValue: "second"},
    {value: "third", label: "Third Label", searchableValue: "third"}
];

describe("Dnn Dropdown component", () => {

    it("Renders with minimal setup", () => {

        const wrapper = shallow(<Dropdown options={[]} value={null} onSelect={f => f}/>);
        expect(wrapper).toMatchSnapshot();

    });

    it("Renders labels properly according to props", () => {

        const wrapper = shallow(<Dropdown options={[]} value={null} onSelect={f => f}/>);
        expect(wrapper).toMatchSnapshot();

        wrapper.setProps({label: "My new label"});
        const modifiedWrapper = wrapper.update();
        expect(modifiedWrapper).toMatchSnapshot();
        let collapsibleLabel = modifiedWrapper.find(".collapsible-label").first();

        expect(collapsibleLabel.text()).toEqual("My new label");

        const secondSelection = "second";

        modifiedWrapper.setProps({
            options,
            value: secondSelection,
            label: "My new label",
            prependWith: "Prepend text:"
        });
        const dropDownWithOptions = modifiedWrapper.update();
        collapsibleLabel = dropDownWithOptions.find(".collapsible-label").first();
        expect(dropDownWithOptions).toMatchSnapshot();
        expect(collapsibleLabel.text()).toEqual(`Prepend text: Second Label`);

    });

    it("Displays a dropdown list onClick", () => {

        const props = {options, value: "second", label: "My new label", prependWith: "Prepend text:"};

        const wrapper = mount(<Dropdown onSelect={f => f} {...props}/>);

        let json = toJson(wrapper);
        expect(json).toMatchSnapshot();

        expect(wrapper.state('dropDownOpen')).toBe(false);

        let collapsibleLabel = wrapper.find(".collapsible-label").first();
        collapsibleLabel.simulate("click");

        expect(wrapper.state('dropDownOpen')).toBe(true);

        json = toJson(wrapper.update());

        expect(json).toMatchSnapshot();
    });

    it("Displays/Hides icon when withIcon is true", () => {

        const props = {options, value: "second", label: "My new label", prependWith: "Prepend text:", withIcon: false};
        const wrapper = shallow(<Dropdown onSelect={f => f} {...props}/>);
        expect(wrapper).toMatchSnapshot();

        let icon = wrapper.find(".dropdown-icon");
        expect(icon.length).toBe(0);

        wrapper.setProps({withIcon: true});
        wrapper.update();
        icon = wrapper.find(".dropdown-icon");
        expect(icon.length).toBe(1);
        expect(wrapper).toMatchSnapshot();

    });

    it("Renders <Collapse />", () => {
        const props = {
            options,
            value: "second",
            label: "My new label",
            prependWith: "Prepend text:",
            withIcon: false,
            isDropDownOpen: true
        };
        const deepRendering = mount(<Dropdown onSelect={f => f} {...props}/>);

        const collapse = deepRendering.find(Collapse);
        expect(collapse).toHaveLength(1);

        let collapsibleLabel = deepRendering.find(".collapsible-label").first();
        collapsibleLabel.simulate("click");

        const listItems = deepRendering.find("ul");
        expect(listItems).toHaveLength(1);

        const json = toJson(deepRendering);

        expect(json).toMatchSnapshot();
    });

    it("Calculates correct Dropdown height", () => {
        const props = {
            options,
            value: "second",
            label: "My new label",
            prependWith: "Prepend text:",
            withIcon: false,
            isDropDownOpen: true,
            fixedHeight: 600
        };
        const deepRendering = mount(<Dropdown onSelect={f => f} {...props}/>);
        let collapsibleLabel = deepRendering.find(".collapsible-label").first();
        collapsibleLabel.simulate("click");

        const json = toJson(deepRendering);
        expect(json).toMatchSnapshot();

    });

    it("Extracts text from a complex label, given a getLabelText function", () => {

            const options = [
                {value: "first", label: <div title={"First Label"}>First Label</div>, searchableValue: "first"},
                {value: "second", label: <div title={"Second Label"}>Second Label</div>, searchableValue: "second"},
                {value: "third", label: <div title={"Third Label"}>Third Label</div>, searchableValue: "third"},
            ];

            const props = {
                options,
                value: "",
                label: "My new label",
                prependWith: "Prepend text:",
                withIcon: false,
                isDropDownOpen: true,
                fixedHeight: 600,
                getLabelText: (label) => label.props.title
            };
            const deepRendering = mount(<Dropdown onSelect={f => f} {...props}/>);
            let collapsibleLabel = deepRendering.find(".collapsible-label").first();
            collapsibleLabel.simulate("click");

            const input = deepRendering.find("input");
            input.simulate('change', {target: {value: "t"}});

            const label = deepRendering.find("li.selected > div").first().props().title;

            expect(label).toEqual("Third Label");

    });

});
