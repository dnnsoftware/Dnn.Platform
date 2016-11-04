import React, {Component, PropTypes } from "react";
import Label from "dnn-label";
import InputGroup from "dnn-input-group";
import Switch from "dnn-switch";
import GlobalIcon from "./GlobalIcon";

export default class InfoBlock extends Component {

    render() {
        const {props} = this;

        return <InputGroup>
            <Label className="title"
                tooltipMessage={props.tooltip}
                label={props.label} style={{width: "auto", "margin-top": "8px"}} />   
            {props.isGlobal && <GlobalIcon isSwitch={true} /> }         
            <Switch labelHidden={false}
                readOnly={false}
                value={props.value}
                onChange={props.onChange} 
                style={{float: "right"}} />
        </InputGroup>;
    }
}

InfoBlock.propTypes = {
    label: PropTypes.string,
    tooltip: PropTypes.string,
    value: PropTypes.bool.isRequired,
    onChange: PropTypes.func.isRequired,
    isGlobal: PropTypes.bool.isRequired  
};

InfoBlock.defaultProps = {
    isGlobal: false
};