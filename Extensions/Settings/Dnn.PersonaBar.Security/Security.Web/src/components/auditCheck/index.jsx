/* eslint-disable react/no-danger */
import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    security as SecurityActions
} from "../../actions";
import "./style.less";
import ContentLoadWrapper from "dnn-content-load-wrapper";
import { TableEmptyState } from "dnn-svg-icons";
import resx from "../../resources";
import styles from "./style.less";

class AuditCheckPanelBody extends Component {
    constructor() {
        super();

        this.state = {
            checking: false
        };
    }

    UNSAFE_componentWillMount() {
        const {props} = this;
        if (props.auditCheckResults) {
            return;
        }
        props.dispatch(SecurityActions.getAuditCheckResults());
    }

    renderHeader() {
        const tableFields = [
            { "name": resx.get("SecurityCheck"), "id": "SecurityCheck" },
            { "name": resx.get("Result"), "id": "Result" },
            { "name": resx.get("Notes"), "id": "Notes" }
        ];
        let tableHeaders = tableFields.map((field, i) => {
            let className = "auditCheckHeader auditCheckHeader-" + field.id;
            return <div className={className} key={i}>
                <span>{field.name}</span>
            </div>;
        });

        return <div className="auditCheckHeader-wrapper">{tableHeaders}</div>;
    }

    getResultDisplay(result) {
        const severity = result.Severity;
        const successText = result.SuccessText;
        const failureText = result.FailureText;
        const reason = result.Reason;
        
        switch (severity) {
            case 0:
                return (
                    <div className="label-result-severity">
                        <div className="label-result-severity-pass">
                            {resx.get("Pass")}
                        </div>
                        <div>
                            {successText}
                        </div>
                    </div>
                );
            case 1:
                return (
                    <div className="label-result-severity">
                        <div className="label-result-severity-alert">
                            {resx.get("Alert")}
                        </div>
                        <div>
                            {failureText}
                        </div>
                    </div>
                );
            case 2:
                return (
                    <div className="label-result-severity">
                        <div className="label-result-severity-fail">
                            {resx.get("Fail")}
                        </div>
                        <div dangerouslySetInnerHTML={{__html: failureText}}>
                        </div>
                    </div>
                );
            default:
                return (
                    <div className="label-result-severity">
                        <div className="label-result-severity-unverified" onClick={this.onAuditCheck.bind(this, result)}>
                            {resx.get("Check")}
                        </div>
                        <div>
                            {reason}
                        </div>
                    </div>
                );
        }
    }

    onAuditCheck(result) {
        const {state, props} = this;
        if (state.checking) {
            return;
        }

        this.setState({checking: true}, () => {
            props.dispatch(SecurityActions.getAuditCheckResult(result.CheckName, () => {
                this.setState({checking: false});
            }));
        });
    }

    /* eslint-disable react/no-danger */
    getNotesDisplay(notes) {
        if (notes && notes.length > 0) {
            return <div className="log-detail" dangerouslySetInnerHTML={{ __html: notes }}></div>;
        }
        else {
            return "N/A";
        }
    }

    renderedList() {
        const {props} = this;
        return props.auditCheckResults.map((term, i) => {
            return (
                <div className="auditCheckItem" key={i}>
                    <div className="label-name">
                        <div className="label-wrapper">
                            <span>{term.CheckNameText}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-result">
                        <div className="label-wrapper">
                            <span>{this.getResultDisplay(term)}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-notes">
                        <div className="label-wrapper">
                            <span>{this.getNotesDisplay(term.Notes)}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        let contentShouldShow = (props.auditCheckResults && props.auditCheckResults.length > 0) ? true : false;
        return (
            <ContentLoadWrapper loadComplete={contentShouldShow}
                svgSkeleton={<div dangerouslySetInnerHTML={{ __html: TableEmptyState }} />}>
                <div className={styles.auditCheckResults}>
                    <div className="auditcheck-topbar">
                        {resx.get("AuditExplanation")}
                    </div>
                    <div className="auditCheckItems">
                        {contentShouldShow && this.renderHeader()}
                        {contentShouldShow && this.renderedList()}
                    </div>
                </div>
            </ContentLoadWrapper>
        );

    }
}

AuditCheckPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    auditCheckResults: PropTypes.object
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        auditCheckResults: state.security.auditCheckResults
    };
}

export default connect(mapStateToProps)(AuditCheckPanelBody);
