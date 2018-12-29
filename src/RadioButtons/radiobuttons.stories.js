import React, { Component } from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import RadioButtons from "./index";

class MyRadioButtons extends Component{
  constructor(){
    super();
    this.state = { value: 1 };
  }

  render(){
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

storiesOf("RadioButtons", module).add("with content", () => (
  <MyRadioButtons />
));
