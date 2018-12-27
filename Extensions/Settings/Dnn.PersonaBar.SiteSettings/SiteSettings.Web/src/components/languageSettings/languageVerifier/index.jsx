import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    languages as LanguagesActions
} from "../../../actions";
import LanguageVerifierGrid from "./languageVerifierGrid";
import { PersonaBarPageBody } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import resx from "../../../resources";
import styles from "./style.less";

class LanguageVerifierPanelBody extends Component {
    constructor() {
        super();
    }

    componentDidMount() {

    }

    componentDidUpdate(props) {
        if (props.selectedPage === 2) {
            if (props.verificationResults) {
                this.setState({
                    verificationResults: props.verificationResults
                });
                return;
            }
            props.dispatch(LanguagesActions.verifyLanguageResourceFiles((data) => {
                this.setState({
                    verificationResults: Object.assign({}, data.Results)
                });
            }));
        }
    }

    renderedResults() {
        if (this.props.verificationResults) {
            return this.props.verificationResults.map((item, i) => {
                return (
                    <LanguageVerifierGrid
                        language={item.Language}
                        isDefault={item.IsSystemDefault}
                        icon={item.Icon}
                        missingFiles={item.MissingFiles}
                        filesWithDuplicateEntries={item.FilesWithDuplicateEntries}
                        filesWithMissingEntries={item.FilesWithMissingEntries}
                        filesWithObsoleteEntries={item.FilesWithObsoleteEntries}
                        oldFiles={item.OldFiles}
                        malformedFiles={item.MalformedFiles}
                        key={i}>
                    </LanguageVerifierGrid>
                );
            });
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        return (
            <div className={styles.languageVerifier}>                
                <PersonaBarPageBody backToLinkProps={{
                    text: resx.get("BackToLanguages"),
                    onClick: this.props.closeLanguageVerifier.bind(this)
                }}>
                    {this.renderedResults()}
                </PersonaBarPageBody>
            </div>
        );
    }
}

LanguageVerifierPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    portalId: PropTypes.number,
    closeLanguageVerifier: PropTypes.func,
    verificationResults: PropTypes.array,
    selectedPage: PropTypes.number
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        selectedPage: state.visiblePanel.selectedPage,
        verificationResults: state.languages.verificationResults
    };
}

export default connect(mapStateToProps)(LanguageVerifierPanelBody);