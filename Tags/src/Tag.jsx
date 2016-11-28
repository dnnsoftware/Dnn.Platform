import React, { Component, PropTypes } from "react";
import "./style.less";

export default class Tag extends Component {

    render() {
        const tagName = this.props.tag.trim();
        if (!tagName) {
            return false;
        }
        return (
            <div className="dnn-uicommon-tag-input">
                <span>{this.props.tag}</span>
                <span className="close" onClick={this.props.onRemove }>×</span>
            </div>
        );
    }
}

Tag.propTypes = {
    tag: PropTypes.string.isRequired,
    onRemove: PropTypes.func.isRequired
};