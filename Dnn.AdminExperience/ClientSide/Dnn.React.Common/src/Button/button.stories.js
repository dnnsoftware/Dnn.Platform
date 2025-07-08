import React from "react";
import { action } from "storybook/actions";
import Button from "../Button";

export default {
    component: Button,
};

export const WithText = () => (
    <Button onClick={action("clicked")}>Hello Button</Button>
);
