import React, {Component, PropTypes } from "react";
import Label from "dnn-label";
import InputGroup from "dnn-input-group";
import SingleLineInput from "dnn-single-line-input";
import GlobalIcon from "./GlobalIcon";

export default class EditBlock extends Component {
    
    render() {
        const {props} = this;

        return <InputGroup>
            <Label className="title"
                tooltipMessage={props.tooltip}
                label={props.label} style={{width: "auto"}} />
            {props.isGlobal && <GlobalIcon /> }
            <SingleLineInput value={props.value} />
        </InputGroup>;
    }
}

EditBlock.propTypes = {
    label: PropTypes.string,
    tooltip: PropTypes.string,
    value: PropTypes.string,
    isGlobal: PropTypes.bool.isRequired
};

EditBlock.defaultProps = {
    isGlobal: false
};