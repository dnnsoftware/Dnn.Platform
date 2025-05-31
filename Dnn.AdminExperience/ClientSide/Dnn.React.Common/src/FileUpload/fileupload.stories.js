import React from "react";
import { action } from "@storybook/addon-actions";
import FileUpload from "./index";
import util from "../../.storybook/utils";

export const WithContent = () => <FileUpload utils={util} onSelectFile={action("Select File")} />;