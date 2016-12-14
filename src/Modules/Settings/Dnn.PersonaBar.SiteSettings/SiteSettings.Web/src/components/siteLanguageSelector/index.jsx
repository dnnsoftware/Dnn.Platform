import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    siteInfo as SiteInfoActions,
    search as SearchActions
} from "../../actions";
import InputGroup from "dnn-input-group";
import Dropdown from "dnn-dropdown";
import Label from "dnn-label";
import "./style.less";
import resx from "../../resources";
import styles from "./style.less";
import util from "../../utils";

let isHost = false;

class SiteLanguageSelector extends Component {
    constructor() {
        super();
        this.state = {
            cultureCode: undefined,
            portalId: undefined
        };
        isHost = util.settings.isHost;
    }

    componentWillMount() {
        const {state, props} = this;
        this.setState({
            portalId: props.portalId,
            cultureCode: props.cultureCode
        }, () => {
            if (isHost) {
                props.dispatch(SiteInfoActions.getPortals(() => {
                    props.dispatch(SearchActions.getCultureList(props.portalId));
                }));
            }
            else {
                props.dispatch(SearchActions.getCultureList(state.portalId));
            }
        });
    }

    componentWillReceiveProps(props) {
        const { state } = this;
        if (props.portalId !== state.portalId && props.portalId !== undefined) {
            this.onSiteChange({ value: props.portalId });
        }
        if (props.cultureCode !== state.cultureCode) {
            this.onLanguageChange({ value: props.cultureCode });
        }
    }

    onSiteChange(event) {
        let {state, props} = this;
        props.dispatch(SearchActions.getCultureList(event.value, () => {
            let culture = this.validateCultureCode();
            if (state.cultureCode !== culture) {
                this.setState({
                    portalId: event.value,
                    cultureCode: culture
                });
                this.triggerEvent("portalIdCultureCodeChanged",
                    {
                        portalId: event.value,
                        cultureCode: culture,
                        referrer: props.referrer,
                        referrerText: props.referrerText,
                        backToReferrerFunc: props.backToReferrerFunc
                    });
            }
            else {
                this.setState({
                    portalId: event.value
                });
                this.triggerEvent("portalIdChanged",
                    {
                        portalId: event.value,
                        referrer: props.referrer,
                        referrerText: props.referrerText,
                        backToReferrerFunc: props.backToReferrerFunc
                    });
            }
        }));
    }

    onLanguageChange(event) {
        let {props} = this;
        this.setState({
            cultureCode: event.value
        });
        this.triggerEvent("cultureCodeChanged",
            {
                cultureCode: event.value,
                referrer: props.referrer,
                referrerText: props.referrerText,
                backToReferrerFunc: props.backToReferrerFunc
            });
    }

    getSiteOptions() {
        const {state, props} = this;
        let options = [];
        if (props.portals !== undefined) {
            options = props.portals.map((item) => {
                if (item.IsCurrentPortal && state.portalId === undefined) {
                    this.setState({
                        portalId: item.PortalID
                    });
                }
                return {
                    label: item.PortalName, value: item.PortalID
                };
            });
        }
        return options;
    }

    getLanguageOptions() {
        const {props} = this;
        let options = [];
        if (props.languages !== undefined) {
            options = props.languages.map((item) => {
                return {
                    label: <div style={{ float: "left", display: "flex" }}>
                        <div className="language-flag">
                            <img src={item.Icon} />
                        </div>
                        <div className="language-name">{item.Name}</div>
                    </div>, value: item.Code
                };
            });
        }
        return options;
    }

    validateCultureCode() {
        const {state, props} = this;

        if (props.languages.filter(language => {
            return language.Code === state.cultureCode;
        }).length > 0) {
            return state.cultureCode;
        }
        else {
            let culture = (props.languages.filter(language => {
                return language.IsDefault === true;
            }))[0].Code;
            this.setState({
                cultureCode: culture
            });
            this.triggerEvent("cultureCodeChanged",
                {
                    cultureCode: culture,
                    referrer: props.referrer,
                    referrerText: props.referrerText,
                    backToReferrerFunc: props.backToReferrerFunc
                });
            return culture;
        }
    }

    triggerEvent(name, parameter) {
        let event = document.createEvent("Event");
        event.initEvent(name, true, true);
        event = Object.assign(event, parameter);
        document.dispatchEvent(event);
    }

    renderSitesList() {
        const {props, state} = this;
        if (props.portals) {
            return (<div className="site-selection">
                <InputGroup>
                    <Label
                        labelType="inline"
                        label={resx.get("SiteSelectionLabel")}
                        />
                    <Dropdown
                        options={this.getSiteOptions()}
                        value={state.portalId}
                        onSelect={this.onSiteChange.bind(this)}
                        />
                </InputGroup>
            </div>
            );
        }
        else return (<div />);
    }

    renderLanguagesList() {
        const {props} = this;
        if (props.languages) {
            return (<div className="language-selection">
                <InputGroup>
                    <Label
                        labelType="inline"
                        label={resx.get("LanguageSelectionLabel")}
                        />
                    <Dropdown
                        options={this.getLanguageOptions()}
                        value={this.validateCultureCode()}
                        onSelect={this.onLanguageChange.bind(this)}
                        />
                </InputGroup>
            </div>
            );
        }
        else return (<div />);
    }

    /* eslint-disable react/no-danger */
    render() {
        return <div className={styles.sitesHeader}>
            {isHost && this.renderSitesList()}
            {this.renderLanguagesList()}
        </div>;
    }
}

SiteLanguageSelector.propTypes = {
    dispatch: PropTypes.func.isRequired,
    portals: PropTypes.array,
    languages: PropTypes.array,
    cultureCode: PropTypes.string,
    portalId: PropTypes.number,
    referrer: PropTypes.string,
    referrerText: PropTypes.string,
    backToReferrerFunc: PropTypes.func
};

function mapStateToProps(state) {
    return {
        portals: state.siteInfo.portals,
        languages: state.search.cultures
    };
}

export default connect(mapStateToProps)(SiteLanguageSelector);