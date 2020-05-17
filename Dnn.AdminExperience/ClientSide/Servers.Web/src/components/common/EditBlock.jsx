import React, {Component } from "react";
import PropTypes from "prop-types";
import { Label, InputGroup, SingleLineInputWithError } from "@dnnsoftware/dnn-react-common";
import GlobalIcon from "./GlobalIcon";

export default class EditBlock extends Component {
    
    render() {
        const {props} = this;

        return <InputGroup>
            <Label className="title"
                tooltipMessage={props.tooltip}
                label={props.label} style={{width: "auto"}} />
            {props.isGlobal && <GlobalIcon /> }
            <SingleLineInputWithError 
                value={props.value} 
                type={props.type} 
                onChange={props.onChange}
                error={!!props.error} 
                errorMessage={props.error} />
        </InputGroup>;
    }
}

EditBlock.propTypes = {
    label: PropTypes.string,
    tooltip: PropTypes.string,
    value: PropTypes.string,
    isGlobal: PropTypes.bool.isRequired,
    type: PropTypes.string,
    onChange: PropTypes.func,
    error: PropTypes.string
};

EditBlock.defaultProps = {
    isGlobal: false
};