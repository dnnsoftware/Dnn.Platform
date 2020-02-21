import React, { Component } from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import Collapsible from "./index";
import InputGroup from "../InputGroup";
import SingleLineInputWithError from "../SingleLineInputWithError";
import MultiLineInputWithError from "../MultiLineInputWithError";
import Button from "../Button";

class MyCollapsible extends Component {
  constructor(props) {
    super(props);
    this.state = { opened: true };
  }

  render (){
    return (
      <div>
        <Collapsible isOpened={this.state.opened} autoScroll={true} scrollDelay={10}>
          <div>
            <p className="add-term-title">Add Term</p>
            <InputGroup>
              <SingleLineInputWithError
                inputId={"create-term-name"}
                withLabel={true}
                label="Required Term *"
                value="Term Value"
                onChange={action("changed")}
                errorMessage="Error"
              />
            </InputGroup>
            <InputGroup>
              <MultiLineInputWithError
                inputId={"create-term-description"}
                withLabel={true}
                label="Description"
                value="Long descritpion here"
                onChange={action("changed")}
              />
            </InputGroup>
          </div>
        </Collapsible>
        <Button onClick={() => this.setState({opened: true})}>Open</Button>
        <Button onClick={() => this.setState({opened: false})}>Close</Button>
      </div>
    );
  }
}

storiesOf("Collapsible", module).add("with text", () => (
  <MyCollapsible />
));
