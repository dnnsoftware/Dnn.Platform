import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import { Collapsible as Collapse, Checkbox } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import {
    log as LogActions
} from "../../../actions";

/*eslint-disable eqeqeq*/
class LogItemRow extends Component {
    constructor() {
        super();
        this.state = {
            collapsed: true,
            collapsedClass: true,
            repainting: false
        };
        this.timeout = 0;
        this.handleClick = this.handleClick.bind(this);
    }

    componentWillReceiveProps() {
        this.setState({});
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

        if (!this.node.contains(event.target) && (typeof event.target.className == "string" && event.target.className.indexOf("do-not-close") == -1)) {

            this.timeout = 475;
            this.collapse();
        } else {

            this.timeout = 0;
        }
    }

    uncollapse() {
        setTimeout(() => {
            this.setState({
                collapsed: false
            });
        }, this.timeout);
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

    onSelectRow(rowId) {
        const {props} = this;
        if (this.isRowSelected(rowId)) {
            props.dispatch(LogActions.deselectRow(rowId));
        }
        else {
            props.dispatch(LogActions.selectRow(rowId));
        }
    }

    isRowSelected() {
        const { props } = this;
        return props.selectedRowIds.some((id) => id === props.logId);
    }

    render() {
        const {props, state} = this;
        return (
            <div ref={node => this.node = node} className={"collapsible-logitemdetail " + state.collapsed + (props.className ? (" " + props.className) : "")}>
                <div className={"collapsible-logitemdetail-header " + state.collapsed}>
                    <div className="term-header">
                        <div data-index="0" className="term-label-checkbox">
                            <div className="term-label-wrapper">
                                <Checkbox value={this.isRowSelected()} onChange={this.onSelectRow.bind(this, this.props.logId)} />
                            </div>
                        </div>
                        <div className="term-label-cssclass" onClick={this.toggle.bind(this)}>
                            <div className="logItemIndicator">
                                <span className={this.props.cssClass}></span>
                            </div>
                        </div>
                        <div className="term-label-createdate" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span>{this.props.createDate}</span>
                            </div>
                        </div>
                        <div className="term-label-typename" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span>{this.props.typeName}</span>
                            </div>
                        </div>
                        <div className="term-label-username" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span>{this.props.userName}&nbsp; </span>
                            </div>
                        </div>
                        <div className="term-label-portalname" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span>{this.props.portalName}&nbsp; </span>
                            </div>
                        </div>
                        <div className="term-label-summary" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span>{this.props.summary}&nbsp; </span>
                            </div>
                        </div>
                    </div>
                </div>
                <Collapse className="logitem-collapsible" isOpened={!this.state.collapsed} style={{ float: "left", width: "100%" }}>{!state.collapsed && props.children}</Collapse>
            </div>
        );
    }
}

LogItemRow.propTypes = {
    allRowIds: PropTypes.array.isRequired,
    selectedRowIds: PropTypes.array.isRequired,
    excludedRowIds: PropTypes.array.isRequired,
    cssClass: PropTypes.string,
    logId: PropTypes.string,
    typeName: PropTypes.string,
    createDate: PropTypes.string,
    userName: PropTypes.string,
    portalName: PropTypes.string,
    summary: PropTypes.string,
    label: PropTypes.node,
    children: PropTypes.node,
    disabled: PropTypes.bool,
    className: PropTypes.string
};

function mapStateToProps(state) {
    return {
        selectedRowIds: state.log.selectedRowIds,
        excludedRowIds: state.log.excludedRowIds
    };
}

export default connect(mapStateToProps)(LogItemRow);
