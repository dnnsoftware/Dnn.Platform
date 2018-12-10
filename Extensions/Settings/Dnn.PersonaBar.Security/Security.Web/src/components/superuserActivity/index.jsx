import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    security as SecurityActions
} from "../../actions";
import "./style.less";
import resx from "../../resources";
import styles from "./style.less";

class SuperuserActivityPanelBody extends Component {
    constructor() {
        super();
    }

    UNSAFE_componentWillMount() {
        const {props} = this;
        if (props.activities) {
            return;
        }
        props.dispatch(SecurityActions.getSuperuserActivities());
    }

    renderHeader() {
        const tableFields = [
            { "name": resx.get("Username"), "id": "Username" },
            { "name": resx.get("CreatedDate"), "id": "CreatedDate" },
            { "name": resx.get("LastLogin"), "id": "LastLogin" },
            { "name": resx.get("LastActivityDate"), "id": "LastActivityDate" }
        ];
        let tableHeaders = tableFields.map((field, i) => {
            let className = "activityHeader activityHeader-" + field.id;
            return <div className={className} key={i}>
                <span>{field.name}</span>
            </div>;
        });

        return <div className="activityHeader-wrapper">{tableHeaders}</div>;
    }

    renderedList() {
        const {props} = this;
        return props.activities.map((term, i) => {
            return (
                <div className="activityItem" key={i}>
                    <div className="label-username">
                        <div className="label-wrapper">
                            <span>{term.Username}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-createddate">
                        <div className="label-wrapper">
                            <span>{term.CreatedDate}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-lastlogindate">
                        <div className="label-wrapper">
                            <span>{term.LastLoginDate}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-lastactivitydate">
                        <div className="label-wrapper">
                            <span>{term.LastActivityDate}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        if (props.activities) {
            return (
                <div className={styles.activities}>
                    <div className="activities-topbar">
                        {resx.get("SuperUserActivityExplaination")}
                    </div>
                    <div className="activityItems">
                        {this.renderHeader()}
                        {this.renderedList()}
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

SuperuserActivityPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    activities: PropTypes.object
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        activities: state.security.activities
    };
}

export default connect(mapStateToProps)(SuperuserActivityPanelBody);