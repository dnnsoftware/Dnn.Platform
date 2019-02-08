import React, { Component } from "react";
import PropTypes from "prop-types";
import "./style.less";
import Tooltip from "../Tooltip";

class TextOverflowWrapper extends Component {
    constructor() {
        super();
        this.uniqueId = "overflowTooltip-" + (Date.now() * Math.random());
        this.state = {
            itemWidth: 0
        };
        this.overflowTooltipRef = React.createRef();
    }
    getStyle() {
        const {props} = this;
        return Object.assign({ maxWidth: props.maxWidth }, props.style);
    }

    componentDidMount() {
        //Set time out to ensure calculation happens after render
        this.timerId = setTimeout(() => {
            let input = this.overflowTooltipRef;
            if (typeof input !== "undefined" && input.current.getBoundingClientRect()) {
                let inputRect = input.current.getBoundingClientRect();
                this.setState({
                    itemWidth: inputRect.width
                });
            } else {
                this.setState({
                    itemWidth: this.props.maxWidth
                });
            }
        }, 500);
    }

    componentWillUnmount() {
        clearTimeout(this.timerId);
    }

    render() {
        const {props, state} = this;
        return (
            <div
                className={"dnn-text-overflow-wrapper" + (props.className ? " " + props.className : "")}
                style={this.getStyle()}
                ref={this.overflowTooltipRef}
                data-tip
                data-for={this.uniqueId}
                title={props.doNotUseTitleAttribute ? "" : props.text}>
                {!props.isAnchor && props.text}
                {props.isAnchor && <a href={props.href} target={props.target}>{props.text}</a>}
                {props.doNotUseTitleAttribute && state.itemWidth >= props.maxWidth && <Tooltip
                    key={this.uniqueId}
                    tooltipPlace={props.place}
                    type={props.type}
                    className={"overflow-tooltip" + (props.tooltipClassName ? " " + props.tooltipClassName : "")}
                    messages={[props.text]}
                    style={props.style}
                    tooltipStyle={props.tooltipStyle}
                />
                }
            </div>
        );
    }
}

TextOverflowWrapper.propTypes = {
    text: PropTypes.string,
    maxWidth: PropTypes.number,
    style: PropTypes.object,
    effect: PropTypes.string,
    type: PropTypes.string,
    place: PropTypes.string,
    tooltipClassName: PropTypes.string,
    multiline: PropTypes.bool,
    doNotUseTitleAttribute: PropTypes.bool,
    isAnchor: PropTypes.bool,
    href: PropTypes.string,
    target: PropTypes.string
};

TextOverflowWrapper.defaultProps = {
    maxWidth: 200,
    effect: "solid",
    place: "top",
    type: "info",
    multiline: true
};

export default TextOverflowWrapper;