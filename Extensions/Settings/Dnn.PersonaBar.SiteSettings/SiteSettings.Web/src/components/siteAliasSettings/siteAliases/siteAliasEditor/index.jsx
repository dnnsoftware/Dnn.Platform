import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import { SingleLineInputWithError, GridSystem, Label, Button, InputGroup, Dropdown } from "@dnnsoftware/dnn-react-common";
import {
    siteBehavior as SiteBehaviorActions
} from "../../../../actions";
import util from "../../../../utils";
import resx from "../../../../resources";

class SiteAliasEditor extends Component {
    constructor() {
        super();

        this.state = {
            aliasDetail: {
                HTTPAlias: "",
                BrowserType: "Normal",
                Skin: "",
                CultureCode: "",
                IsPrimary: false
            },
            error: {
                alias: true
            },
            triedToSubmit: false
        };
    }

    componentDidMount() {
        const {props} = this;
        if (props.aliasId) {
            props.dispatch(SiteBehaviorActions.getSiteAlias(props.aliasId));
        }
    }

    onSettingChange(key, event) {
        let {state, props} = this;
        let aliasDetail = Object.assign({}, state.aliasDetail);

        if (aliasDetail[key] === "" && key === "HTTPAlias") {
            state.error["alias"] = true;
        }
        else if (aliasDetail[key] !== "" && key === "HTTPAlias") {
            state.error["alias"] = false;
        }

        if (key === "BrowserType" || key === "CultureCode" || key === "Skin") {
            aliasDetail[key] = event.value;
        }
        else {
            aliasDetail[key] = typeof (event) === "object" ? event.target.value : event;
        }

        this.setState({
            aliasDetail: aliasDetail,
            triedToSubmit: false,
            error: state.error
        });

        props.dispatch(SiteBehaviorActions.siteAliasClientModified(aliasDetail));
    }

    getBrowserOptions() {
        let options = [];
        if (this.props.siteAliases.BrowserTypes !== undefined) {
            options = this.props.siteAliases.BrowserTypes.map((item) => {
                return { label: item, value: item };
            });
        }
        return options;
    }

    getLanguageOptions() {
        let options = [];
        const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
        if (this.props.siteAliases.Languages !== undefined) {
            options = this.props.siteAliases.Languages.map((item) => {
                return { label: item.Key, value: item.Value };
            });
            options.unshift({ label: noneSpecifiedText, value: "" });
        }
        return options;
    }

    getSkinOptions() {
        let options = [];
        const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
        if (this.props.siteAliases.Skins !== undefined) {
            options = this.props.siteAliases.Skins.map((item) => {
                return { label: item.Key, value: item.Value };
            });
            options.unshift({ label: noneSpecifiedText, value: "" });
        }
        return options;
    }

    onSave() {
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.alias) {
            return;
        }

        props.onUpdate(state.aliasDetail);
    }

    onChangePrimary(isPrimary) {
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.alias) {
            return;
        }
        let aliasDetail = Object.assign({}, state.aliasDetail);
        aliasDetail["IsPrimary"] = isPrimary;
        this.setState({
            aliasDetail: aliasDetail
        }, () => {
            props.onUpdate(aliasDetail);
        });
    }

    onCancel() {
        const {props} = this;
        if (props.siteAliasClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(SiteBehaviorActions.cancelSiteAliasClientModified());
                props.Collapse();
            });
        }
        else {
            props.Collapse();
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        /* eslint-disable react/no-danger */
        if (this.state.aliasDetail !== undefined || this.props.id === "add") {
            const columnOne = <div key="column-one" className="left-column">
                <InputGroup>
                    <Label
                        label={resx.get("SiteAlias")}
                    />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={this.state.error.alias && this.state.triedToSubmit}
                        errorMessage={resx.get("InvalidAlias")}
                        value={this.state.aliasDetail.HTTPAlias}
                        onChange={this.onSettingChange.bind(this, "HTTPAlias")}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        label={resx.get("Browser")}
                    />
                    <Dropdown
                        options={this.getBrowserOptions()}
                        value={this.state.aliasDetail.BrowserType}
                        onSelect={this.onSettingChange.bind(this, "BrowserType")}
                    />
                </InputGroup>
            </div>;
            const columnTwo = <div key="column-two" className="right-column">
                {this.props.siteAliases.Languages.length > 1 &&
                    <InputGroup>
                        <Label
                            label={resx.get("Language")}
                        />
                        <Dropdown
                            options={this.getLanguageOptions()}
                            value={this.state.aliasDetail.CultureCode}
                            onSelect={this.onSettingChange.bind(this, "CultureCode")}
                        />
                    </InputGroup>
                }
                <InputGroup>
                    <Label
                        label={resx.get("Theme")}
                    />
                    <Dropdown
                        options={this.getSkinOptions()}
                        value={this.state.aliasDetail.Skin}
                        onSelect={this.onSettingChange.bind(this, "Skin")}
                    />
                </InputGroup>
            </div>;

            return (
                <div className="alias-editor">
                    <GridSystem numberOfColumns={2}>{[columnOne, columnTwo]}</GridSystem>
                    <div className="editor-buttons-box">
                        <Button
                            type="secondary"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            type="secondary"
                            onClick={this.onChangePrimary.bind(this, this.state.aliasDetail.IsPrimary ? false : true)}>
                            {this.state.aliasDetail.IsPrimary ? resx.get("UnassignPrimary") :resx.get("SetPrimary")}
                        </Button>
                        <Button
                            type="primary"
                            onClick={this.onSave.bind(this)}>
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

SiteAliasEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    aliasDetail: PropTypes.object,
    aliasId: PropTypes.number,
    siteAliases: PropTypes.object,
    Collapse: PropTypes.func,
    onUpdate: PropTypes.func,
    id: PropTypes.string,
    siteAliasClientModified: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        aliasDetail: state.siteBehavior.aliasDetail,
        siteAliases: state.siteBehavior.siteAliases,
        siteAliasClientModified: state.siteBehavior.siteAliasClientModified
    };
}

export default connect(mapStateToProps)(SiteAliasEditor);