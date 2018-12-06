import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    seo as SeoActions
} from "../../actions";
import InputGroup from "dnn-input-group";
import PagePicker from "dnn-page-picker";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInput from "dnn-multi-line-input";
import Grid from "dnn-grid-system";
import Label from "dnn-label";
import Button from "dnn-button";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

class TestUrlPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            test: {
                PageToTest: "",
                QueryString: "",
                CustomPageName: "",
                UrlToTest: ""
            }
        };
    }

    onSettingChange(key, event) {
        let {state, props} = this;
        let test = Object.assign({}, state.test);

        if (key === "PageToTest") {
            test[key] = event;
            if (test[key] === "-1") {
                props.dispatch(SeoActions.clearUrlTestResults());
            }
        }
        else if (key === "UrlToTest") {
            test[key] = typeof (event) === "object" ? event.target.value : event;
            if (test[key] === "") {
                props.dispatch(SeoActions.clearUrlRewritingTestResults());
            }
        }
        else {
            test[key] = typeof (event) === "object" ? event.target.value : event;
        }

        this.setState({
            test: test
        });
    }

    keyValuePairsToOptions(keyValuePairs) {
        let options = [];
        if (keyValuePairs !== undefined) {
            options = keyValuePairs.map((item) => {
                return { label: item.Key, value: item.Value };
            });
        }
        return options;
    }

    onTestPage() {
        let {state, props} = this;
        props.dispatch(SeoActions.testUrl(state.test.PageToTest, state.test.QueryString, state.test.CustomPageName));
    }

    onTestUrl() {
        let {state, props} = this;
        props.dispatch(SeoActions.testUrlRewrite(state.test.UrlToTest));
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
        const PageToTestParameters = {
            portalId: -2,
            cultureCode: "",
            isMultiLanguage: false,
            excludeAdminTabs: false,
            disabledNotSelectable: false,
            roles: "0",
            sortOrder: 0
        };

        const columnOne = <div className="left-column">
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("selectPageToTestLabel.Help")}
                    label={resx.get("selectPageToTestLabel") + "*"} />
                <PagePicker
                    serviceFramework={util.utilities.sf}
                    style={{ width: "100%", zIndex: 2 }}
                    OnSelect={this.onSettingChange.bind(this, "PageToTest")}
                    defaultLabel={noneSpecifiedText}
                    noneSpecifiedText={noneSpecifiedText}
                    CountText={"{0} Results"}
                    PortalTabsParameters={PageToTestParameters} />
            </InputGroup>
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("queryStringLabel.Help")}
                    label={resx.get("queryStringLabel")} />
                <SingleLineInputWithError
                    inputStyle={{ margin: "0" }}
                    withLabel={false}
                    error={false}
                    value={state.test.QueryString}
                    onChange={this.onSettingChange.bind(this, "QueryString")} />
            </InputGroup>
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("pageNameLabel.Help")}
                    label={resx.get("pageNameLabel")} />
                <SingleLineInputWithError
                    inputStyle={{ margin: "0" }}
                    withLabel={false}
                    error={false}
                    value={state.test.CustomPageName}
                    onChange={this.onSettingChange.bind(this, "CustomPageName")} />
            </InputGroup>
            <div className="buttons-box">
                <Button
                    disabled={!this.state.test.PageToTest || this.state.test.PageToTest === "-1"}
                    type="secondary"
                    onClick={this.onTestPage.bind(this)}>
                    {resx.get("TestUrlButtonCaption")}
                </Button>
            </div>
        </div>;
        const columnTwo = <div className="right-column">
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("resultingUrlsLabel.Help")}
                    label={resx.get("resultingUrlsLabel")} />
                <MultiLineInput
                    value={props.urls}
                    enabled={false} />
            </InputGroup>
        </div>;

        const columnThree = <div className="left-column">
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("testUrlRewritingLabel.Help")}
                    label={resx.get("testUrlRewritingLabel") + "*"} />
                <SingleLineInputWithError
                    inputStyle={{ margin: "0" }}
                    withLabel={false}
                    error={false}
                    value={state.test.UrlToTest}
                    onChange={this.onSettingChange.bind(this, "UrlToTest")} />
            </InputGroup>
            <div className="buttons-box">
                <Button
                    disabled={!this.state.test.UrlToTest}
                    type="secondary"
                    onClick={this.onTestUrl.bind(this)}>
                    {resx.get("testUrlRewritingButton")}
                </Button>
            </div>
        </div>;

        const columnFour = <div className="right-column">
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("rewritingResultLabel.Help")}
                    label={resx.get("rewritingResultLabel")} />
                <SingleLineInputWithError
                    inputStyle={{ margin: "0" }}
                    withLabel={false}
                    error={false}
                    enabled={false}
                    value={props.rewritingResult} />
            </InputGroup>
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("languageLabel.Help")}
                    label={resx.get("languageLabel")} />
                <SingleLineInputWithError
                    inputStyle={{ margin: "0" }}
                    withLabel={false}
                    error={false}
                    enabled={false}
                    value={props.culture} />
            </InputGroup>
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("identifiedTabLabel.Help")}
                    label={resx.get("identifiedTabLabel")} />
                <SingleLineInputWithError
                    inputStyle={{ margin: "0" }}
                    withLabel={false}
                    error={false}
                    enabled={false}
                    value={props.identifiedPage} />
            </InputGroup>
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("redirectionResultLabel.Help")}
                    label={resx.get("redirectionResultLabel")} />
                <SingleLineInputWithError
                    inputStyle={{ margin: "0" }}
                    withLabel={false}
                    error={false}
                    enabled={false}
                    value={props.redirectionResult} />
            </InputGroup>
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("redirectionReasonLabel.Help")}
                    label={resx.get("redirectionReasonLabel")} />
                <SingleLineInputWithError
                    inputStyle={{ margin: "0" }}
                    withLabel={false}
                    error={false}
                    enabled={false}
                    value={props.redirectionReason} />
            </InputGroup>
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("operationMessagesLabel.Help")}
                    label={resx.get("operationMessagesLabel")} />
                <SingleLineInputWithError
                    inputStyle={{ margin: "0" }}
                    withLabel={false}
                    error={false}
                    enabled={false}
                    value={props.operationMessages} />
            </InputGroup>
        </div>;

        return (
            <div className={styles.testUrl}>
                <div className="columnTitleOne">{resx.get("TestPageUrl")}</div>
                <Grid numberOfColumns={2}>{[columnOne, columnTwo]}</Grid>
                <div className="columnTitleTwo">{resx.get("TestUrlRewriting")}</div>
                <Grid numberOfColumns={2}>{[columnThree, columnFour]}</Grid>
            </div>
        );
    }
}

TestUrlPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    urls: PropTypes.string,
    rewritingResult: PropTypes.string,
    culture: PropTypes.string,
    identifiedPage: PropTypes.string,
    redirectionReason: PropTypes.string,
    redirectionResult: PropTypes.string,
    operationMessages: PropTypes.string
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        urls: state.seo.urls,
        rewritingResult: state.seo.rewritingResult,
        culture: state.seo.culture,
        identifiedPage: state.seo.identifiedPage,
        redirectionReason: state.seo.redirectionReason,
        redirectionResult: state.seo.redirectionResult,
        operationMessages: state.seo.operationMessages
    };
}

export default connect(mapStateToProps)(TestUrlPanelBody);