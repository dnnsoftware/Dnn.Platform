import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import { Collapsible as Collapse } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import * as dayjs from "dayjs";

/*eslint-disable eqeqeq*/
class LogFileRow extends Component {
    constructor() {
        super();
        this.state = {
            collapsed: true,
            collapsedClass: true,
            repainting: false,
        };
        this.timeout = 0;
        this.handleClick = this.handleClick.bind(this);

        const localizedFormat = require('dayjs/plugin/localizedFormat');
        dayjs.extend(localizedFormat);
        require('dayjs/locale/' + window.dnn.utility.getCulture().substring(0,2));
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
        if (!this._isMounted) {
            return;
        }

        if (
            !this.node.contains(event.target) &&
      typeof event.target.className == "string" &&
      event.target.className.indexOf("do-not-close") == -1
        ) {
            this.timeout = 475;
            this.collapse();
        } else {
            this.timeout = 0;
        }
    }

    uncollapse() {
        setTimeout(() => {
            this.setState({
                collapsed: false,
            });
        }, this.timeout);
    }

    collapse() {
        this.setState({
            collapsed: true,
        });
    }

    toggle() {
        if (this.state.collapsed) {
            this.uncollapse();
            this.props.onOpen();
        } else {
            this.collapse();
        }
    }

    render() {
        const { props, state } = this;
        return (
            <div
                ref={(node) => (this.node = node)}
                className={"collapsible-logitemdetail " + state.collapsed}
            >
                <div
                    className={"collapsible-logitemdetail-header " + state.collapsed}
                    onClick={this.toggle.bind(this)}
                >
                    <div className="term-header">
                        <div className="term-label-typename">
                            <div className="term-label-wrapper">
                                <span>{this.props.typeName}</span>
                            </div>
                        </div>
                        <div className="term-label-filename">
                            <div className="term-label-wrapper">
                                <span>{this.props.fileName}</span>
                            </div>
                        </div>
                        <div className="term-label-modifieddate">
                            <div className="term-label-wrapper">
                                <span>{dayjs(this.props.lastWriteTimeUtc).locale(window.dnn.utility.getCulture().substring(0,2)).format("LLL")}</span>
                            </div>
                        </div>
                        <div className="term-label-size">
                            <div className="term-label-wrapper">
                                <span>{this.props.size}</span>
                            </div>
                        </div>
                    </div>
                </div>
                <Collapse
                    className="logitem-collapsible"
                    isOpened={!this.state.collapsed}>
                    {!state.collapsed && props.children}
                </Collapse>
            </div>
        );
    }
}

LogFileRow.propTypes = {
    fileName: PropTypes.string,
    typeName: PropTypes.string,
    lastWriteTimeUtc: PropTypes.string,
    size: PropTypes.string,
    children: PropTypes.node,
    onOpen: PropTypes.func,
};

export default connect()(LogFileRow);
