import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    licensing as LicensingActions
} from "../../actions";
import "./style.less";
import resx from "../../resources";
import styles from "./style.less";

/*eslint-disable quotes*/
const dnnTechnologyEditorialIcon = require(`!raw-loader!./../svg/dnn_technology_editorial.svg`).default;
const githubIcon = require(`!raw-loader!./../svg/github.svg`).default;
const dnnIcon = require(`!raw-loader!./../svg/dnn_logo_primary.svg`).default;
const docsIcon = require(`!raw-loader!./../svg/dnn_docs_logo.svg`).default;

class Platform extends Component {
    constructor() {
        super();
    }

    renderVersion() {
        return <div className="intro-header-row">{this.props.productVersion}</div>;
    }

    onGitHubClick() {
        window.open("https://github.com/dnnsoftware/Dnn.Platform", "_blank");
    }

    onCommunityClick() {
        window.open("https://dnncommunity.org", "_blank");
    }

    onDocsClick() {
        window.open("https://dnndocs.com", "_blank");
    }

    /* eslint-disable react/no-danger */
    renderLinks() {
        return (
            <div className="links-wrapper">
                <div className="link-docs-wrapper" title={resx.get("Docs.Header")} onClick={this.onDocsClick.bind(this) }>
                    <div className="link-docs">
                        <div className="docs-icon" dangerouslySetInnerHTML={{ __html: docsIcon }} />
                        <div className="link-docs-header">{resx.get("Docs.Header") }</div>
                        <div className="link-docs-desc">{resx.get("Docs") }</div>
                    </div>
                </div>
                <div className="link-community-wrapper">
                    <div className="link-community" title={resx.get("Community.Header")} onClick={this.onCommunityClick.bind(this) }>
                        <div className="dnn-icon" dangerouslySetInnerHTML={{ __html: dnnIcon }} />
                        <div className="link-community-header">{resx.get("Community.Header") }</div>
                        <div className="link-community-desc">{resx.get("Community") }</div>
                    </div>
                </div>
                <div className="link-github-wrapper">
                    <div className="link-github" title={resx.get("GitHub.Header")} onClick={this.onGitHubClick.bind(this)}>
                        <div className="github-icon" dangerouslySetInnerHTML={{ __html: githubIcon }} />
                        <div className="link-github-header">{resx.get("GitHub.Header")}</div>
                        <div className="link-github-desc">{resx.get("GitHub")}</div>
                    </div>
                </div>
            </div>
        );
    }

    componentDidMount() {
        const {props} = this;
        props.dispatch(LicensingActions.getServerInfo());
    }

    /*eslint no-mixed-spaces-and-tabs: "error"*/
    render() {
        return (
            <div className={styles.licensingPlatform}>
                <div>
                    {this.renderVersion()}
                    <div className="intro">
                        <div className="dnn-technology-editorial-icon" dangerouslySetInnerHTML={{ __html: dnnTechnologyEditorialIcon }} />
                        <div className="intro-header">{resx.get("Intro.Header") }</div>
                        <div className="intro-body">{resx.get("Intro") }</div>
                    </div>
                </div>
                {this.renderLinks() }
            </div>
        );
    }
}

Platform.propTypes = {
    dispatch: PropTypes.func.isRequired,
    productVersion: PropTypes.string
};

function mapStateToProps(state) {
    return {
        productVersion: state.licensing.productVersion
    };
}

export default connect(mapStateToProps)(Platform);