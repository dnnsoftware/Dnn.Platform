import React, { Component, PropTypes } from "react";
import Tooltip from "dnn-tooltip";
import "./style.less";

const validators = {};

export default class Validator extends Component {

    constructor(props) {
        super(props);

        this.state = {
            validated: true
        };
        this.editorValue = "";
        this.editorChangedEvent = null;
        this.elementsParsed = false;

        let groupName = props.group || "globals";
        let validatorName = props.name;

        if (!validators[groupName]) {
            validators[groupName] = {};
        }

        let groupValidators = validators[groupName];
        groupValidators[validatorName] = this;
    }

    static validate(groupName) {
        let validated = true;
        groupName = groupName || "globals";
        let groupValidators = validators[groupName];
        for (let validator in groupValidators) {
            if (groupValidators.hasOwnProperty(validator)) {
                validated = groupValidators[validator].validate();
                if (!validated) {
                    break;
                }
            }
        }

        return validated;
    }

    validate() {
        const {props} = this;
        let validated = props.required && this.editorValue !== "";
        this.setState({ validated: validated });

        return validated;
    }

    componentDidMount() {
    }

    onEditorChanged(event) {
        this.editorValue = event.target.value;
        this.validate();

        if (typeof this.editorChangedEvent === "function") {
            this.editorChangedEvent(event);
        }
    }

    getClassName() {
        const {props, state} = this;

        let className = "form-validator";
        className += state.validated ? ' validated' : ' not-validated';

        if (props.required) {
            className += " required";
        }

        return className;
    }

    renderValidator() {
        const {props, state} = this;

        return <Tooltip className="validator" messages={[props.errorMessage]} type="error" renderd={true} />;
    }

    parseElements() {
        const {props, state} = this;

        this.children = props.children.map((function (item, index) {
            let cloneProps = Object.assign({}, item.props);
            cloneProps.key = "control" + index;
            if (item.type !== "label") {
                this.editorChangedEvent = cloneProps.onChange;
                cloneProps.onChange = this.onEditorChanged.bind(this);
                let value = (cloneProps.defaultValue || cloneProps.value) || '';
                if (!this.elementsParsed && value !== this.editorValue) {
                    this.editorValue = value;
                    this.elementsParsed = true;
                }
            }
            return React.cloneElement(item, cloneProps);
        }).bind(this));
    }

    render() {
        this.parseElements();

        return <div className={this.getClassName() }>
            {this.children}
            {this.renderValidator() }
        </div>;
    }
}

Validator.propTypes = {
    children: PropTypes.array,
    group: PropTypes.string,
    name: PropTypes.string,
    required: PropTypes.bool,
    validateEmpty: PropTypes.bool,
    errorMessage: PropTypes.string
};