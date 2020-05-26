import React, { Component } from "react";
import PropTypes from "prop-types";
import { Tooltip as AccessibleTooltip } from "react-accessible-tooltip";
import InfoIcon from "./InfoIcon";
import ErrorIcon from "./ErrorIcon";
import GlobalIcon from "./GlobalIcon";
import CustomIcon from "./CustomIcon";
import "./style.less";

const getTooltipText = function (messages) {
    if (!messages || !messages.length) {
        return "";
    }
    else if (messages.length === 1) {
        return messages[0];
    }

    return "- " + messages.join("<br />- ");
};

/*eslint-disable react/no-danger*/
function getIconComponent(type) {
    switch (type) {
        case "info": return InfoIcon;
        case "error": case "warning": return ErrorIcon;
        case "global": return GlobalIcon;
    }
}

class Tooltip extends Component {

    constructor() {
        super();
    }    

    render() {
        const {messages, rendered, type, className, style} = this.props;
        const containerClass = "dnn-ui-common-tooltip " + type + " " + (className ? className : "");
        const message = getTooltipText(messages);
        

        if (!message || rendered === false) {
            return <noscript />;
        }
        return (
            <div className={containerClass} style={style}>
                <AccessibleTooltip 
                    label={props => {  
                        const {customIcon, type, onClick} = this.props;
                        const TooltipIcon = !customIcon ? getIconComponent(type) : CustomIcon;
                        return (
                            <div className="icon" onClick={onClick} {...props.labelAttributes} >
                                <TooltipIcon icon={customIcon ? customIcon : null} />
                            </div>
                        );
                    }}
                    overlay={props => {
                        const {tooltipPlace, maxWidth} = this.props;
                        
                        const classNames = [];
                        classNames.push("tooltip-overlay");
                        if (props.isHidden) {
                            classNames.push("tooltip-overlay--hidden");
                        }
                        classNames.push(tooltipPlace);
                        return (
                            <div
                                {...props.overlayAttributes}
                                className={classNames.join(" ")}
                            >
                                <div className="tooltip-inner" style={{maxWidth: maxWidth}} dangerouslySetInnerHTML={{__html: message}} />
                            </div>
                        );
                    }}
                />
            </div>
        );
    }
}

Tooltip.propTypes = {
    messages: PropTypes.array.isRequired,
    type: PropTypes.oneOf(["error", "warning", "info", "global"]).isRequired,
    rendered: PropTypes.bool,
    tooltipPlace: PropTypes.oneOf(["top", "bottom"]).isRequired,
    style: PropTypes.object,
    tooltipStyle: PropTypes.object,
    tooltipColor: PropTypes.string,
    className: PropTypes.string,
    customIcon: PropTypes.node,
    tooltipClass: PropTypes.string,
    onClick: PropTypes.func,
    maxWidth: PropTypes.number
};

Tooltip.defaultProps = {
    tooltipPlace: "top",
    type: "info",
    maxWidth: 400
};

export default Tooltip;
