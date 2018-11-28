import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    siteBehavior as SiteBehaviorActions
} from "../../actions";
import ProfileProperties from "./profileProperties";
import InputGroup from "dnn-input-group";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Grid from "dnn-grid-system";
import Switch from "dnn-switch";
import Dropdown from "dnn-dropdown";
import Label from "dnn-label";
import Button from "dnn-button";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

class ProfileSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            profileSettings: undefined
        };
    }

    loadData() {
        const {props} = this;
        if (props.profileSettings) {
            if (props.portalId === undefined || props.profileSettings.PortalId === props.portalId) {
                return false;
            }
            else {
                return true;
            }
        }
        else {
            return true;
        }
    }

    componentDidMount() {
        const {props} = this;
        if (!this.loadData()) {
            this.setState({
                profileSettings: props.profileSettings
            });
            return;
        }
        props.dispatch(SiteBehaviorActions.getProfileSettings(props.portalId, (data) => {
            this.setState({
                profileSettings: Object.assign({}, data.Settings)
            });
        }));
    }

    componentDidUpdate(props) {
        this.setState({
            profileSettings: Object.assign({}, props.profileSettings)
        });
    }

    getProfileVisibilityOptions() {
        let options = [];
        if (this.props.userVisibilityOptions !== undefined) {
            options = this.props.userVisibilityOptions.map((item) => {
                return { label: resx.get(item.label), value: item.value };
            });
        }
        return options;
    }

    onSettingChange(key, event) {
        let {state, props} = this;

        let profileSettings = Object.assign({}, state.profileSettings);
        if (key === "ProfileDefaultVisibility") {
            profileSettings[key] = event.value;
        }
        else {
            profileSettings[key] = typeof (event) === "object" ? event.target.value : event;
        }
        this.setState({
            profileSettings: profileSettings
        });

        props.dispatch(SiteBehaviorActions.profileSettingsClientModified(profileSettings));
    }

    onUpdate(event) {
        event.preventDefault();
        const {props, state} = this;

        props.dispatch(SiteBehaviorActions.updateProfileSettings(state.profileSettings, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
    }

    onCancel() {
        const {props} = this;
        util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SiteBehaviorActions.getProfileSettings(props.portalId, (data) => {
                let profileSettings = Object.assign({}, data.Settings);
                this.setState({
                    profileSettings
                });
            }));
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        if (state.profileSettings) {
            const columnOne = <div className="left-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("Profile_DefaultVisibility.Help")}
                        label={resx.get("Profile_DefaultVisibility")}
                    />
                    <Dropdown
                        options={this.getProfileVisibilityOptions()}
                        value={state.profileSettings.ProfileDefaultVisibility}
                        onSelect={this.onSettingChange.bind(this, "ProfileDefaultVisibility")}
                    />
                </InputGroup>
                <InputGroup>
                    <div className="profileSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("redirectOldProfileUrlsLabel.Help")}
                            label={resx.get("redirectOldProfileUrlsLabel")}
                        />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.profileSettings.RedirectOldProfileUrl}
                            onChange={this.onSettingChange.bind(this, "RedirectOldProfileUrl")}
                        />
                    </div>
                </InputGroup>
            </div>;
            const columnTwo = <div className="right-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("vanilyUrlPrefixLabel.Help")}
                        label={resx.get("vanilyUrlPrefixLabel")}
                    />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={false}
                        value={state.profileSettings.VanityUrlPrefix}
                        onChange={this.onSettingChange.bind(this, "VanityUrlPrefix")}
                    />
                    <div className="VanityUrlPrefix">/{resx.get("VanityUrlExample")}</div>
                </InputGroup>
                <InputGroup>
                    <div className="profileSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("Profile_DisplayVisibility.Help")}
                            label={resx.get("Profile_DisplayVisibility")}
                        />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={state.profileSettings.ProfileDisplayVisibility}
                            onChange={this.onSettingChange.bind(this, "ProfileDisplayVisibility")}
                        />
                    </div>
                </InputGroup>
            </div>;

            return (
                <div className={styles.profileSettings}>
                    <div className="sectionTitleNoBorder">{resx.get("UserProfileSettings")}</div>
                    <Grid numberOfColumns={2}>{[columnOne, columnTwo]}</Grid>
                    <ProfileProperties portalId={props.portalId} cultureCode={props.cultureCode}/>
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.profileSettingsClientModified}
                            type="secondary"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.profileSettingsClientModified}
                            type="primary"
                            onClick={this.onUpdate.bind(this)}>
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

ProfileSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    profileSettings: PropTypes.object,
    userVisibilityOptions: PropTypes.array,
    profileSettingsClientModified: PropTypes.bool,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        profileSettings: state.siteBehavior.profileSettings,
        userVisibilityOptions: state.siteBehavior.userVisibilityOptions,
        profileSettingsClientModified: state.siteBehavior.profileSettingsClientModified
    };
}

export default connect(mapStateToProps)(ProfileSettingsPanelBody);