import React, {Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import { GridCell, DatePicker }  from "@dnnsoftware/dnn-react-common";
import IconButton from "../../../common/IconButton";
import util from "../../../../utils";
import resx from "../../../../resources";
import {
    roleUsers as RoleUsersActions
} from "../../../../actions";

class UserRow extends Component {

    constructor() {
        super();
        this.state = {
            userSelectState: { userId: -1, keyword: "" },
            currentPage: 0,
            pageSize: 10,
            usersKeyword: "",
            editIndex: -1,
            editCommand: "",
            isCalendarVisible: false
        };
    }

    formateDate(dateValue) {
        let date = new Date(dateValue);

        let dayValue = date.getDate(),
            monthValue = date.getMonth() + 1,
            yearValue = date.getFullYear();

        if (yearValue < 1900) {
            return "-";
        }

        return monthValue + "/" + dayValue + "/" + yearValue;
    }
    onStartTimeClick(userRole, index) {
        this.setState({ editIndex: index, editCommand: "startTime", isCalendarVisible: true });
    }

    onExpiresTimeClick(userRole, index) {
        this.setState({ editIndex: index, editCommand: "expiresTime", isCalendarVisible: true });
    }

    onDeleteClick(userRole) {
        const {props} = this;
        util.utilities.confirm(resx.get("DeleteUser.Confirm"), resx.get("Delete"), resx.get("Cancel"), () => {
            props.dispatch(RoleUsersActions.removeUserFromRole(userRole));
        });
    }
    isEmptyDate(date) {
        return !date || new Date(date).getFullYear() < 1970;
    }

    onChange(userRole, command, FirstDate) {
        const {state} = this;
        state.editIndex = -1;
        state.editCommand = "";
        let startTime = command === "startTime" ? FirstDate : userRole.startTime;
        let expiresTime = command === "expiresTime" ? FirstDate : userRole.expiresTime;

        this.props.saveUser(userRole.userId, startTime, expiresTime);
        this.setState({ isCalendarVisible: false });
    }
    getBoundDate(userRole, command) {
        if (command === "startTime") {
            let maxValue = new Date(2049, 11, 31);
            if (!this.isEmptyDate(userRole.expiresTime)) {
                maxValue = new Date(new Date().setTime(new Date(userRole.expiresTime).getTime() - 1 * 86400000));
            }
            return maxValue;
        } else if (command === "expiresTime") {
            let minValue = new Date(1970, 0, 1);
            if (!this.isEmptyDate(userRole.startTime)) {
                minValue = new Date(new Date().setTime(new Date(userRole.startTime).getTime() + 1 * 86400000));
            }
            return minValue;
        }
    }
    getDate(userDetails, command) {
        let dateValue = new Date();
        if (command === "startTime") {
            if (!this.isEmptyDate(userDetails.startTime)) {
                dateValue = new Date(userDetails.startTime);
            }

        } else if (command === "expiresTime") {
            if (!this.isEmptyDate(userDetails.expiresTime)) {
                dateValue = new Date(userDetails.expiresTime);
            }
        }
        return dateValue;
    }
    createUserActions() {
        const {props, state} = this;

        let startTimeAction = props.userDetails.allowExpired ? <span>
            <DatePicker  date={this.getDate(props.userDetails, "startTime") } maxDate={this.getBoundDate(props.userDetails, "startTime") }
                updateDate={this.onChange.bind(this, props.userDetails, "startTime") } mode={"start"} applyButtonText={resx.get("Apply") }
                showIcon={true} showInput={false}
                onIconClick={this.onStartTimeClick.bind(this, props.userDetails, props.index) }             />
        </span> : null;
        let expiresTimeAction = props.userDetails.allowExpired ? <span>
            <DatePicker  date={this.getDate(props.userDetails, "expiresTime") } minDate={this.getBoundDate(props.userDetails, "expiresTime") }
                updateDate={this.onChange.bind(this, props.userDetails, "expiresTime") } mode={"end"} applyButtonText={resx.get("Apply") }
                showIcon={true} showInput={false}
                onIconClick={this.onExpiresTimeClick.bind(this, props.userDetails, props.index) }             />
        </span> : null;
        let deleteAction = props.userDetails.allowDelete ? <IconButton type="x" width={17} onClick={this.onDeleteClick.bind(this, props.userDetails, props.index) } /> : null;
        return <div className={state.editIndex === props.index ? "edit-row" : null}>
            {deleteAction}
            {expiresTimeAction}
            {startTimeAction}
        </div>;
    }
    render() {
        const {props} = this;
        return (
            <div className="role-user-row">
                <GridCell title={props.roleName} columnSize={25} >{props.userDetails.displayName}</GridCell>
                <GridCell  columnSize={20} >
                    {this.formateDate(props.userDetails.startTime) }</GridCell>
                <GridCell  columnSize={20} >
                    {this.formateDate(props.userDetails.expiresTime) }</GridCell>
                <GridCell  columnSize={35} >
                    <div className="actions">
                        {this.createUserActions() }
                    </div>
                </GridCell>
            </div>
        );
    }
}
UserRow.propTypes = {
    userDetails: PropTypes.object.isRequired,
    index: PropTypes.number,
    saveUser: PropTypes.func.isRequired,
    deleteUser: PropTypes.func.isRequired
};
UserRow.defaultProps = {
    roleUsersList: []
};
function mapStateToProps() {
    return {};
}


export default connect(mapStateToProps)(UserRow);