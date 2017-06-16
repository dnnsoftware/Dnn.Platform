import React, { Component, PropTypes } from "react";
import ReactTooltip from "react-tooltip";

import "./style.less";

const generateID = () => "a" + Math.random().toString(36).substr(2, 10);
class TextOverflowWrapperNew extends Component {

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

        const hotspotStyles = {
            marginLeft:"20px",
            backgroundColor:"transparent",
            position:"absolute",
            top:0,
            left:0,
            height: "20px",
            width: "200px",
            zIndex: 10000
        };

        return (
            <div>
                <div
                    data-tip data-for={state.id}
                    style={props.hotspotStyles || hotspotStyles}
                    id={state.id} >
                </div>
                <ReactTooltip
                    id={state.id}
                    type={props.type || "light"}
                    place={props.place || "bottom"}
                    effect={props.effect || "float"}
                    offset={props.offset}
                    multiline={props.multiline || true}
                    className={props.className || "page-picker-tooltip-style"}
                    border={props.bool || false}
                    >
                    <div>
                        {props.text}
                    </div>
                </ReactTooltip>
            </div>
        );
    }
}

TextOverflowWrapperNew.propTypes = {
    text: PropTypes.string,
    hotspotStyles: PropTypes.hotspotStyles,
    type: PropTypes.string.isRequired,
    place: PropTypes.string.isRequired,
    effect: PropTypes.string,
    offset: PropTypes.offset,
    multiline: PropTypes.bool,
    className: PropTypes.string,
    border: PropTypes.bool
};

export default TextOverflowWrapperNew;

