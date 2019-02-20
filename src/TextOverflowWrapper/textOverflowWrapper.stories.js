import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import TextOverflowWrapper from "./index";

storiesOf("TextOverflowWrapper", module).add("with content", () => (
    <div style={{ paddingTop: 100, paddingLeft: 100 }}>        
        <TextOverflowWrapper text="Default Content & tooltip" />
    </div>
));

storiesOf("TextOverflowWrapper", module).add("with tooltip", () => (            
    <div style={{ paddingTop: 100, paddingLeft: 100 }}>                
        <TextOverflowWrapper
            text="Tooltip test"            
            doNotUseTitleAttribute={true}
            maxWidth={50}
            type="info"
            place="top"
            style={{ float: "left", overflow: "none" }}
            toolTipStyle={{ display: "inline-block" }}
        />
    </div>
));

storiesOf("TextOverflowWrapper", module).add("with hyperlink", () => (
    <div style={{ paddingTop: 100, paddingLeft: 100 }}>
        <TextOverflowWrapper
            text="hyperlink test"
            href="http://www.google.com"
            isAnchor={true}
            type="info"            
        />
    </div>
));
