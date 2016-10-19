import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import RadioButtons from "dnn-radio-buttons";
import Collapsible from "react-collapse";

import "./style.less";

class ParseThemePackage extends Component {
    constructor() {
        super();
        this.state = {
            parseType: "0"
        };
    }

    onParseTypeChanged(type){
        this.setState({parseType: type});
    }

    render() {
        const {props, state} = this;

        return (
            <div className="parse-theme-package">
                <Button size="small">{Localization.get("ParseThemePackage")}</Button>
                <RadioButtons 
                    options={[{value: "0", label: Localization.get("Localized")}, {value: "1", label: Localization.get("Portable")}]} 
                    onChange={this.onParseTypeChanged.bind(this)}
                    value={this.state.parseType}/>
                <div className="clear" />
            </div>
        );
    }
}

ParseThemePackage.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
    };
}

export default connect(mapStateToProps)(ParseThemePackage);