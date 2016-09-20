import React, {PropTypes} from "react";
import "./style.less";

const Button = ({children, className, size, type, onClick, style, disabled}) => { 
    return (
        <button className={"dnn-ui-common-button" + (className ? (" " + className) : "") + (" " + size)} style={style} 
            role={type} onClick={onClick} disabled={disabled}>
            {children}
        </button> 
    );
};

Button.propTypes = {
    children: PropTypes.node.isRequired,
    className: PropTypes.string,
    size: PropTypes.oneOf(["small", "large"]),
    type: PropTypes.oneOf(["primary", "secondary"]).isRequired,
    onClick: PropTypes.func,
    disabled: PropTypes.bool,
    style: PropTypes.object
};

Button.defaultProps = {
    type: "secondary",
    size: "small",
    className: ""
};

export default Button;