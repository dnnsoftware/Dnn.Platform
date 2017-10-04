jest.mock("./style.less", () => jest.fn());
jest.useFakeTimers();

import React, {PropTypes} from "react";
import Dropdown from "./Dropdown";
import Collapse from "react-collapse";

import {configure} from 'enzyme';
import Adapter from 'enzyme-adapter-react-15.4';

import {shallow, mount, render} from "enzyme";

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
        const wrapper = shallow(<Dropdown onSelect={f => f} {...props}/>);
        expect(wrapper).toMatchSnapshot();

        expect(wrapper.state('dropDownOpen')).toBe(false);

        let collapsibleLabel = wrapper.find(".collapsible-label").first();
        collapsibleLabel.simulate("click");

        expect(wrapper.state('dropDownOpen')).toBe(true);

        expect(wrapper.update()).toMatchSnapshot();
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

    it("Renders <Collapse /> with minimal setup", () => {
        const props = {options, value: "second", label: "My new label", prependWith: "Prepend text:", withIcon: false};
        const deepRendering = mount(<Dropdown onSelect={f => f} {...props}/>);

        const collapseCount = deepRendering.find(Collapse).length;
        expect(collapseCount).toBe(1);
    });
});
