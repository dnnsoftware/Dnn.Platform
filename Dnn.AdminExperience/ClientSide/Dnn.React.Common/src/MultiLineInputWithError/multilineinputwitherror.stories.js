import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import MultiLineInputWithError from "./index";

storiesOf("MultiLineInputWithError", module).add("with content", () => (
    <MultiLineInputWithError
        inputId={"create-term-description"}
        withLabel={true}
        label="Description"
        value="Long descritpion here"
        onChange={action("changed")}
    />
));
