import React, { Component } from "react";
import RadioButtons from "./index";

export default {
    component: RadioButtons,
};

class MyRadioButtons extends Component {
    constructor() {
        super();
        this.state = { value: 1 };
    }

    render() {
        return (
            <RadioButtons
                options={[
                    { value: "1", label: "Value 1" },
                    { value: "2", label: "Value 2" }
                ]}
                onChange={(value) => this.setState({value:value})}
                value={this.state.value}
            />
        );
    }
}

export const WithContent = () => (
    <MyRadioButtons />
);
