import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import { Tab, Tabs, TabList, TabPanel } from "react-tabs";
import Button from "dnn-button";
import "./style.less";
import util from "../../../utils";
import resx from "../../../resources";
import styles from "./style.less";

class HtmlEditorManagerPanelBody extends Component {
    constructor() {
        super();
    }

    getIframeUrl(){
        return "/Host/HTMLEditorManager?portalid=" + this.props.portalId + "&portpopUp=true";
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <div className={styles.htmlEditorManager}>
                <div className="htmlEditor-back" onClick={this.props.closeHtmlEditorManager.bind(this)}>{resx.get("BackToSiteBehavior")}</div>
                <iframe className="htmlEditorIframe" src={this.getIframeUrl()} frameBorder="0" />
            </div>
        );
    }
}

HtmlEditorManagerPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    portalId: PropTypes.number,
    closeHtmlEditorManager: PropTypes.func
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(HtmlEditorManagerPanelBody);