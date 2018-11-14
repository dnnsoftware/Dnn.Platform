import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import SearchBox from "./index";

storiesOf("SearchBox", module).add("with content", () => <SearchBox placeholder="Search..." onSearch={action("OnSearch")} maxLength={50} iconStyle={{ right: 0 }} inputDisabled={false} />);