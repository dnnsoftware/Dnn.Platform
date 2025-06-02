import React from "react";
import PropTypes from "prop-types";

 
const CustomIcon = props => (
    <div dangerouslySetInnerHTML={{ __html: props.icon }} />
);
CustomIcon.propTypes = {
    icon: PropTypes.string
};
export default CustomIcon;