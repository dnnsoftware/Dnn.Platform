import React, { Component } from "react";
import PropTypes from "prop-types";
import { Label, InputGroup, RadioButtons } from "@dnnsoftware/dnn-react-common";
import GlobalIcon from "./GlobalIcon";

export default class RadioButtonBlock extends Component {

    render() {
        const {props} = this;

        return <InputGroup>
            <Label className="title"
                tooltipMessage={props.tooltip}
                label={props.label} style={{width: "auto"}}/>
            {props.isGlobal && <GlobalIcon /> }
            <RadioButtons
                options={props.options}                
                value={props.value}
                onChange={props.onChange} />
        </InputGroup>;
    }
}

RadioButtonBlock.propTypes = {
    label: PropTypes.string,
    tooltip: PropTypes.string,
    options: PropTypes.array.isRequired,
    value: PropTypes.string,
    onChange: PropTypes.func.isRequired,
    isGlobal: PropTypes.bool.isRequired
};

RadioButtonBlock.defaultProps = {
    isGlobal: false
};