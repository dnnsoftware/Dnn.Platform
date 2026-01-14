import React from "react";
import { action } from "storybook/actions";
import DnnTabs from "./index";
import "react-tabs/style/react-tabs.less";

export default {
    component: DnnTabs,
};

export const WithContent = () => <DnnTabs tabHeaders={["Tab 1", "Tab 2", "Tab 3"]} className="dnn-tabs" onSelect={action("selected")}>
    <div>Tab 1</div>
    <div>Tab 2</div>
    <div>Tab 3</div>
</DnnTabs>;