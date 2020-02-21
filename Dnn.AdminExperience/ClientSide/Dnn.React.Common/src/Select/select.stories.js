import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import Select from "./index";

storiesOf("Select", module).add("with content", () => <Select onChange={action("OnSelect")} 
    options={[
        { label: "Opt 1", value: 1 },
        { label: "Opt 2", value: 2 },
        { label: "Opt 3", value: 3 }
    ]} 
    enabled={true} />);
