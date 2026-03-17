import React from "react";
import { action } from "storybook/actions";
import Flag from "./index";

export default {
    component: Flag,
    render: ({...args}) => <Flag {...args} onClick={action("Clicked")} />
};

// export const WithContent = () => (
//     <Flag title="Test" culture="en-US" onClick={action("Clicked")} />
// );

export const EnUs = {
    args: {
        title: "English (United States)",
        culture: "en-US",
    },
};

export const FrCa = {
    args: {
        title: "French (Canada)",
        culture: "fr-CA",
    },
};

export const Missing = {
    args: {
        title: "Missing Flag",
        culture: "ab-CD",
    },
};