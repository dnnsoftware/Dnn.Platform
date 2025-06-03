import React from "react";
import TextOverflowWrapperNew from "./index";

export default {
    component: TextOverflowWrapperNew,
};

export const WithContent = () => (
    <div>
        Hover Mouse Here
        <TextOverflowWrapperNew
            text="Text Overflow Wrapper New Content"
            maxWidth={200}
            effect="solid"
            place="top"
            type="dark"
            multiline={true}
            delayHide={250}
        />
    </div>
);
