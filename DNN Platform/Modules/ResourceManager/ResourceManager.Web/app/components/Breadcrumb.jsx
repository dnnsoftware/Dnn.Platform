import React from "react";
import PropTypes from "prop-types";

const Breadcrumb = ({name, onClick}) => (
    <div onClick={onClick}>
        <span>{ name }</span>
    </div>
);

Breadcrumb.propTypes = {
    name: PropTypes.string.isRequired,
    onClick: PropTypes.func
};

export default Breadcrumb;
