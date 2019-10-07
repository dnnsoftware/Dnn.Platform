import React, {Component } from "react";
import PropTypes from "prop-types";
import { Label, InputGroup, Switch } from "@dnnsoftware/dnn-react-common";
import GlobalIcon from "./GlobalIcon";

export default class InfoBlock extends Component {

    render() {
        const {props} = this;

        return <InputGroup>
            <Label className="title"
                tooltipMessage={props.tooltip}
                label={props.label} style={{width: "auto", "margin-top": "8px"}} />   
            {props.isGlobal && <GlobalIcon isSwitch={true} tooltipStyle={this.props.globalTooltipStyle} /> }         
            <Switch labelHidden={false}
                onText={props.onText}
                offText={props.offText}
                readOnly={props.readOnly}
                value={props.value}
                onChange={props.onChange} 
                style={{float: "right"}} />
        </InputGroup>;
    }
}

InfoBlock.propTypes = {
    label: PropTypes.string,
    onText: PropTypes.string,
    offText: PropTypes.string,
    tooltip: PropTypes.string,
    readOnly: PropTypes.bool,
    value: PropTypes.bool.isRequired,
    onChange: PropTypes.func.isRequired,
    isGlobal: PropTypes.bool.isRequired,
    globalTooltipStyle: PropTypes.object  
};

InfoBlock.defaultProps = {
    isGlobal: false,
    readOnly: false
};