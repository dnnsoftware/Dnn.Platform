import React from "react";
import { action } from "storybook/actions";
import BackToLink from "./index";

export default {
    component: BackToLink,
};

export const WithText = () => (
    <BackToLink onClick={action("clicked")} text="Hello BackToLink" />
);
