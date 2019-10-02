import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import TextOverflowWrapperNew from "./index";

storiesOf("TextOverflowWrapperNew", module).add("with content", () => (<div>Hover Mouse Here
    <TextOverflowWrapperNew text="Text Overflow Wrapper New Content" maxWidth={200}
        effect="solid"
        place="top"
        type="dark"
        multiline={true}
        delayHide={250}></TextOverflowWrapperNew> </div>
));
