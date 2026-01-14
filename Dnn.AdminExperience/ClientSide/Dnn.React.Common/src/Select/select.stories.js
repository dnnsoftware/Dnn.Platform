import React from "react";
import { action } from "storybook/actions";
import Select from "./index";

export default {
    component: Select,
};

export const WithContent = () => (
    <Select onChange={action("OnSelect")} 
        options={[
            { label: "Opt 1", value: 1 },
            { label: "Opt 2", value: 2 },
            { label: "Opt 3", value: 3 }
        ]} 
        enabled={true}
    />
);
