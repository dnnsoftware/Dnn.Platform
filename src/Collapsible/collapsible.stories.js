import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import Collapsible from "./index";

storiesOf("Collapsible", module).add("with text", () => (
  <Collapsible isOpened={true} autoScroll={true} scrollDelay={10}>
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
));
