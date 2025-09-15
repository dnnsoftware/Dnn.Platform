import React from "react";
import { action } from "storybook/actions";
import SearchableTags from "./index";
import util from "../../.storybook/utils";

export default {
    component: SearchableTags,
};

export const WithContent = () => (
    <SearchableTags
        utils={util}
        tags={[{ id: "Tag1", name: "Tag 1" }, { id: "Tag2", name: "Tag 2" }]}
        onUpdateTags={action("update")}
        error={false}
        errorMessage={"Error Message"}
        enabled={false}
    />
);
