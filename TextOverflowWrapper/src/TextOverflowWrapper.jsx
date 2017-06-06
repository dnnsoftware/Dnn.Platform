import React, { Component, PropTypes } from "react";
import "./style.less";
import ReactTooltip from "react-tooltip";
import ToolTip from "react-portal-tooltip";

const generateID = () => "a" + Math.random().toString(36).substr(2, 10);
class TextOverflowWrapper extends Component {

    constructor() {
        super();
        this.state = {
            isTooltipActive: false,
            id: generateID()
        };
    }

    showTooltip() {
        this.setState({
            isTooltipActive: true
        });
    }

    hideTooltip() {
        this.setState({
            isTooltipActive: false
        });
    }

    render() {
        const { props, state } = this;

        const TooltipStyle = {
            style: {
                wordWrap: "break-word",
                textOverflow: "ellipsis",
                zIndex: 10000,
                maxWidth: "255px",
                padding: "7px 15px",
                pointerEvents: "auto"
            },
            arrowStyle: {
            }
        };

        const hotspotStyles = {
            wordWrap: "break-word",
            textOverflow: "wrap",
            marginTop: "150px",
            height: "20px",
            width: "200px"
        };

        return (
            <div>
                <div
                    style={props.hotspotStyles || hotspotStyles}
                    id={this.state.id}
                    onMouseEnter={this.showTooltip.bind(this)}
                    onMouseLeave={this.hideTooltip.bind(this)} >
                    &nbsp;
                </div>
                <ToolTip
                    tooltipTimeout={10}
                    active={this.state.isTooltipActive}
                    position={props.position || "bottom"}
                    parent={`#${this.state.id}`}
                    style={props.tooltipStyles || TooltipStyle} >
                    <div>
                        {props.text}
                    </div>
                </ToolTip>
            </div>
        );
    }
}

TextOverflowWrapper.propTypes = {
    text: PropTypes.string,
    position: PropTypes.string,
    hotspotStyles: PropTypes.object,
    tooltipStyles: PropTypes.object
};

export default TextOverflowWrapper;

