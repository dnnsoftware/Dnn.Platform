import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import TextOverflowWrapper from "./index";

storiesOf("TextOverflowWrapper", module).add("with content", () => (
    <TextOverflowWrapper text="Text Overflow Wrapper Content" />
));
