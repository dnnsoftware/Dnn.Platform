import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Localization from "localization";
import { CommonUsersActions } from "../../actions";
import {validateEmail} from "../../helpers";
import utilities from "utils";
import styles from "./style.less";
import Password from "../common/Password";
import { GridCell, GridSystem, SingleLineInputWithError, Button, Switch, Checkbox } from "@dnnsoftware/dnn-react-common";


const inputStyle = { width: "100%" };
const newUserRegistrationDetails = {
    firstName: "",
    lastName: "",
    email: "",
    userName: "",
    password: "",
    question: "",
    answer: "",
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
                passwordsMatch: false,
                question: false,
                answer: false
            }
        };
        this.submitted = false;
    }

    onChangePassword(event) {
        this.setState({
            UserDetails: Object.assign(this.state.UserDetails, {password : event.target.value}),
            errors: Object.assign(this.state.errors, {password:false})
        });
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
            this.props.save(CommonUsersActions.createUser(this.state.UserDetails, this.props.filter, () => {
                this.cancel();
                utilities.notify(Localization.get("UserCreated"), 3000);
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
        errors.question = false;
        errors.answer = false;
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
        const {props } = this;

        let valid = true;
        let requiresQuestionAndAnswer = props.appSettings.applicationSettings.settings.requiresQuestionAndAnswer;
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
            errors.question = false;
            errors.answer = false;
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
            else if (UserDetails.randomPassword === false && this.state.confirmPassword !== UserDetails.password) {
                errors.passwordsMatch = true;
                valid = false;
            }

            if (requiresQuestionAndAnswer) {
                if (UserDetails.question === "") {
                    errors.question = true;
                    valid = false;
                }
                if (UserDetails.answer === "") {
                    errors.answer = true;
                    valid = false;
                }
            }

            this.setState({ errors });
        }
        return valid;
    }
    render() {
        const {props, state } = this;
        let requiresQuestionAndAnswer = props.appSettings.applicationSettings.settings.requiresQuestionAndAnswer;
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
                                errorMessage={Localization.get("FirstName.Required") }
                                style={inputStyle}
                                autoComplete="off"
                                inputStyle={{ marginBottom: 25 }} tabIndex={1}/>
                            <SingleLineInputWithError value={state.UserDetails.userName}
                                error={state.errors.userName}
                                onChange={this.onChange.bind(this, "userName") }
                                label={Localization.get("Username") }
                                tooltipMessage={Localization.get("Username.Help") }
                                errorMessage={Localization.get("Username.Required") }
                                style={inputStyle}
                                autoComplete="off"
                                inputStyle={{ marginBottom: 25 }}  tabIndex={3}/>
                            <Switch value={state.UserDetails.authorize}
                                label={Localization.get("Approved")} title={Localization.get("Approved.Help")}
                                onChange={this.onChange.bind(this, "authorize") }  tabIndex={5}
                                onText={Localization.get("SwitchOn")} offText={Localization.get("SwitchOff")}/>
                        </div>
                        <div>
                            <SingleLineInputWithError value={state.UserDetails.lastName}
                                error={state.errors.lastName}
                                onChange={this.onChange.bind(this, "lastName") }
                                label={Localization.get("LastName") }
                                tooltipMessage={Localization.get("LastName.Help") }
                                errorMessage={Localization.get("LastName.Required") }
                                style={inputStyle}
                                autoComplete="off"
                                inputStyle={{ marginBottom: 25 }}  tabIndex={2}/>
                            <SingleLineInputWithError value={state.UserDetails.email}
                                error={state.errors.email}
                                onChange={this.onChange.bind(this, "email") }
                                label={Localization.get("Email") }
                                tooltipMessage={Localization.get("Email.Help") }
                                errorMessage={Localization.get("Email.Required") }
                                style={inputStyle}
                                autoComplete="off"
                                inputStyle={{ marginBottom: 25 }}  tabIndex={4}/>
                            <Switch value={state.UserDetails.randomPassword} title={Localization.get("Random.Help")}
                                label={Localization.get("Random") + ":" }
                                onChange={this.onChange.bind(this, "randomPassword") }  tabIndex={6}
                                onText={Localization.get("SwitchOn")} offText={Localization.get("SwitchOff")}/>
                        </div>
                    </GridSystem>
                    {!state.UserDetails.randomPassword && <GridCell><hr/></GridCell>}
                    {!state.UserDetails.randomPassword && <GridSystem>


                            <Password 
                                error={state.errors} 
                                onChangePassword={this.onChangePassword.bind(this)} 
                                style={inputStyle} 
                                inputStyle={!requiresQuestionAndAnswer ? { marginBottom: 15 } : { marginBottom: 0 }}
                                UserDetails={this.state.UserDetails}
                            />
                           
                            <SingleLineInputWithError label={Localization.get("Confirm") }
                                error={state.errors.confirmPassword || state.errors.passwordsMatch}
                                onChange={this.onChange.bind(this,"confirmPassword") }
                                tooltipMessage={Localization.get("Confirm.Help")}
                                errorMessage={state.errors.confirmPassword ? Localization.get("Confirm.Required") : Localization.get("ConfirmMismatch.ErrorMessage") }
                                style={inputStyle}
                                type="password"
                                autoComplete="off"
                                inputStyle={!requiresQuestionAndAnswer ? { marginBottom: 15 } : { marginBottom: 0 }}
                                value={state.confirmPassword}  tabIndex={8}/>

                    </GridSystem>
                    }
                    {requiresQuestionAndAnswer && <GridSystem>
                        <div>
                            <SingleLineInputWithError label={Localization.get("Question") }
                                error={state.errors.question}
                                onChange={this.onChange.bind(this, "question") }
                                tooltipMessage={Localization.get("Question.Help")}
                                errorMessage={Localization.get("Question.Required") }
                                style={inputStyle}
                                inputStyle={{ marginBottom: 15 }}
                                autoComplete="off"
                                value={state.UserDetails.question}  tabIndex={9}/>
                        </div>
                        <div>
                            <SingleLineInputWithError label={Localization.get("Answer") }
                                error={state.errors.answer}
                                onChange={this.onChange.bind(this, "answer") }
                                tooltipMessage={Localization.get("Answer.Help")}
                                errorMessage={Localization.get("Answer.Required")}
                                style={inputStyle}
                                autoComplete="off"
                                inputStyle={{ marginBottom: 15 }}
                                value={state.UserDetails.answer}  tabIndex={10}/></div>
                    </GridSystem>
                    }
                    <GridCell columnSize={100} className="email-notification-line">
                        <Checkbox value={state.UserDetails.notify}
                            label={Localization.get("Notify")}
                            onChange={this.onChange.bind(this, "notify") }  tabIndex={9}/>
                    </GridCell>
                    <GridCell columnSize={100} className="modal-footer">
                        <Button id="cancelbtn"  type="secondary" onClick={this.cancel.bind(this) }  tabIndex={10}>{Localization.get("btnCancel") }</Button>
                        <Button id="confirmbtn" type="primary" onClick={this.save.bind(this) } tabIndex={11}>{Localization.get("btnSave") }</Button>
                    </GridCell>
                </GridCell>
            </GridCell>
        );
    }
}

CreateUserBox.propTypes = {
    save: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,
    style: PropTypes.object,
    filter: PropTypes.number,
    appSettings: PropTypes.object
};

const mapDispatchToProps = (dispatch) => {
    return {
        save : (callback) =>{
            dispatch(callback);
        }
    };
};

export default connect(() => { return {}; },mapDispatchToProps)(CreateUserBox);