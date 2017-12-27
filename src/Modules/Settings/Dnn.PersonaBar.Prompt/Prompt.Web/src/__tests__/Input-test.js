import React from "react";
import { shallow } from "enzyme";
import Input from "../components/Input/index";

describe("Input element renders", () => {

    const input = shallow(<Input/>);

    expect(input);
});