import React from "react";
import "./style.less";

const getClassName = function (className, isOpen) {
    return "dnn-persona-bar-page" + (isOpen ? " show " : " ") + className;
};

const PersonaBarPage = ({className, isOpen, children}) => (
    <div className={getClassName(className, isOpen)}>
        {children}
    </div> 
);

PersonaBarPage.propTypes = {
    isOpen: React.PropTypes.bool.isRequired, 
    className: React.PropTypes.string,
    children: React.PropTypes.node
};

export default PersonaBarPage;