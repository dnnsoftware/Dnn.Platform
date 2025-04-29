import React from "react";
import PropTypes from "prop-types";

const CustomIcon = (props) => {
  let Icon = props.icon;
  return (
    <div>
      <Icon />
    </div>
  );
};
CustomIcon.propTypes = {
  icon: PropTypes.string,
};
export default CustomIcon;
