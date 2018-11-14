import React from "react";
import { storiesOf } from "@storybook/react";
import Dropdown from "./index";

storiesOf("Dropdown", module).add("with content", () => (
    <Dropdown
        label="Test"
        options={[
            { label: "Opt 1", value: 1 },
            { label: "Opt 2", value: 2 },
            { label: "Opt 3", value: 3 }
        ]}
        selectedIndex={1}
    />
));
