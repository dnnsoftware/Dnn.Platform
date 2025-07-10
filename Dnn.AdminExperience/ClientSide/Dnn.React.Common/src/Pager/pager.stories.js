import React from "react";
import { action } from "storybook/actions";
import Pager from "./index";

export default {
    component: Pager,
};

export const WithContent = () => (
    <Pager
        totalRecords={100}
        onPageChanged={action("changed")}
    />
);

export const WithFiveNumericCounters = () => (
    <Pager
        totalRecords={100}
        onPageChanged={action("changed")}
        numericCounters={5}
    />
);