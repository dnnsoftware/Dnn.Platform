import React, { PropTypes } from "react";
import ReactTooltip from "react-tooltip";
import uniqueId from "lodash/uniqueId";
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
        case "error": return ErrorIcon;
        case "global": return GlobalIcon;
    }
}

const Tooltip = ({messages, type, rendered, tooltipPlace, style, className, delayHide, customIcon, tooltipClass, onClick}) => {
    const id = uniqueId("tooltip-");
    const containerClass = "dnn-ui-common-tooltip " + type + " " + (className ? className : "");
    const message = getTooltipText(messages);
    const TooltipIcon = !customIcon ? getIconComponent(type) : CustomIcon;

    if (!message || rendered === false) {
        return <noscript />;
    }

    return (
        <div className={containerClass} style={style}>
            <div className="icon" data-tip data-for={id} onClick={onClick}>
                <TooltipIcon icon={customIcon ? customIcon : null} />
            </div>
            <ReactTooltip
                id={id}
                effect="solid"
                place={tooltipPlace}
                type={type}
                class={"tooltip-text" + (tooltipClass ? (" " + tooltipClass) : "")}
                delayHide={delayHide}
                multiline={true}>
                <div dangerouslySetInnerHTML={{ __html: message }}></div>
            </ReactTooltip>
        </div>
    );
};

Tooltip.propTypes = {
    messages: PropTypes.array.isRequired,
    type: PropTypes.oneOf(["error", "info", "global"]).isRequired,
    rendered: PropTypes.bool,
    tooltipPlace: PropTypes.string,
    style: PropTypes.object,
    className: PropTypes.string,
    delayHide: PropTypes.number,
    customIcon: PropTypes.node,
    tooltipClass: PropTypes.string,
    onClick: PropTypes.func
};

Tooltip.defaultProps = {
    tooltipPlace: "top",
    type: "info",
    delayHide: 100
};

export default Tooltip;
