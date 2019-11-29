import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import Checkbox from "./index";

storiesOf("Checkbox", module).add("with text", () => (
  <Checkbox value={false} onChange={action("changed")} label="Hello Checkbox" />
));

storiesOf("Checkbox", module).add("already checked", () => (
  <Checkbox value={true} onChange={action("changed")} label="I should be pre-checked" />
));

storiesOf("Checkbox", module).add("with label and tooltip", () => (
  <Checkbox 
    value={false} 
    onChange={action("changed")} 
    label="I have a tooltip that opens on the bottom" 
    tooltipMessage="This is the tooltip of the checkbox"
    tooltipPlace="bottom"  
  />
))



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