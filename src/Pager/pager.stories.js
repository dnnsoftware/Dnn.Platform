import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import Pager from "./index";

storiesOf("Pager", module).add("with content", () => (
    <Pager
        totalRecords={100}
        onPageChanged={action("changed")}
    />
));

storiesOf("Pager", module).add("with 5 numeric counters", () => (
    <Pager
        totalRecords={100}
        onPageChanged={action("changed")}
        numericCounters={5}
    />
));