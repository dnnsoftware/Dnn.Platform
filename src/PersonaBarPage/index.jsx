import React from "react";
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
    isOpen: React.PropTypes.bool.isRequired, 
    className: React.PropTypes.string,
    children: React.PropTypes.node,
    fullWidth: React.PropTypes.bool
};

PersonaBarPage.defaultProps = {
    fullWidth: false
};

export default PersonaBarPage;