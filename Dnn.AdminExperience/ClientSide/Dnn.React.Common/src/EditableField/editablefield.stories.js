import React from "react";
import { action } from "storybook/actions";
import EditableField from "./index";

export default {
    component: EditableField,
};

export const WithSingleLineInput =  () => (
    <EditableField
        label="Test"
        value="Content"
        onFocus={action("focus")}
        onEnter={action("enter")}
    />
);

export const WithMultiLineInput =  () => (
    <EditableField
        label="Test"
        value="Content"
        inputType="textArea"
        onFocus={action("focus")}
        onEnter={action("enter")}
    />
);
