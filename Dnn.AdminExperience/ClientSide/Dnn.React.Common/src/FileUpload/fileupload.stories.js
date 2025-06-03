import React from "react";
import { action } from "storybook/actions";
import FileUpload from "./index";
import util from "../../.storybook/utils";

export default {
    component: FileUpload,
};

export const WithContent = () => <FileUpload utils={util} onSelectFile={action("Select File")} />;