import React, {Component } from "react";
import PropTypes from "prop-types";
import { Label, InputGroup } from "@dnnsoftware/dnn-react-common";

export default class InfoBlock extends Component {
    
    render() {
        const {props} = this;

        return <InputGroup>
            <Label className="title"
                tooltipMessage={props.tooltip}
                label={props.label} />
            <Label label={props.text} />
        </InputGroup>;
    }
}

InfoBlock.propTypes = {
    label: PropTypes.string,
    tooltip: PropTypes.string,
    text: PropTypes.string
};