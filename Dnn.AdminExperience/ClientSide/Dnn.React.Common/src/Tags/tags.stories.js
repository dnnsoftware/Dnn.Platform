import React from "react";
import { action } from "storybook/actions";
import Tags from "./index";

export default {
    component: Tags,
};

export const WithContent = () => (
    <Tags
        tags={["Tag 1", "Tag 2", "Tag 3"]}
        onUpdateTags={action("Tags Update")}
    />
);
