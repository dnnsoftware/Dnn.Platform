import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import Button from "dnn-button";
import Localization from "localization";
import styles from "./style.less";

class AuthenticationSystemSettings extends Component {
    render() {
        const {props, state} = this;

        return (
            <GridCell>
                Hello World
            </GridCell>
        );
    }
}

AuthenticationSystemSettings.PropTypes = {
};

export default AuthenticationSystemSettings;