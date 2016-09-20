import React, { Component, PropTypes } from "react";
import "./style.less";

export default class Tag extends Component {
    
    removeTag() {
        this.props.onRemove(this.props.tag);
    }
    
    render() {
        return (
            <div className="dnn-uicommon-tag-input">
                <span>{this.props.tag}</span>
                <span className="close" onClick={this.removeTag.bind(this)}>×</span>
            </div>
        );
    }
}

Tag.propTypes = {
    tag: PropTypes.string.isRequired,
    onRemove: PropTypes.func.isRequired
};