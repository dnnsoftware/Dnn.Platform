import React, {Component } from "react";
import PropTypes from "prop-types";
import EditableWithLabel from "dnn-editable-field";
import LocalizedResources from "../../../resources";
import util from "utils";
import styles from "./style.less";


class LeftPane extends Component {
    constructor() {
        super();
    }
    onEnter(key, value) {
        const {props} = this;
        props.onEnter(key, value, props.index);
    }

    canEdit() {
        const {props} = this;
        return util.isHost() || (props.scopeType === "Portal" && util.canEdit());
    }

    render() {
        const {props} = this;
        return (
            <div className={styles.vocabulariesLeftPane}>
                <EditableWithLabel
                    label={LocalizedResources.get("Description") }
                    value={props.description}
                    onEnter={this.onEnter.bind(this, "Description") }
                    inputType="textArea"
                    editable={this.canEdit()}
                    helpText={LocalizedResources.get("EditFieldHelper")}
                />
                <EditableWithLabel
                    label={LocalizedResources.get("Type") }
                    value={LocalizedResources.get(props.type)}
                    onEnter={this.onEnter.bind(this, "Type") }
                    editable={false}
                />
                <EditableWithLabel
                    label={LocalizedResources.get("Scope") }
                    value={LocalizedResources.get(props.scopeType)}
                    onEnter={this.onEnter.bind(this, "ScopeType") }
                    editable={false}
                />
            </div>
        );
    }
}

LeftPane.propTypes = {
    description: PropTypes.string,
    type: PropTypes.string,
    scope: PropTypes.string,
    onEnter: PropTypes.func,
    index: PropTypes.number
};

export default LeftPane;