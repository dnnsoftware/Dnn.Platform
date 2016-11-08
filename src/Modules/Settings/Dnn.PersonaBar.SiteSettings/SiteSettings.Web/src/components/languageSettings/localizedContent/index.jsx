import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    languages as LanguagesActions
} from "../../../actions";
import SocialPanelBody from "dnn-social-panel-body";
import InputGroup from "dnn-input-group";
import Button from "dnn-button";
import resx from "../../../resources";
import "./style.less";

class LanguagePackPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            error: {
                name: false,
                module: true
            },
            triedToSubmit: false
        };
    }

    onEnable(event) {
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <SocialPanelBody
                className="create-language-pack-panel enable-localized-content-panel"
                workSpaceTrayOutside={true}
                workSpaceTray={<div className="siteSettings-back dnn-grid-cell" onClick={props.closePersonaBarPage.bind(this) }>
                    {resx.get("BackToLanguages") }
                </div>}
                workSpaceTrayVisible={true}>
                <div className="languagePack-wrapper">
                    <InputGroup>
                        <div className="help-text-with-background">
                            <p>{resx.get("EnableLocalizedContentHelpText") }</p>
                            <p>{resx.get("EnableLocalizedContentClickCancel") }</p>
                        </div>
                    </InputGroup>
                    <div className="buttons-box">
                        <Button
                            type="secondary"
                            onClick={props.closePersonaBarPage.bind(this) }>
                            {resx.get("Cancel") }
                        </Button>
                        <Button
                            type="primary"
                            onClick={this.onEnable.bind(this) }>
                            {resx.get("Create") }
                        </Button>
                    </div>
                </div>
            </SocialPanelBody>
        );
    }
}

LanguagePackPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    closePersonaBarPage: PropTypes.func,
    languages: PropTypes.array
};

function mapStateToProps(state) {
    return {
        languages: state.languages.languageList
    };
}

export default connect(mapStateToProps)(LanguagePackPanelBody);