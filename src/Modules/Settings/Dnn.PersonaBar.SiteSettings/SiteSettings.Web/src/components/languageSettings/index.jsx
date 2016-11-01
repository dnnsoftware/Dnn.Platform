import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import { Tab, Tabs, TabList, TabPanel } from "react-tabs";
import {
    pagination as PaginationActions,
    siteSettings as SiteSettingsActions
} from "../../actions";
import InputGroup from "dnn-input-group";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInput from "dnn-multi-line-input";
import Grid from "dnn-grid-system";
import Dropdown from "dnn-dropdown";
import Label from "dnn-label";
import Button from "dnn-button";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

class LanguageSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            languageSettings: undefined
        };
    }

    componentWillMount() {
        const {state, props} = this;
        if (props.languageSettings) {
            this.setState({
                languageSettings: props.languageSettings
            });
            return;
        }
        props.dispatch(SiteSettingsActions.getLanguageSettings(props.portalId, (data) => {
            this.setState({
                languageSettings: Object.assign({}, data.Settings)
            });
        }));
    }

    componentWillReceiveProps(props) {
        let {state} = this;        

        this.setState({
            languageSettings: Object.assign({}, props.languageSettings)
        });
    }

    onSettingChange(key, event) {
        let {state, props} = this;
        let languageSettings = Object.assign({}, state.languageSettings);

        languageSettings[key] = typeof (event) === "object" ? event.target.value : event;

        this.setState({
            languageSettings: languageSettings
        });

        props.dispatch(SiteSettingsActions.languageSettingsClientModified(languageSettings));
    }
    
    onUpdate(event) {
        event.preventDefault();
        const {props, state} = this;

        props.dispatch(SiteSettingsActions.updateLanguageSettings(state.languageSettings, (data) => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, (error) => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
    }

    onCancel(event) {
        const {props, state} = this;
        util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SiteSettingsActions.getLanguageSettings(props.portalId, (data) => {
                this.setState({
                    languageSettings: Object.assign({}, data.Settings)
                });
            }));
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        if (state.languageSettings) {
            const columnOne = <div className="left-column">
                
            </div>;
            const columnTwo = <div className="right-column">
                
            </div>;           

            return (
                <div className={styles.languageSettings}>                    
                    <Grid children={[columnOne, columnTwo]} numberOfColumns={2} />                    
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.languageSettingsClientModified}
                            type="secondary"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.languageSettingsClientModified}
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

LanguageSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    languageSettings: PropTypes.object,
    languageSettingsClientModified: PropTypes.bool,
    portalId: PropTypes.number
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        languageSettings: state.siteSettings.settings,
        languageSettingsClientModified: state.siteSettings.clientModified
    };
}

export default connect(mapStateToProps)(LanguageSettingsPanelBody);