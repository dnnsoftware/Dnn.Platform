import React, { PropTypes, Component } from "react";

import IconButton from "./IconButton/IconButton";

class StatusSwitch extends Component {
    constructor(props) {
        super(props);
        
        this.state = {
        };
    }

    changeState() {
        const {props} = this;

        if (props.status === 3){
            return;
        }

        let status = props.status + 1;
        if (status > 2){
            status = 0;
        }

        if (typeof props.onChange === "function"){
            props.onChange(status);
        }
    }

    render() {
        const {props } = this;

        let type = "";
        let className = "";
        switch (props.status) {
            case 0:
                type = "unchecked";
                break;
            case 1:
                type = "checkbox";
                break;
            case 2:
                type = "denied";
                break;
            case 3:
                type = "checked";
                className = "locked";
                break;
        }
        return (
            
            <IconButton type={type} className={className} onClick={this.changeState.bind(this)} />
        );
    }
}

StatusSwitch.propTypes = {
    localization: PropTypes.object,
    definitions: PropTypes.object.isRequired,
    permission: PropTypes.object.isRequired,
    type: PropTypes.oneOf(["role", "user"]),
    status: PropTypes.number
};

export default StatusSwitch;