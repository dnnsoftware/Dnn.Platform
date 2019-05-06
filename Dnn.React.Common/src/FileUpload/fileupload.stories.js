import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import FileUpload from "./index";
import util from "../../.storybook/utils";

storiesOf("FileUpload", module).add("with content", () => <FileUpload utils={util} onSelectFile={action("Select File")} />);