import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import {users as UserActions} from "../../../../actions";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import date from "../../../../utils/date";
import util from "../../../../utils";
import Button from "dnn-button";
import styles from "./style.less";
import ChangePassword from "../ChangePassword";

const inputStyle = { width: "100%" };
const blankAccountSettings = {
    userId: 0,
    displayName: "",
    userName: "",
    email: ""
};

class UserSettings extends Component {
    constructor(props) {
        super(props);
        this.state = {
            accountSettings: blankAccountSettings,
            userDetails: props.userDetails,
            errors: {
                displayName: false,
                userName: false,
                email: false
            },
            reload: false,
            ChangePasswordVisible: false
        };
    }
    componentWillMount() {
        let {props} = this;
        if (props.userDetails === undefined || props.userDetails.userId !== props.userId) {
            this.getUserDetails(props);
        }
    }
    componentWillReceiveProps(newProps) {
        if (newProps.userDetails === undefined && newProps.userDetails.userId !== newProps.userId || this.state.reload) {
            this.getUserDetails(newProps);
        }
    }
    getUserDetails(props) {
        props.dispatch(UserActions.getUserDetails({ userId: props.userId }, (data) => {
            let userDetails = Object.assign({}, data);
            let {accountSettings} = this.state;
            accountSettings.displayName = userDetails.displayName;
            accountSettings.userName = userDetails.userName;
            accountSettings.email = userDetails.email;
            accountSettings.userId = userDetails.userId;
            this.setState({
                accountSettings,
                userDetails,
                reload: false
            });
        }));
    }
    onChange(key, item) {
        let {accountSettings} = this.state;
        accountSettings[key] = item.target.value;
        this.setState({ accountSettings }, () => {
            this.validateForm(true);
        });
    }
    save() {
        if (this.validateForm()) {
            this.props.dispatch(UserActions.updateUserBasicInfo(this.state.accountSettings, (data) => {
                if (data.Success) {
                    console.log('ss');
                    util.utilities.notify("User updated successfully.");
                    let {reload} = this.state;
                    reload = true;
                    this.setState({ reload });
                    this.props.collapse();
                }
                else {
                    util.utilities.notify(data.Message);
                }
            }));
        }
    }
    validateEmail(value) {
        const re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(value);
    }
    validateForm() {
        let valid = true;
        let {errors} = this.state;
        errors.displayName = false;
        errors.userName = false;
        errors.email = false;
        let {accountSettings} = this.state;
        if (accountSettings.displayName === "") {
            errors.displayName = true;
            valid = false;
        }
        if (accountSettings.userName === "") {
            errors.userName = true;
            valid = false;
        }
        if (accountSettings.email === "" || !this.validateEmail(accountSettings.email)) {
            errors.email = true;
            valid = false;
        }
        this.setState({ errors });

        return valid;
    }
    onCancelPassword() {
        this.setState({
            ChangePasswordVisible: false
        });
    }
    onChangePassword() {
        this.setState({
            ChangePasswordVisible: true
        });
    }
    onForcePasswordChange() {
        this.props.dispatch(UserActions.forceChangePassword({ userId: this.props.userId }, (data) => {

            if (data.Success) {
                util.utilities.notify("User must update Password on next login.");
                let {userDetails} = this.state;
                userDetails.needUpdatePassword = true;
                this.setState({ userDetails });
            }
            else {
                util.utilities.notify(data.Message);
            }
        }));
    }
    onSendPasswordLink() {
        this.props.dispatch(UserActions.sendPasswordResetLink({ userId: this.props.userId }, (data) => {
            if (data.Success)
                util.utilities.notify("Password reset link sent successfully");
            else {
                util.utilities.notify(data.Message);
            }
        }));
    }
    render() {
        let {state} = this;
        return <GridCell className={styles.userSettings}>
            <GridSystem>
                <GridCell className="left">
                    <ChangePassword visible={this.state.ChangePasswordVisible} onCancel={this.onCancelPassword.bind(this) } userId={this.props.userId} />
                    <GridCell className="edit-details-box">
                        <div className="title">
                            Account Settings
                        </div>
                        <SingleLineInputWithError value={state.accountSettings.userName}
                            error={state.errors.userName}
                            onChange={this.onChange.bind(this, "userName") }
                            label={Localization.get("label_UserName") }
                            tooltipMessage="Please enter username."
                            style={inputStyle}
                            inputStyle={{ marginBottom: 25 }}/>
                        <SingleLineInputWithError value={state.accountSettings.displayName}
                            error={state.errors.displayName}
                            onChange={this.onChange.bind(this, "displayName") }
                            label={Localization.get("label_DisplayName") }
                            tooltipMessage="Please enter display name."
                            style={inputStyle}
                            inputStyle={{ marginBottom: 25 }} />
                        <SingleLineInputWithError value={state.accountSettings.email}
                            error={state.errors.email}
                            onChange={this.onChange.bind(this, "email") }
                            label={Localization.get("label_EmailAddress") }
                            tooltipMessage="Please enter email."
                            style={inputStyle}
                            inputStyle={{ marginBottom: 25 }}/>
                    </GridCell>
                    <GridCell>
                        <div className="title">
                            Password Management
                        </div>
                        <GridCell className="link">
                            <div onClick={this.onChangePassword.bind(this) }>[Change Password]
                            </div></GridCell>
                        {!state.userDetails.needUpdatePassword && <GridCell className="link">
                            <div onClick={this.onForcePasswordChange.bind(this) }>[Force Change Password]
                            </div></GridCell>}
                        <GridCell className="link">
                            <div onClick={this.onSendPasswordLink.bind(this) }>[Send Password Reset Link]
                            </div></GridCell>
                    </GridCell>
                </GridCell>
                <GridCell className="right">
                    <div className="title">
                        Account Data
                    </div>
                    <GridSystem className="first">
                        <GridCell>
                            Created Date:
                        </GridCell>
                        <GridCell>
                            {date.format(state.userDetails.createdOnDate, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell>
                            Last Login Date:
                        </GridCell>
                        <GridCell>
                            {date.format(state.userDetails.lastLogin, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell>
                            Last Activity Date:
                        </GridCell>
                        <GridCell>
                            {date.format(state.userDetails.lastActivity, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell>
                            Last Password Change:
                        </GridCell>
                        <GridCell>
                            {date.format(state.userDetails.lastPasswordChange, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell>
                            Last Lockout Date:
                        </GridCell>
                        <GridCell>
                            {date.format(state.userDetails.lastLockout, true) === "-" ? "Never" : date.format(state.userDetails.lastLockout, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell>
                            User Is Online:
                        </GridCell>
                        <GridCell>
                            {state.userDetails.isOnline ? "True" : "False"}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell>
                            Locked Out:
                        </GridCell>
                        <GridCell>
                            {state.userDetails.isLocked ? "True" : "False"}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell>
                            Authorized:
                        </GridCell>
                        <GridCell>
                            {state.userDetails.authorized ? "True" : "False"}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell>
                            Update Password:
                        </GridCell>
                        <GridCell>
                            {state.userDetails.needUpdatePassword ? "True" : "False"}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell>
                            Deleted:
                        </GridCell>
                        <GridCell>
                            {state.userDetails.isDeleted ? "True" : "False"}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell>
                            User Folder:
                        </GridCell>
                        <GridCell>
                            {state.userDetails.userFolder}
                        </GridCell>
                    </GridSystem>
                </GridCell>
            </GridSystem>
            <GridCell className="buttons">
                <GridCell columnSize={50} className="leftBtn">
                    <Button id="cancelbtn"  type="secondary" onClick={this.props.collapse.bind(this) }>{Localization.get("btn_Cancel") }</Button>
                </GridCell>
                <GridCell columnSize={50} className="rightBtn">
                    <Button id="confirmbtn" type="primary" onClick={this.save.bind(this) }>{Localization.get("btn_Save") }</Button>
                </GridCell>
            </GridCell>
        </GridCell>;
    }
}
UserSettings.propTypes = {
    dispatch: PropTypes.func.isRequired,
    userId: PropTypes.array.isRequired,
    collapse: PropTypes.func.isRequired,
    userDetails: PropTypes.object
};
function mapStateToProps(state) {
    return {
        userDetails: state.users.userDetails
    };
}

export default connect(mapStateToProps)(UserSettings);