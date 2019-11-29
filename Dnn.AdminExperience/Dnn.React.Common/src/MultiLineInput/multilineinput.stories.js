import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import MultiLineInput from "./index";

storiesOf("MultiLineInput", module).add("with content", () => (
    <MultiLineInput
        inputId={"create-term-description"}
        placeholder="Long descritpion here"
        onChange={action("changed")}
    />
));
