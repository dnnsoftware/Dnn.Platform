import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import BackToLink from "../BackToLink";
import Button from "../Button";
import Checkbox from "../Checkbox";

storiesOf("BackToLink", module)
    .add("with text", () => <BackToLink onClick={action("clicked")}>Hello BackToLink</BackToLink>); 

storiesOf("Button", module)
    .add("with text", () => <Button onClick={action("clicked")}>Hello Button</Button>); 
    
storiesOf("Checkbox", module)
    .add("with text", () => <Checkbox onClick={action("clicked")} label="Hello Checkbox"></Checkbox>); 