import React from "react";
import { action } from "storybook/actions";
import Flag from "./index";

export default {
    component: Flag,
};

export const WithContent = () => (
    <Flag title="Test" culture="en-US" onClick={action("Clicked")} />
);
