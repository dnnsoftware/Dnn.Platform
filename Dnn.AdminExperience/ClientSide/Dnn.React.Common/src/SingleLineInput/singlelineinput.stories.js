import React from "react";
import { action } from "@storybook/addon-actions";
import SingleLineInput from "./index";

export const WithContent = () => (
    <SingleLineInput
        inputId={"create-term-name"}
        placeholder="Term Value"
        onChange={action("changed")}
    />
);
