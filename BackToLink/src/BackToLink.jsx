import React, {PropTypes} from "react";
import { ArrowBack } from "dnn-svg-icons";
import "./style.less";

/*eslint-disable react/no-danger*/
const BackToLink = ({text, children, className, onClick, style, arrowStyle}) => { 
    return (
        <a className={"dnn-back-to-link" + (className ? (" " + className) : "")} style={style} onClick={onClick}>
            <div style={arrowStyle} className="dnn-back-to-arrow" dangerouslySetInnerHTML={{__html: ArrowBack }} />
            <span>{text}</span>
            {children}
        </a> 
    );
};

BackToLink.propTypes = {
    children: PropTypes.node,
    text: PropTypes.node.isRequired,
    className: PropTypes.string,
    onClick: PropTypes.func,
    arrowStyle: PropTypes.object,
    style: PropTypes.object
};

BackToLink.defaultProps = {
    className: ""
};

export default BackToLink;