import React from "react";
import PropTypes from "prop-types";
import "./style.less";

const getClassName = function (className, isOpen, fullWidth) {
    return "dnn-persona-bar-page" + (isOpen ? " show " : " ") + (fullWidth ? " full-width " : " ") + className;
};

const PersonaBarPage = ({className, isOpen, children, fullWidth}) => (
    <div className={getClassName(className, isOpen, fullWidth)}>
        {children}
    </div> 
);

PersonaBarPage.propTypes = {
    isOpen: PropTypes.bool.isRequired, 
    className: PropTypes.string,
    children: PropTypes.node,
    fullWidth: PropTypes.bool
};

PersonaBarPage.defaultProps = {
    fullWidth: false
};

export default PersonaBarPage;