import React from "react";
import { action } from "storybook/actions";
import MultiLineInput from "./index";

export default {
    component: MultiLineInput,
};

export const WithContent = () => (
    <MultiLineInput
        inputId={"create-term-description"}
        placeholder="Long descritpion here"
        onChange={action("changed")}
    />
);
