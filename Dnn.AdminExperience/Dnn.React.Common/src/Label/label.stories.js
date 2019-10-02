import React from "react";
import { storiesOf } from "@storybook/react";
import Label from "./index";

storiesOf("Label", module).add("simple label", () => 
    <Label label="Test" />
);

storiesOf("Label", module).add("with short tooltip", () =>
    <div style={{paddingTop:100,paddingLeft:100}}>
        <Label 
            label="Some field label:" 
            tooltipMessage="This is a required field"
            tooltipPlace="top"        
        />
    </div>
);

storiesOf("Label", module).add("with short tooltip on right", () =>
    <div style={{paddingTop:100,paddingLeft:100}}>
        <Label 
            label="Some field label:"   
            labelType="inline"          
            tooltipMessage="This is a required field"
            tooltipPlace="top"     
            tooltipStyle={{float:"right"}}
        />
    </div>
);

storiesOf("Label", module).add("with short tooltip on bottom-right", () =>
    <div style={{paddingTop:100,paddingLeft:100}}>
        <Label 
            label="Some field label:"   
            labelType="inline"          
            tooltipMessage="This is a required field"
            tooltipPlace="bottom"     
            tooltipStyle={{float:"right"}}
        />
    </div>
);

storiesOf("Label", module).add("Reproduces issue", () =>    
    <div style={{paddingTop:100,paddingLeft:100}}>
        <p>This case reproduces the issue identified in <br />
            <a href="https://github.com/dnnsoftware/Dnn.AdminExperience/pull/320">https://github.com/dnnsoftware/Dnn.AdminExperience/pull/320</a><br /> and <br />
            <a href="https://github.com/romainberger/react-portal-tooltip/issues/84">https://github.com/romainberger/react-portal-tooltip/issues/84</a>
        </p>
        <Label 
            label="Some field label:" 
            labelType="inline"
            tooltipMessage={["This is a required field", "it should show as a list", "and be on the bottom right"]}
            tooltipPlace="top"
        />
    </div>
);





// ---------------- Label available props -------------------
//
// Label.propTypes = {
//     label: PropTypes.string,
//     className: PropTypes.string,
//     labelFor: PropTypes.string,
//     tooltipMessage: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
//     tooltipPlace: PropTypes.string,
//     tooltipStyle: PropTypes.object,
//     tooltipColor: PropTypes.string,
//     labelType: PropTypes.oneOf(["inline", "block"]),
//     style: PropTypes.object,
//     extra: PropTypes.node,
//     onClick: PropTypes.func
// };
// Label.defaultProps = {
//     labelType: "block",
//     className: ""
// };