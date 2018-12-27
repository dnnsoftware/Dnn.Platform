import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import resx from "../../../resources";
import { BackToLink } from "@dnnsoftware/dnn-react-common";
import util from "utils";
import styles from "./style.less";
import "./style.less";

class HtmlEditorManagerPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            url: undefined
        };
    }

    componentDidMount() {
        this.setState({
            url: util.siteRoot + "Host/HTMLEditorManager?portalid=" + this.props.portalId + "&portpopUp=true"
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        return (
            <div className="dnn-persona-bar-page-body">
                <div className={styles.htmlEditorManager}>
                    <BackToLink onClick={this.props.closeHtmlEditorManager} label={resx.get("BackToSiteBehavior") } />
                    <iframe className="htmlEditorIframe" src={this.state.url} frameBorder="0" />
                </div>
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