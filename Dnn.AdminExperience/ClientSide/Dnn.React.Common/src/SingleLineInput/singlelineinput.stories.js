import React from "react";
import { action } from "storybook/actions";
import SingleLineInput from "./index";

export default {
    component: SingleLineInput,
};

export const WithContent = () => (
    <SingleLineInput
        inputId={"create-term-name"}
        placeholder="Term Value"
        onChange={action("changed")}
    />
);
