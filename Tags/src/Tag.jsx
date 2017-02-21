import React, { Component, PropTypes } from "react";
import "./style.less";

export default class Tag extends Component {

    render() {
        const {onRemove} = this.props;
        const tagName = this.props.tag.trim();
        if (!tagName) {
            return false;
        }
        return (
            <div className={"dnn-uicommon-tag-input" + (this.props.enabled ? "": " disabled")}>
                <span>{this.props.tag}</span>
                {this.props.enabled && <span className="close" onClick={e => {
                    e.stopPropagation();
                    onRemove();
                }}>×</span>}
            </div>
        );
    }
}

Tag.propTypes = {
    tag: PropTypes.string.isRequired,
    onRemove: PropTypes.func.isRequired,
    enabled: PropTypes.bool.isRequired
};

Tag.defaultProps = {
    enabled: true
};