import React, { Component } from "react";
import { action } from "storybook/actions";
import Switch from "./index";

export default {
    component: Switch,
};

class MyControlledSwitch extends Component {
    constructor(props) {
        super(props);
        this.state = { value: props.value ? props.value : false};
    }

    render() {
        return (
            <Switch
                labelHidden={false}
                onText="On"
                offText="Off"
                label="Controlled Switch"
                onChange={(value) => this.setState({ value: value })}
                labelPlacement="left"
                value={this.state.value}    
            />
        );
    }
}

export const WithOff = () => (
    <Switch
        labelHidden={false}
        onText="On"
        offText="Off"
        label="Example Switch"
        onChange={action("changed")}
        labelPlacement="left"
    />
);

export const WithOn = () => (
    <Switch
        labelHidden={false}
        onText="On"
        offText="Off"
        label="Example Switch"
        onChange={action("changed")}
        labelPlacement="left"
        value={true}
    />
);

export const ControlledSwitch = () => (
    <MyControlledSwitch />
);