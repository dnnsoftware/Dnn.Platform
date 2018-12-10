import React, {Component } from "react";
import PropTypes from "prop-types";
import Collapse from "dnn-collapsible";
import "./style.less";

class BulletinItemRow extends Component {
    constructor() {
        super();
        this.state = {
            collapsed: true,
            collapsedClass: true,
            repainting: false
        };
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
        const {props, state} = this;
        return (
            <div className="bulletinItem">
                <div className={"collapsible-bulletinitemdetail-header " + state.collapsed} >
                    <div className="term-header">
                        <div className="term-label-title" onClick={this.toggle.bind(this) }>
                            <div className="term-label-wrapper">
                                <span>{this.props.title}&nbsp; </span>
                            </div>
                        </div>
                        <div className="term-label-date" onClick={this.toggle.bind(this) }>
                            <div className="term-label-wrapper">
                                <span>{this.props.pubDate}&nbsp; </span>
                            </div>
                        </div>                        
                    </div>
                </div>
                <Collapse className={this.props.className} isOpened={!this.state.collapsed}>{!state.collapsed && props.children }</Collapse>
            </div>
        );
    }
}

BulletinItemRow.propTypes = {
    pubDate: PropTypes.string,
    title: PropTypes.string,
    link: PropTypes.string,
    description: PropTypes.string,
    author: PropTypes.string,
    label: PropTypes.node,
    children: PropTypes.node,
    disabled: PropTypes.bool,
    className: PropTypes.string
};

export default BulletinItemRow;
