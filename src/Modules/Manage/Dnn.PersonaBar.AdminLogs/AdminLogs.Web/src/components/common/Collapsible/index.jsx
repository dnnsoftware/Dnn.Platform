import React, { Component, PropTypes } from "react";
import ReactDOM from "react-dom";
import Collapse from "react-collapse";
import "./style.less";

export default class Collapsible extends Component {
    constructor() {
        super();
        this.state = { collapsed: true, collapsedClass: true, repainting: false };
        // setInterval(() => {
        //     console.log("Repainting: ", this.state.repainting);
        // }, 500);
        this.handleClick = this.handleClick.bind(this);
    }
    componentDidMount() {
        document.addEventListener("click", this.handleClick);
        this._isMounted = true;
    }

    componentWillUnmount() {
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
    }
    handleClick(event) {
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }

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
    toggle() {
        if (this.state.collapsed) {
            this.uncollapse();
        } else {
            this.collapse();
        }
    }
    render() {
        const {props} = this;
        return (
            <div className={"collapsible-component" + (props.className ? (" " + props.className) : "")}>
                <div className="collapsible-header" onClick={this.toggle.bind(this) }>{props.label}
                    {!props.disabled &&
                        <span
                            className={"collapse-icon " + (this.state.collapsed ? "collapsed" : "") }>
                        </span>
                    }
                </div>
                <Collapse isOpened={!this.state.collapsed} style={{ float: "left" }}>{props.children }</Collapse>
            </div>
        );
    }
}

Collapsible.propTypes = {
    label: PropTypes.node,
    children: PropTypes.node,
    disabled: PropTypes.bool,
    className: PropTypes.string
};