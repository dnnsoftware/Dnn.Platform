import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import {
    Dropdown,
    InputGroup,
    Switch,
    Label,
    Button,
    Tooltip,
    MultiLineInputWithError,
} from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import "./style.less";

class CspSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            cspSettings: undefined,
        };        
    }

    UNSAFE_componentWillMount() {
        const { props } = this;
        if (props.cspSettings) {
            this.setState({
                cspSettings: props.cspSettings,
            });
            return;
        }
        this.getSettings();
    }

    getCspHeaderModeOptions() {
        let cspHeaderModeOptions = [];
        cspHeaderModeOptions.push({ "value": 0, "label": resx.get("CspHeaderModeOff") });
        cspHeaderModeOptions.push({ "value": 1, "label": resx.get("CspHeaderModeReportOnly") });
        cspHeaderModeOptions.push({ "value": 2, "label": resx.get("CspHeaderModeOn") });

        return cspHeaderModeOptions;
    }

    onSettingChange(key, event) {
        const { state, props } = this;
        let cspSettings = Object.assign({}, state.cspSettings);
        cspSettings[key] = typeof event === "object" ? event.target.value : event;
        this.setState({
            cspSettings: cspSettings,
        }, () => {
            
        });
        props.dispatch(SecurityActions.cspSettingsClientModified(cspSettings));
    }

    onUpdate(event) {
        event.preventDefault();
        const { props, state } = this;

        props.dispatch(
            SecurityActions.updateCspSettings(
                state.cspSettings,
                () => {
                    util.utilities.notify(resx.get("CspSettingsUpdateSuccess"));
                    this.getSettings();
                },
                () => {
                    util.utilities.notifyError(resx.get("CspSettingsError"));
                }
            )
        );
    }

    onCancel() {
        util.utilities.confirm(
            resx.get("CspSettingsRestoreWarning"),
            resx.get("Yes"),
            resx.get("No"),
            () => {
                this.getSettings();
            }
        );
    }

    getSettings() {
        const { props } = this;
        props.dispatch(
            SecurityActions.getCspSettings((data) => {
                let cspSettings = Object.assign({}, data.Results.Settings);
                this.setState({
                    cspSettings: cspSettings,
                });
            })
        );
    }
     
    render() {
        const { state } = this;        

        if (state.cspSettings) {              
            return (
                <div id="cspSettings-container">                                          
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plCspHeaderMode.Help")}
                            label={resx.get("plCspHeaderMode")}
                        />
                        <Dropdown
                            options={this.getCspHeaderModeOptions()}
                            value={state.cspSettings.CspHeaderMode}
                            onSelect={(newVal) => {
                                this.onSettingChange("CspHeaderMode", newVal.value);
                            }}                            
                        />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plCspHeader.Help")}
                            label={resx.get("plCspHeader")}
                        />
                         <MultiLineInputWithError
                            value={state.cspSettings.CspHeader}
                            onChange={this.onSettingChange.bind(this, "CspHeader")} />
                    </InputGroup>
                    <div className="warningBox">
                        <div className="warningText">{resx.get("CspSettings.Help")}</div>
                    </div>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plCspReportingHeader.Help")}
                            label={resx.get("plCspReportingHeader")}
                        />
                         <MultiLineInputWithError
                            value={state.cspSettings.CspReportingHeader}
                            onChange={this.onSettingChange.bind(this, "CspReportingHeader")} />
                    </InputGroup>
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.cspSettingsClientModified}
                            type="neutral"
                            onClick={this.onCancel.bind(this)}
                        >
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.cspSettingsClientModified}
                            type="primary"
                            onClick={this.onUpdate.bind(this)}
                        >
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        } else return <div />;
    }
}

CspSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    cspSettings: PropTypes.object,
    cspSettingsClientModified: PropTypes.bool,
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        cspSettings: state.security.cspSettings,
        cspSettingsClientModified: state.security.cspSettingsClientModified,
    };
}

export default connect(mapStateToProps)(CspSettingsPanelBody);
