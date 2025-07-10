import React from "react";
import { action } from "storybook/actions";
import MultiLineInputWithError from "./index";

export default {
    component: MultiLineInputWithError,
};

export const WithContent = () => (
    <MultiLineInputWithError
        inputId={"create-term-description"}
        withLabel={true}
        label="Description"
        value="Long descritpion here"
        onChange={action("changed")}
    />
);
