import React, { Component, PropTypes } from "react";
import Tooltip from "dnn-tooltip";
import "./style.less";

class Switch extends Component {

    constructor(props) {
        super(props);

        this.state = {
            switchActive: props.value,
            innerStateSet: false
        };
    }

    componentWillReceiveProps(props) {
        this.setState({
            switchActive: props.value
        });
    }

    toggleStatus() {
        const { props, state } = this;

        if (props.readOnly) {
            return;
        }

        if (typeof this.props.onChange === "function") {
            props.onChange(!state.switchActive);
        }
    }

    getClassName() {
        const { props } = this;

        let className = "dnn-switch";
        if (props.value) {
            className += " dnn-switch-active";
        }

        if (props.readOnly) {
            className += " dnn-switch-readonly";
        }
        if (!props.labelHidden) {
            className += (" place-" + props.labelPlacement);
        }

        return className;
    }

    renderComponent() {
        const { props, state } = this;
        if (props.labelHidden) {
            return <span className={this.getClassName()} onClick={this.toggleStatus.bind(this)}>
                <span className="mark" />
            </span>;
        }
        else {
            return <div className="switch-button">
                <label className={"on-off-text place-" + props.labelPlacement}>
                    {(state.switchActive ? props.onText : props.offText)}
                </label>
                <span className={this.getClassName()} onClick={this.toggleStatus.bind(this)}>
                    <span className="mark" />
                </span>
            </div>;
        }
    }

    render() {
        const { props } = this;
        const tooltipMessages = props.tooltipMessage instanceof Array ? props.tooltipMessage : [props.tooltipMessage];
        return (
            <div className="dnn-switch-container">
                {props.label && <span className="switch-label">{props.label}</span>}
                <Tooltip
                    messages={tooltipMessages}
                    type="info"
                    className={props.placement}
                    tooltipPlace={props.tooltipPlace}
                    rendered={props.tooltipMessage} />
                {this.renderComponent()}
            </div>
        );
    }
}

Switch.propTypes = {
    value: PropTypes.bool,
    labelHidden: PropTypes.bool,
    onText: PropTypes.string,
    offText: PropTypes.string,
    label: PropTypes.string,
    onChange: PropTypes.func,
    readOnly: PropTypes.bool,
    labelPlacement: PropTypes.oneOf(["left", "right"]),
    tooltipMessage: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
    placement: PropTypes.string,
    tooltipPlace: PropTypes.string
};

Switch.defaultProps = {
    onText: "On",
    offText: "Off",
    labelPlacement: "left"
};

export default Switch;