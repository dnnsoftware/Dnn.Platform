import React, { Component } from "react";
import { storiesOf } from "@storybook/react";
import Dropdown from "./index";

class MyDropdown extends Component{
    constructor(){
        super();
        this.state = { option: {} }
    }

    handleSelect(option){
        this.setState( { option: option });
    }

    render(){
        return (
            <Dropdown
                label="Test"
                options={[
                    { label: "Opt 1", value: 1 },
                    { label: "Opt 2", value: 2 },
                    { label: "Opt 3", value: 3 }
                ]}
                value={this.state.option.value}
                onSelect={this.handleSelect.bind(this)}
            />
        );
    }
}

storiesOf("Dropdown", module).add("with content", () => (
    <MyDropdown />
));
