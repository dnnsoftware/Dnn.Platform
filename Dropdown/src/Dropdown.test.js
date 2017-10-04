jest.mock("./style.less", () => jest.fn());

import React, { PropTypes } from "react";
import Dropdown from "./Dropdown";

import { configure } from 'enzyme';
import Adapter from 'enzyme-adapter-react-15.4';

import {shallow, mount, render} from "enzyme";

configure({ adapter: new Adapter() });

describe("Dnn Dropdown component", () => {

    it("Renders with minimal setup",()=>{

        const wrapper = shallow(<Dropdown options={[]} value={null} onSelect={f => f} />);
        expect(wrapper).toMatchSnapshot();

        //const prependedText = wrapper.find(".dropdown-prepend");
        //expect(prependedText.text()).toEqual('');

    });

});