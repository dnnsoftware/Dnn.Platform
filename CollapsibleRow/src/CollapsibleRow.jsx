import React, { Component, PropTypes } from "react";
import ReactDOM from "react-dom";
import Collapse from "react-collapse";
import Button from "dnn-button";
import "./style.less";

export default class CollapsibleRow extends Component {
    constructor() {
        super();
        this.handleClick = this.handleClick.bind(this);
    }

    componentDidMount() {
        const {props} = this;
        if (props.closeOnBlur) {
            document.addEventListener("click", this.handleClick);
            this._isMounted = true;
        }
    }

    componentWillMount() {
        this.setState({ collapsed: this.props.collapsed });
    }

    componentWillReceiveProps(newProps) {
        if (newProps.collapsed !== this.props.collapsed) {
            this.setState({ collapsed: newProps.collapsed });
        }
    }

    componentWillUnmount() {
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
    }

    handleClick(event) {
        const {props} = this;
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted || !props.closeOnBlur) { return; }

        if (!ReactDOM.findDOMNode(this).contains(event.target)) {
            this.collapse();
        }
    }
    uncollapse() {
        this.setState({
            collapsed: false
        });
    }
    collapse() {
        this.setState({
            collapsed: true
        });
    }
    onCancel() {
        this.collapse();
        if (this.props.onChange) {
            this.props.onChange(true);
        }
    }
    toggle() {
        if (this.props.onChange) {
            this.props.onChange(this.state.collapsed);
        }
        if (this.state.collapsed) {
            this.uncollapse();
        } else {
            this.collapse();
        }
    }
    render() {
        const {props} = this;
        return (
            <div className={"dnn-collapsible-component" + (props.className ? (" " + props.className) : "") }>
                <div className="collapsible-header" onClick={this.toggle.bind(this) }>{props.label}
                    {!props.disabled &&
                        <span
                            className={"collapse-icon " + (this.state.collapsed ? "collapsed" : "") }>
                        </span>
                    }
                </div>
                <Collapse
                    isOpened={!this.state.collapsed}
                    style={Object.assign({ float: "left" }, props.collapseStyle) }
                    keepCollapsedContent={props.keepCollapsedContent}
                    springConfig={props.springConfig}
                    fixedHeight={props.fixedHeight}
                    onRest={props.onRest}
                    onHeightReady={props.onHeightReady}>
                    {props.children }
                    {!props.buttonsAreHidden &&
                        <div className="collapsible-footer">
                            <Button type="secondary" onClick={this.onCancel.bind(this) }>{props.secondaryButtonText || "Cancel"}</Button>
                            {props.extraFooterButtons}
                        </div>
                    }
                </Collapse>
            </div>
        );
    }
}

CollapsibleRow.propTypes = {
    label: PropTypes.node,
    collapseStyle: PropTypes.object,
    keepCollapsedContent: PropTypes.bool,
    springConfig: PropTypes.object,
    fixedHeight: PropTypes.number,
    onRest: PropTypes.func,
    onHeightReady: PropTypes.func,
    children: PropTypes.node,
    disabled: PropTypes.bool,
    className: PropTypes.string,
    closeOnBlur: PropTypes.bool,
    secondaryButtonText: PropTypes.string,
    extraFooterButtons: PropTypes.node,
    buttonsAreHidden: PropTypes.bool,
    collapsed: PropTypes.bool,
    onChange: PropTypes.func
};

CollapsibleRow.defaultProps = {
    collapsed: true
};

