import React, { PropTypes } from "react";

/* eslint-disable react/no-danger */
const CustomIcon = props => (
    <div dangerouslySetInnerHTML={{ __html: props.icon }} />
);
CustomIcon.propTypes = {
    icon: PropTypes.string
};
export default CustomIcon;