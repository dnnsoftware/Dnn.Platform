import React from "react";
import TextOverflowWrapper from "./index";

export default {
    component: TextOverflowWrapper,
};

export const WithContent = () => (
    <div style={{ paddingTop: 100, paddingLeft: 100 }}>        
        <TextOverflowWrapper text="Default Content & tooltip" />
    </div>
);

export const WithTooltip = () => (            
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
);

export const WithHyperlink = () => (
    <div style={{ paddingTop: 100, paddingLeft: 100 }}>
        <TextOverflowWrapper
            text="hyperlink test"
            href="http://www.google.com"
            isAnchor={true}
            type="info"            
        />
    </div>
);
