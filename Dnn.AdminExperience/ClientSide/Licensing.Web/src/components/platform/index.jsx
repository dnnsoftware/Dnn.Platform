import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { licensing as LicensingActions } from "../../actions";
import resx from "../../resources";
import styles from "./style.module.less";

import DnnTechnologyEditorialIcon from "./../svg/dnn_technology_editorial.svg";
import GithubIcon from "./../svg/github.svg";
import DnnIcon from "./../svg/dnn_logo_primary.svg";
import DocsIcon from "./../svg/dnn_docs_logo.svg";

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
        window.open("https://docs.dnncommunity.org", "_blank");
    }

     
    renderLinks() {
        return (
            <div className="links-wrapper">
                <div className="link-docs-wrapper" title={resx.get("Docs.Header")} onClick={this.onDocsClick.bind(this) }>
                    <div className="link-docs">
                        <div className="docs-icon"><DocsIcon /></div>
                        <div className="link-docs-header">{resx.get("Docs.Header") }</div>
                        <div className="link-docs-desc">{resx.get("Docs") }</div>
                    </div>
                </div>
                <div className="link-community-wrapper">
                    <div className="link-community" title={resx.get("Community.Header")} onClick={this.onCommunityClick.bind(this) }>
                        <div className="dnn-icon"><DnnIcon /></div>
                        <div className="link-community-header">{resx.get("Community.Header") }</div>
                        <div className="link-community-desc">{resx.get("Community") }</div>
                    </div>
                </div>
                <div className="link-github-wrapper">
                    <div className="link-github" title={resx.get("GitHub.Header")} onClick={this.onGitHubClick.bind(this)}>
                        <div className="github-icon"><GithubIcon /></div>
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
                        <div className="dnn-technology-editorial-icon"><DnnTechnologyEditorialIcon /></div>
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