import React from "react";
import { action } from "@storybook/addon-actions";
import MultiLineInput from "./index";

export const WithContent = () => (
    <MultiLineInput
        inputId={"create-term-description"}
        placeholder="Long descritpion here"
        onChange={action("changed")}
    />
);
