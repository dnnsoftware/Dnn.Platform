import React, { Component } from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import Switch from "./index";

class ControlledSwitch extends Component {
  constructor(props){
    super(props);
    this.state = { value: props.value ? props.value : false}
  }

  render(){
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

storiesOf("Switch", module).add("with off", () => (
  <Switch
    labelHidden={false}
    onText="On"
    offText="Off"
    label="Example Switch"
    onChange={action("changed")}
    labelPlacement="left"
  />
));

storiesOf("Switch", module).add("with on", () => (
  <Switch
    labelHidden={false}
    onText="On"
    offText="Off"
    label="Example Switch"
    onChange={action("changed")}
    labelPlacement="left"
    value={true}
  />
));

storiesOf("Switch", module).add("controlled switch", () => (
  <ControlledSwitch />
));