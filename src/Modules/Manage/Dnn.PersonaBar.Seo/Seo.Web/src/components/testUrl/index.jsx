import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import { Tab, Tabs, TabList, TabPanel } from "react-tabs";
import {
    pagination as PaginationActions,
    seo as SeoActions
} from "../../actions";
import InputGroup from "dnn-input-group";
import PagePicker from "dnn-page-picker";
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

    componentWillMount() {

    }

    componentWillReceiveProps(props) {

    }

    onSettingChange(key, event) {
        let {state, props} = this;
        let test = Object.assign({}, state.test);

        if (key === "PageToTest") {
            test[key] = event.value;
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

    }

    onTestUrl() {

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
                    label={resx.get("selectPageToTestLabel")}
                    />
                <PagePicker
                    serviceFramework={util.utilities.sf}
                    style={{ width: "100%", zIndex: 2 }}
                    OnSelect={this.onSettingChange.bind(this, "PageToTest")}
                    defaultLabel={noneSpecifiedText}
                    noneSpecifiedText={noneSpecifiedText}
                    CountText={"{0} Results"}
                    PortalTabsParameters={PageToTestParameters}
                    />
            </InputGroup>
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("queryStringLabel.Help")}
                    label={resx.get("queryStringLabel")}
                    />
                <SingleLineInputWithError
                    inputStyle={{ margin: "0" }}
                    withLabel={false}
                    error={false}
                    value={state.test.CustomPageName}
                    onChange={this.onSettingChange.bind(this, "CustomPageName")}
                    />
            </InputGroup>
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("pageNameLabel.Help")}
                    label={resx.get("pageNameLabel")}
                    />
                <SingleLineInputWithError
                    inputStyle={{ margin: "0" }}
                    withLabel={false}
                    error={false}
                    value={state.test.QueryString}
                    onChange={this.onSettingChange.bind(this, "QueryString")}
                    />
            </InputGroup>
            <div className="buttons-box">
                <Button
                    disabled={!this.state.test.PageToTest}
                    type="primary"
                    onClick={this.onTestPage.bind(this)}>
                    {resx.get("TestUrlButtonCaption")}
                </Button>
            </div>
        </div>;
        const columnTwo = <div className="right-column">
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("resultingUrlsLabel.Help")}
                    label={resx.get("resultingUrlsLabel")}
                    />
                <MultiLineInput
                    value={props.pageTestResults}
                    enabled={false}
                    />
            </InputGroup>
        </div>;

        const columnThree = <div className="left-column">
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("testUrlRewritingLabel.Help")}
                    label={resx.get("testUrlRewritingLabel")}
                    />
                <SingleLineInputWithError
                    inputStyle={{ margin: "0" }}
                    withLabel={false}
                    error={false}
                    value={state.test.UrlToTest}
                    onChange={this.onSettingChange.bind(this, "UrlToTest")}
                    />
            </InputGroup>
            <div className="buttons-box">
                <Button
                    disabled={!this.state.test.UrlToTest}
                    type="primary"
                    onClick={this.onTestUrl.bind(this)}>
                    {resx.get("testUrlRewritingButton")}
                </Button>
            </div>
        </div>;

        const columnFour = <div className="right-column">

        </div>;

        return (
            <div className={styles.testUrl}>
                <div className="columnTitleOne">{resx.get("TestPageUrl")}</div>
                <Grid children={[columnOne, columnTwo]} numberOfColumns={2} />
                <div className="columnTitleTwo">{resx.get("TestUrlRewriting")}</div>
                <Grid children={[columnThree, columnFour]} numberOfColumns={2} />
            </div>
        );
    }
}

TestUrlPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    pageTestResults: PropTypes.string
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(TestUrlPanelBody);