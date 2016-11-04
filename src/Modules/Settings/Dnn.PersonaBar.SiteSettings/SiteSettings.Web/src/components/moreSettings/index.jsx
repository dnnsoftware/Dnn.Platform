import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import { Tab, Tabs, TabList, TabPanel } from "react-tabs";
import {
    pagination as PaginationActions
} from "../../actions";
import Button from "dnn-button";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

class MoreSettingsPanelBody extends Component {
    constructor() {
        super();
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <div className={styles.moreSettings}>
                <div className="sectionTitle">{resx.get("HtmlEditor")}</div>
                <div className="htmlEditorWrapper">
                    <div className="htmlEditorWrapper-left">
                        <div className="htmlEditorWarning">{resx.get("HtmlEditorWarning")}</div>
                    </div>
                    <div className="htmlEditorWrapper-right">
                        <Button
                            type="secondary"
                            onClick={props.openHtmlEditorManager.bind(this)}>
                            {resx.get("OpenHtmlEditor")}
                        </Button>
                    </div>
                </div>
            </div>
        );
    }
}

MoreSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    portalId: PropTypes.number,
    openHtmlEditorManager: PropTypes.func
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(MoreSettingsPanelBody);