import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import { CommonUsersActions } from "../../../actions";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import {formatDate, validateEmail} from "../../../helpers";
import utilities from "utils";
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
        else
        {
            let userDetails = Object.assign({}, props.userDetails);
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
        }
    }
    componentWillReceiveProps(newProps) {
        if (newProps.userDetails === undefined && newProps.userDetails.userId !== newProps.userId || this.state.reload) {
            this.getUserDetails(newProps);
        }
        else
        {
            let userDetails = Object.assign({}, newProps.userDetails);
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
        }
    }
    getUserDetails(props) {
        props.dispatch(CommonUsersActions.getUserDetails({ userId: props.userId }, (data) => {
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
            this.props.dispatch(CommonUsersActions.updateUserBasicInfo(this.state.accountSettings, (data) => {
                if (data.Success) {
                    utilities.notify(Localization.get("UserUpdated"));
                    let {reload} = this.state;
                    reload = true;
                    this.setState({ reload });
                    this.props.collapse();
                }
                else {
                    utilities.notify(data.Message);
                }
            }));
        }
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
        if (accountSettings.email === "" || !validateEmail(accountSettings.email)) {
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
        this.props.dispatch(CommonUsersActions.forceChangePassword({ userId: this.props.userId }, (data) => {
            if (data.Success) {
                utilities.notify(Localization.get("UserPasswordUpdateChanged"));
                let {userDetails} = this.state;
                userDetails.needUpdatePassword = true;
                this.setState({ userDetails });
            }
            else {
                utilities.notify(data.Message);
            }
        }));
    }
    onSendPasswordLink() {
        this.props.dispatch(CommonUsersActions.sendPasswordResetLink({ userId: this.props.userId }, (data) => {
            if (data.Success)
                utilities.notify(Localization.get("PasswordSent"));
            else {
                utilities.notify(data.Message);
            }
        }));
    }
    render() {
        let {state} = this;
        return <GridCell className={styles.userSettings}>
            <GridCell>
                <GridCell className="outer-box" columnSize={50}>
                    <ChangePassword visible={this.state.ChangePasswordVisible} onCancel={this.onCancelPassword.bind(this) } userId={this.props.userId} />
                    <div className="title">
                        {Localization.get("AccountSettings")}
                    </div>
                    <div>
                        <SingleLineInputWithError value={state.accountSettings.userName}
                            error={state.errors.userName}
                            onChange={this.onChange.bind(this, "userName") }
                            label={Localization.get("Username") }
                            tooltipMessage={Localization.get("Username.Help")}
                            errorMessage={Localization.get("Username.Required") }
                            style={inputStyle}
                            autoComplete="off"
                            inputStyle={{ marginBottom: 25 }}/>
                        <SingleLineInputWithError value={state.accountSettings.displayName}
                            error={state.errors.displayName}
                            onChange={this.onChange.bind(this, "displayName") }
                            label={Localization.get("DisplayName") }
                            tooltipMessage={Localization.get("DisplayName.Help")}
                            errorMessage={Localization.get("DisplayName.Required") }
                            style={inputStyle}
                            autoComplete="off"
                            inputStyle={{ marginBottom: 25 }} />
                        <SingleLineInputWithError value={state.accountSettings.email}
                            error={state.errors.email}
                            onChange={this.onChange.bind(this, "email") }
                            label={Localization.get("Email") }
                            tooltipMessage={Localization.get("Email.Help")}
                            errorMessage={Localization.get("Email.Required") }
                            style={inputStyle}
                            autoComplete="off"
                            inputStyle={{ marginBottom: 25 }}/>
                    </div>
                    <GridCell className="no-padding">
                        <div className="title">
                            {Localization.get("PasswordManagement")}
                        </div>
                        <GridCell className="link">
                            <div onClick={this.onChangePassword.bind(this) }>[ {Localization.get("ChangePassword")} ]
                            </div>
                            </GridCell>
                        {!state.userDetails.needUpdatePassword && <GridCell className="link">
                            <div onClick={this.onForcePasswordChange.bind(this) }>[ {Localization.get("ForceChangePassword")} ]
                            </div>
                            </GridCell>}
                        <GridCell className="link">
                            <div onClick={this.onSendPasswordLink.bind(this) }>[ {Localization.get("ResetPassword")} ]
                            </div>
                            </GridCell>
                    </GridCell>
                </GridCell>
                <GridCell className="outer-box right" columnSize={50}>
                    <div className="title">
                        Account Data
                    </div>
                    <GridSystem className="first">
                        <GridCell  title={Localization.get("CreatedDate.Help")}>
                            {Localization.get("CreatedDate")}
                        </GridCell>
                        <GridCell>
                            {formatDate(state.userDetails.createdOnDate, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("LastLoginDate.Help")}>
                            {Localization.get("LastLoginDate")}
                        </GridCell>
                        <GridCell>
                            {formatDate(state.userDetails.lastLogin, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("LastActivityDate.Help")}>
                            {Localization.get("LastActivityDate")}
                        </GridCell>
                        <GridCell>
                            {formatDate(state.userDetails.lastActivity, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("LastPasswordChangeDate.Help")}>
                            {Localization.get("LastPasswordChangeDate")}
                        </GridCell>
                        <GridCell>
                            {formatDate(state.userDetails.lastPasswordChange, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("LastLockoutDate.Help")}>
                            {Localization.get("LastLockoutDate")}
                        </GridCell>
                        <GridCell>
                            {formatDate(state.userDetails.lastLockout, true) === "-" ? "Never" : formatDate(state.userDetails.lastLockout, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("IsOnLine.Help")}>
                            {Localization.get("IsOnLine")}
                        </GridCell>
                        <GridCell>
                            {state.userDetails.isOnline ? "True" : "False"}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("LockedOut.Help")}>
                            {Localization.get("LockedOut")}
                        </GridCell>
                        <GridCell>
                            {state.userDetails.isLocked ? "True" : "False"}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("Approved.Help")}>
                            {Localization.get("Approved")}
                        </GridCell>
                        <GridCell>
                            {state.userDetails.authorized ? "True" : "False"}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("UpdatePassword.Help")}>
                            {Localization.get("UpdatePassword")}
                        </GridCell>
                        <GridCell>
                            {state.userDetails.needUpdatePassword ? "True" : "False"}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("IsDeleted.Help")}>
                            {Localization.get("IsDeleted")}
                        </GridCell>
                        <GridCell>
                            {state.userDetails.isDeleted ? "True" : "False"}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("UserFolder.Help")}>
                            {Localization.get("UserFolder")}
                        </GridCell>
                        <GridCell>
                            {state.userDetails.userFolder}
                        </GridCell>
                    </GridSystem>
                </GridCell>
            </GridCell>
            <GridCell className="buttons">
                <GridCell columnSize={50} className="leftBtn">
                    <Button id="cancelbtn"  type="secondary" onClick={this.props.collapse.bind(this) }>{Localization.get("btnCancel") }</Button>
                </GridCell>
                <GridCell columnSize={50} className="rightBtn">
                    <Button id="confirmbtn" type="primary" onClick={this.save.bind(this) }>{Localization.get("btnSave") }</Button>
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