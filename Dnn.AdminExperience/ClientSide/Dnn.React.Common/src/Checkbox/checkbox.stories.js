import React from "react";
import { action } from "storybook/actions";
import Checkbox from "./index";

export default {
    component: Checkbox,
};

export const WithText = () => (
    <Checkbox value={false} onChange={action("changed")} label="Hello Checkbox" />
);

export const AlreadyChecked = () => (
    <Checkbox value={true} onChange={action("changed")} label="I should be pre-checked" />
);

export const WithLabelAndTooltip = () => (
    <Checkbox 
        value={false} 
        onChange={action("changed")} 
        label="I have a tooltip that opens on the right" 
        tooltipMessage="This is the tooltip of the checkbox"
        tooltipPlace="right"  
    />
);

// -------- CHECKBOX AVAILABLE PROPS -------------

// Checkbox.propTypes = {
//   value: PropTypes.bool.isRequired,
//   labelPlace: PropTypes.oneOf(["left", "right"]),
//   size: PropTypes.number,
//   checkBoxStyle: PropTypes.object,
//   label: PropTypes.string,
//   onChange: PropTypes.func,
//   enabled: PropTypes.bool,
//   tooltipMessage: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
//   tooltipPlace: PropTypes.string
// };

// Checkbox.defaultProps = {
//   enabled: true,
//   size: 13,
//   labelPlace: "right"
// };