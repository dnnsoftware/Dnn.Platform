import React, {PropTypes, Component} from "react";
import { connect } from "react-redux";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Localization from "localization";
import Button from "dnn-button";
import Switch from "dnn-switch";
import CheckBox from "dnn-checkbox";
import { CommonUsersActions } from "../../actions";
import {validateEmail} from "../../helpers";
import utilities from "utils";
import styles from "./style.less";

const inputStyle = { width: "100%" };
const newUserRegistrationDetails = {
    firstName: "",
    lastName: "",
    email: "",
    userName: "",
    password: "",
    randomPassword: false,
    authorize: true,
    notify: false
};

class CreateUserBox extends Component {
    constructor(props) {
        super(props);
        this.state = {
            UserDetails: Object.assign({}, newUserRegistrationDetails),
            confirmPassword: "",
            errors: {
                firstName: false,
                lastName: false,
                userName: false,
                email: false,
                password: false,
                confirmPassword: false,
                passwordsMatch: false
            }
        };
        this.submitted = false;
    }
    onChange(key, item) {
        let {UserDetails} = this.state;
        if (key === "randomPassword" || key === "authorize" || key === "notify") {
            UserDetails[key] = item;
        } else if (key === "confirmPassword") {
            let {confirmPassword} = this.state;
            confirmPassword = item.target.value;
            this.setState({ confirmPassword });
        }
        else {
            UserDetails[key] = item.target.value;
        }
        this.setState({}, () => {
            this.validateForm();
        });
    }
    save() {
        this.submitted = true;
        if (this.validateForm()) {
            this.props.dispatch(CommonUsersActions.createUser(this.state.UserDetails, (data) => {
                if (data.Success) {
                    this.cancel();
                    utilities.notify(Localization.get("UserCreated"));
                }
                else {
                    utilities.notify(data.Message);
                }
            }));
        }
    }
    
    clearForm(callback) {
        let {UserDetails} = this.state;
        UserDetails = Object.assign({}, newUserRegistrationDetails);
        let {errors} = this.state;
        errors.firstName = false;
        errors.lastName = false;
        errors.userName = false;
        errors.email = false;
        errors.password = false;
        errors.confirmPassword = false;
        errors.passwordsMatch = false;
        this.submitted = false;
        this.setState({
            UserDetails,
            errors,
            confirmPassword: ""
        }, () => {
            if (typeof callback === "function") {
                callback();
            }
        });
    }
    cancel() {
        this.clearForm(() => {
            this.props.onCancel();
        });
    }
    validateForm() {
        let valid = true;
        if (this.submitted) {
            let {UserDetails} = this.state;
            let {errors} = this.state;
            errors.firstName = false;
            errors.lastName = false;
            errors.userName = false;
            errors.email = false;
            errors.password = false;
            errors.confirmPassword = false;
            errors.passwordsMatch = false;
            if (UserDetails.firstName === "") {
                errors.firstName = true;
                valid = false;
            }
            if (UserDetails.lastName === "") {
                errors.lastName = true;
                valid = false;
            }
            if (UserDetails.userName === "") {
                errors.userName = true;
                valid = false;
            }
            if (UserDetails.email === "" || !validateEmail(UserDetails.email)) {
                errors.email = true;
                valid = false;
            }
            if (UserDetails.randomPassword === false && UserDetails.password === "") {
                errors.password = true;
                valid = false;
            }
            if (UserDetails.randomPassword === false && this.state.confirmPassword === "") {
                errors.confirmPassword = true;
                valid = false;
            }
            if (UserDetails.randomPassword === false && this.state.confirmPassword !== UserDetails.password) {
                errors.passwordsMatch = true;
                valid = false;
            }
            this.setState({ errors });
        }
        return valid;
    }
    render() {
        const {props, state } = this;
        return (
            <GridCell className={styles.newExtensionModal} style={props.style}>
                <GridCell className="new-user-box">
                    <GridSystem className="with-right-border top-half">
                        <div>
                            <SingleLineInputWithError value={state.UserDetails.firstName}
                                error={state.errors.firstName}
                                onChange={this.onChange.bind(this, "firstName") }
                                label={Localization.get("FirstName") }
                                tooltipMessage={Localization.get("FirstName.Help") }
                                style={inputStyle}
                                inputStyle={{ marginBottom: 25 }} />
                            <SingleLineInputWithError value={state.UserDetails.userName}
                                error={state.errors.userName}
                                onChange={this.onChange.bind(this, "userName") }
                                label={Localization.get("UserName") }
                                tooltipMessage={Localization.get("UserName.Help") }
                                style={inputStyle}
                                inputStyle={{ marginBottom: 25 }}/>
                            <Switch value={state.UserDetails.authorize}
                                label={Localization.get("Approved")} title={Localization.get("Approved.Help")}
                                onChange={this.onChange.bind(this, "authorize") }/>
                        </div>
                        <div>
                            <SingleLineInputWithError value={state.UserDetails.lastName}
                                error={state.errors.lastName}
                                onChange={this.onChange.bind(this, "lastName") }
                                label={Localization.get("LastName") }
                                tooltipMessage={Localization.get("LastName") }
                                style={inputStyle}
                                inputStyle={{ marginBottom: 25 }}/>
                            <SingleLineInputWithError value={state.UserDetails.email}
                                error={state.errors.email}
                                onChange={this.onChange.bind(this, "email") }
                                label={Localization.get("Email") }
                                tooltipMessage={Localization.get("Email.Text") }
                                style={inputStyle}
                                inputStyle={{ marginBottom: 25 }}/>
                            <Switch value={state.UserDetails.randomPassword} title={Localization.get("Random.Help")}
                                label={Localization.get("Random") + ":" }
                                onChange={this.onChange.bind(this, "randomPassword") }/>
                        </div>
                    </GridSystem>
                    {!state.UserDetails.randomPassword && <GridCell><hr/></GridCell>}
                    {!state.UserDetails.randomPassword && <GridSystem>
                        <div>
                            <SingleLineInputWithError label={Localization.get("Password") }
                                error={state.errors.password}
                                onChange={this.onChange.bind(this, "password") }
                                tooltipMessage={Localization.get("Password.Help")}
                                style={inputStyle}
                                inputStyle={{ marginBottom: 15 }}
                                type="password"
                                value={state.UserDetails.password}/>
                        </div>
                        <div>
                            <SingleLineInputWithError label={Localization.get("Confirm") }
                                error={state.errors.confirmPassword || state.errors.passwordsMatch}
                                onChange={this.onChange.bind(this, "confirmPassword") }
                                tooltipMessage={state.errors.confirmPassword ? Localization.get("Confirm.Help") : Localization.get("ConfirmMismatch.Help") }
                                style={inputStyle}
                                type="password"
                                inputStyle={{ marginBottom: 15 }}
                                value={state.confirmPassword}/></div>
                    </GridSystem>
                    }
                    <GridCell columnSize={100} className="email-notification-line">
                        <CheckBox value={state.UserDetails.notify}
                            label={Localization.get("Notify")}
                            onChange={this.onChange.bind(this, "notify") }/>
                    </GridCell>
                    <GridCell columnSize={100} className="modal-footer">
                        <Button id="cancelbtn"  type="secondary" onClick={this.cancel.bind(this) }>{Localization.get("btnCancel") }</Button>
                        <Button id="confirmbtn" type="primary" onClick={this.save.bind(this) }>{Localization.get("btnSave") }</Button>
                    </GridCell>
                </GridCell>
            </GridCell>
        );
    }
}

CreateUserBox.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,
    style: PropTypes.object
};

function mapStateToProps(state) {
    return {
    };
}

export default connect(mapStateToProps)(CreateUserBox);