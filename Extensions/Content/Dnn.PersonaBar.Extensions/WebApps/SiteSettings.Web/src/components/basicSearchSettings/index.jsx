import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    search as SearchActions
} from "../../actions";

import { InputGroup, SingleLineInputWithError, DropdownWithError, NumberSlider, GridSystem, Switch, Label, Button, Tooltip } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

const re = /^[1-9]\d*$/;

class BasicSearchSettingsPanelBody extends Component {
    constructor() {
        super();
        this._mounted = false;
        this.state = {
            basicSearchSettings: undefined,
            error: {
                minlength: false,
                maxlength: false
            },
            triedToSubmit: false
        };
    }


    componentDidMount() {
        const {props} = this;
        this._mounted = true;
        if (this.isHost()) {
            props.dispatch(SearchActions.getBasicSearchSettings((data) => {
                if(this._mounted) {
                    this.setState({
                        basicSearchSettings: Object.assign({}, data.Settings)
                    });
                }
            }));
        }
    }

    componentWillUnmount() {
        this._mounted = false;
    }

    isHost() {
        return util.settings.isHost;
    }

    onSettingChange(key, event) {
        let {state, props} = this;
        let basicSearchSettings = Object.assign({}, state.basicSearchSettings);
        const error = {};

        if (key === "TitleBoost" || key === "TagBoost" || key === "ContentBoost" || key === "DescriptionBoost" || key === "AuthorBoost") {
            basicSearchSettings[key] = event;
        }
        else if (key === "SearchCustomAnalyzer") {
            basicSearchSettings[key] = event.value;
        }
        else {
            basicSearchSettings[key] = typeof (event) === "object" ? event.target.value : event;
        }

        if (!re.test(basicSearchSettings[key]) && key === "MinWordLength") {
            error["minlength"] = true;
        }
        else if (re.test(basicSearchSettings[key]) && key === "MinWordLength") {
            error["minlength"] = false;
        }

        if (key === "MaxWordLength" && (!re.test(basicSearchSettings[key])
            || parseInt(basicSearchSettings["MinWordLength"]) >= parseInt(basicSearchSettings["MaxWordLength"]))) {
            error["maxlength"] = true;
        }
        else if (key === "MaxWordLength" && re.test(basicSearchSettings[key])
            && parseInt(basicSearchSettings["MaxWordLength"]) > parseInt(basicSearchSettings["MinWordLength"])) {
            error["maxlength"] = false;
        }

        this.setState({
            basicSearchSettings: basicSearchSettings,
            error,
            triedToSubmit: false
        });

        props.dispatch(SearchActions.basicSearchSettingsClientModified(basicSearchSettings));
    }

    getAnalyzerTypeOptions() {
        let options = [];
        const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
        if (this.props.searchCustomAnalyzers !== undefined) {
            options = this.props.searchCustomAnalyzers.map((item) => {
                return { label: item, value: item };
            });
            options.unshift({ label: noneSpecifiedText, value: "" });
        }
        return options;
    }

    onUpdate(event) {
        event.preventDefault();
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });

        props.dispatch(SearchActions.updateBasicSearchSettings(state.basicSearchSettings, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
    }

    onCancel() {
        const {props} = this;
        util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SearchActions.getBasicSearchSettings((data) => {
                this.setState({
                    basicSearchSettings: Object.assign({}, data.Settings)
                });
            }));
        });
    }

    onReindexContent() {
        const {props} = this;
        util.utilities.confirm(resx.get("ReIndexConfirmationMessage"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SearchActions.portalSearchReindex(props.portalId));
        });
    }

    onReindexHostContent() {
        const {props} = this;
        util.utilities.confirm(resx.get("ReIndexConfirmationMessage"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SearchActions.hostSearchReindex());
        });
    }

    onCompactIndex() {
        const {props} = this;
        util.utilities.confirm(resx.get("CompactIndexConfirmationMessage"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SearchActions.compactSearchIndex());
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        const {state} = this;
        if (this.isHost()) {
            if (state.basicSearchSettings) {
                const columnOne = <div className="left-column" key="columnOneSearchSettingsPanel">
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("lblIndexWordMinLength.Help")}
                            label={resx.get("lblIndexWordMinLength")}
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }}
                                />}
                        />
                        <SingleLineInputWithError
                            inputStyle={{ margin: "0" }}
                            withLabel={false}
                            error={state.error.minlength && state.triedToSubmit}
                            errorMessage={resx.get("valIndexWordMinLengthRequired.Error")}
                            value={state.basicSearchSettings.MinWordLength}
                            onChange={this.onSettingChange.bind(this, "MinWordLength")}
                            style={{ width: "100%" }}
                        />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("lblIndexWordMaxLength.Help")}
                            label={resx.get("lblIndexWordMaxLength")}
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }}
                                />}
                        />
                        <SingleLineInputWithError
                            inputStyle={{ margin: "0" }}
                            withLabel={false}
                            error={state.error.maxlength && state.triedToSubmit}
                            errorMessage={resx.get("valIndexWordMaxLengthRequired.Error")}
                            value={state.basicSearchSettings.MaxWordLength}
                            onChange={this.onSettingChange.bind(this, "MaxWordLength")}
                            style={{ width: "100%" }}
                        />
                    </InputGroup>
                </div>;
                const columnTwo = <div className="right-column" key="columnTwoSearchSettingsPanel">
                    <DropdownWithError
                        style={{ maxWidth: "100%" }}
                        options={this.getAnalyzerTypeOptions()}
                        value={state.basicSearchSettings.SearchCustomAnalyzer}
                        onSelect={this.onSettingChange.bind(this, "SearchCustomAnalyzer")}
                        tooltipMessage={resx.get("lblCustomAnalyzer.Help")}
                        label={resx.get("lblCustomAnalyzer")}
                        labelStyle={{ width: "auto" }}
                        labelIsMultiLine={true}
                        extraToolTips={
                            <Tooltip
                                messages={[resx.get("GlobalSetting")]}
                                type="global"
                                style={{ float: "left", position: "static", marginTop: -3 }}
                            />}
                    />
                    <InputGroup>
                        <div className="basicSearchSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("lblAllowLeadingWildcard.Help")}
                                label={resx.get("lblAllowLeadingWildcard")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }}
                                    />}
                            />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.basicSearchSettings.AllowLeadingWildcard}
                                onChange={this.onSettingChange.bind(this, "AllowLeadingWildcard")}
                            />
                        </div>
                    </InputGroup>
                </div>;
                return (
                    <div className={styles.basicSearchSettings}>
                        <GridSystem numberOfColumns={2}>{[columnOne, columnTwo]}</GridSystem>
                        <div className="sectionTitle">{resx.get("SearchPriorities")}</div>
                        <InputGroup>
                            <div className="basicSearchSettings-row_slider">
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("lblTitleBoost.Help")}
                                    label={resx.get("lblTitleBoost")}
                                    extra={
                                        <Tooltip
                                            messages={[resx.get("GlobalSetting")]}
                                            type="global"
                                            style={{ float: "left", position: "static" }}
                                        />}
                                />
                                <NumberSlider
                                    min={0}
                                    max={50}
                                    step={5}
                                    value={state.basicSearchSettings.TitleBoost}
                                    onChange={this.onSettingChange.bind(this, "TitleBoost")}
                                />
                            </div>
                        </InputGroup>
                        <InputGroup>
                            <div className="basicSearchSettings-row_slider">
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("lblTagBoost.Help")}
                                    label={resx.get("lblTagBoost")}
                                    extra={
                                        <Tooltip
                                            messages={[resx.get("GlobalSetting")]}
                                            type="global"
                                            style={{ float: "left", position: "static" }}
                                        />}
                                />
                                <NumberSlider
                                    min={0}
                                    max={50}
                                    step={5}
                                    value={state.basicSearchSettings.TagBoost}
                                    onChange={this.onSettingChange.bind(this, "TagBoost")}
                                />
                            </div>
                        </InputGroup>
                        <InputGroup>
                            <div className="basicSearchSettings-row_slider">
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("lblContentBoost.Help")}
                                    label={resx.get("lblContentBoost")}
                                    extra={
                                        <Tooltip
                                            messages={[resx.get("GlobalSetting")]}
                                            type="global"
                                            style={{ float: "left", position: "static" }}
                                        />}
                                />
                                <NumberSlider
                                    min={0}
                                    max={50}
                                    step={5}
                                    value={state.basicSearchSettings.ContentBoost}
                                    onChange={this.onSettingChange.bind(this, "ContentBoost")}
                                />
                            </div>
                        </InputGroup>
                        <InputGroup>
                            <div className="basicSearchSettings-row_slider">
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("v.Help")}
                                    label={resx.get("lblDescriptionBoost")}
                                    extra={
                                        <Tooltip
                                            messages={[resx.get("GlobalSetting")]}
                                            type="global"
                                            style={{ float: "left", position: "static" }}
                                        />}
                                />
                                <NumberSlider
                                    min={0}
                                    max={50}
                                    step={5}
                                    value={state.basicSearchSettings.DescriptionBoost}
                                    onChange={this.onSettingChange.bind(this, "DescriptionBoost")}
                                />
                            </div>
                        </InputGroup>
                        <InputGroup>
                            <div className="basicSearchSettings-row_slider">
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("lblAuthorBoost.Help")}
                                    label={resx.get("lblAuthorBoost")}
                                    extra={
                                        <Tooltip
                                            messages={[resx.get("GlobalSetting")]}
                                            type="global"
                                            style={{ float: "left", position: "static" }}
                                        />}
                                />
                                <NumberSlider
                                    min={0}
                                    max={50}
                                    step={5}
                                    value={state.basicSearchSettings.AuthorBoost}
                                    onChange={this.onSettingChange.bind(this, "AuthorBoost")}
                                />
                            </div>
                        </InputGroup>

                        <div className="sectionTitle">{resx.get("SearchIndex")}</div>
                        <div className="searchIndexWrapper">
                            <div className="searchIndexWrapper-left">
                                <div className="searchReIndexWarning">{resx.get("MessageReIndexWarning")}</div>
                                <div className="searchCompactIndexWarning">{resx.get("MessageCompactIndexWarning")}</div>
                                <InputGroup>
                                    <Label
                                        labelType="inline"
                                        tooltipMessage={resx.get("lblSearchIndexPath.Help")}
                                        label={resx.get("lblSearchIndexPath")}
                                    />
                                    <Label
                                        labelType="inline"
                                        label={state.basicSearchSettings.SearchIndexPath}
                                    />
                                </InputGroup>
                                <InputGroup>
                                    <Label
                                        labelType="inline"
                                        tooltipMessage={resx.get("lblSearchIndexDbSize.Help")}
                                        label={resx.get("lblSearchIndexDbSize")}
                                    />
                                    <Label
                                        labelType="inline"
                                        label={state.basicSearchSettings.SearchIndexDbSize}
                                    />
                                </InputGroup>
                                <InputGroup>
                                    <Label
                                        labelType="inline"
                                        tooltipMessage={resx.get("lblSearchIndexActiveDocuments.Help")}
                                        label={resx.get("lblSearchIndexActiveDocuments")}
                                    />
                                    <Label
                                        labelType="inline"
                                        label={state.basicSearchSettings.SearchIndexTotalActiveDocuments}
                                    />
                                </InputGroup>
                                <InputGroup>
                                    <Label
                                        labelType="inline"
                                        tooltipMessage={resx.get("lblSearchIndexDeletedDocuments.Help")}
                                        label={resx.get("lblSearchIndexDeletedDocuments")}
                                    />
                                    <Label
                                        labelType="inline"
                                        label={state.basicSearchSettings.SearchIndexTotalDeletedDocuments}
                                    />
                                </InputGroup>
                                <InputGroup>
                                    <Label
                                        labelType="inline"
                                        tooltipMessage={resx.get("lblSearchIndexLastModifiedOn.Help")}
                                        label={resx.get("lblSearchIndexLastModifiedOn")}
                                    />
                                    <Label
                                        labelType="inline"
                                        label={state.basicSearchSettings.SearchIndexLastModifiedOn || "-"}
                                    />
                                </InputGroup>
                                <div className="searchIndexWarning">{resx.get("MessageIndexWarning")}</div>
                            </div>
                            <div className="searchIndexWrapper-right">
                                <Button
                                    type="secondary"
                                    onClick={this.onCompactIndex.bind(this)}>
                                    {resx.get("CompactIndex")}
                                </Button>
                                <Button
                                    type="secondary"
                                    onClick={this.onReindexContent.bind(this)}>
                                    {resx.get("ReindexContent")}
                                </Button>
                                <Button
                                    type="secondary"
                                    onClick={this.onReindexHostContent.bind(this)}>
                                    {resx.get("ReindexHostContent")}
                                </Button>
                            </div>
                        </div>
                        <div className="buttons-box">
                            <Button
                                disabled={!this.props.basicSearchSettingsClientModified}
                                type="secondary"
                                onClick={this.onCancel.bind(this)}>
                                {resx.get("Cancel")}
                            </Button>
                            <Button
                                disabled={!this.props.basicSearchSettingsClientModified}
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
        else {
            return (<div className={styles.basicSearchSettings}>
                <div className="sectionTitle">{resx.get("SearchIndex")}</div>
                <div className="searchIndexWrapper">
                    <div className="searchIndexWrapper-left">
                        <div className="searchReIndexWarning">{resx.get("MessageReIndexWarning")}</div>
                        <div className="searchIndexWarning">{resx.get("MessageIndexWarning")}</div>
                    </div>
                    <div className="searchIndexWrapper-right">
                        <Button
                            type="secondary"
                            onClick={this.onReindexContent.bind(this)}>
                            {resx.get("ReindexContent")}
                        </Button>
                    </div>
                </div>
            </div>);
        }
    }
}

BasicSearchSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    basicSearchSettings: PropTypes.object,
    searchCustomAnalyzers: PropTypes.array,
    basicSearchSettingsClientModified: PropTypes.bool,
    portalId: PropTypes.number
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        basicSearchSettings: state.search.basicSearchSettings,
        searchCustomAnalyzers: state.search.searchCustomAnalyzers,
        basicSearchSettingsClientModified: state.search.basicSearchSettingsClientModified
    };
}

export default connect(mapStateToProps)(BasicSearchSettingsPanelBody);