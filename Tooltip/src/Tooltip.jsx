import React, { Component, PropTypes } from "react";
import ReactPortalTooltip from "react-portal-tooltip";
import uniqueId from "lodash/uniqueId";
import InfoIcon from "./InfoIcon";
import ErrorIcon from "./ErrorIcon";
import GlobalIcon from "./GlobalIcon";
import CustomIcon from "./CustomIcon";
import "./style.less";

const colors = {
    error: "#EA2134",
    info: "#4b4e4f",
    global: "#21A3DA"
};

function getStyle(type, _color) {
    const color = _color || colors[type];
    return {
        style: {
            background: color,
            color: "white",
            padding: "10px 20px",
            transition: "opacity 0.2s ease-in-out, visibility 0.2s ease-in-out",
            boxShadow: "none",
            fontFamily: "'proxima_nova', 'HelveticaNeue', 'Helvetica Neue', Helvetica, Arial, sans-serif"
        },
        arrowStyle: {
            color,
            borderColor: false
        }
    };
}

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

class Tooltip extends Component {

    componentWillMount() {
        const id = uniqueId("tooltip-");
        this.setState({id: id, active: false});
    }

    showTooltip() {
        this.setState({isTooltipActive: true});
    }

    hideTooltip() {
        this.setState({isTooltipActive: false});
    }
    
    render() {
        const {messages, type, rendered, tooltipPlace, style, className, delayHide, customIcon, tooltipClass, onClick, tooltipColor} = this.props;
        const containerClass = "dnn-ui-common-tooltip " + type + " " + (className ? className : "");
        const message = getTooltipText(messages);
        const TooltipIcon = !customIcon ? getIconComponent(type) : CustomIcon;

        if (!message || rendered === false) {
            return <noscript />;
        }
        const tooltipStyle = this.props.tooltipStyle || getStyle(type, tooltipColor);
        return (
            <div className={containerClass} style={style}>
                <div id={this.state.id} className="icon" onClick={onClick}
                    onMouseEnter={this.showTooltip.bind(this)}
                    onMouseLeave={this.hideTooltip.bind(this)}>
                    <TooltipIcon icon={customIcon ? customIcon : null} />
                </div>
                <ReactPortalTooltip
                    style={tooltipStyle}
                    active={this.state.isTooltipActive}
                    position={tooltipPlace}
                    tooltipTimeout={delayHide}
                    arrow="center"
                    parent={"#" + this.state.id}>
                    <div dangerouslySetInnerHTML={{ __html: message }} />
                </ReactPortalTooltip>
            </div>
        );
    }
}

Tooltip.propTypes = {
    messages: PropTypes.array.isRequired,
    type: PropTypes.oneOf(["error", "info", "global"]).isRequired,
    rendered: PropTypes.bool,
    tooltipPlace: PropTypes.oneOf(["top", "bottom"]).isRequired,
    style: PropTypes.object,
    tooltipStyle: PropTypes.object,
    tooltipColor: PropTypes.string,
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
