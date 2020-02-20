import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import SingleLineInput from "./index";

storiesOf("SingleLineInput", module).add("with content", () => (
    <SingleLineInput
        inputId={"create-term-name"}
        placeholder="Term Value"
        onChange={action("changed")}
    />
));
