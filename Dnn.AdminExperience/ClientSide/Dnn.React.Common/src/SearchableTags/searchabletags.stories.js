import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import SearchableTags from "./index";
import util from "../../.storybook/utils";

storiesOf("SearchableTags", module).add("with content", () => <SearchableTags
    utils={util}
    tags={[{ id: "Tag1", name: "Tag 1" }, { id: "Tag2", name: "Tag 2" }]}
    onUpdateTags={action("update")}
    error={false}
    errorMessage={"Error Message"}
    enabled={false}
/>);