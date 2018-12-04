import PropTypes from 'prop-types';
import React, { Component } from "react";
import { connect } from "react-redux";
import Localization from "localization";
import { CommonUsersActions } from "../../../actions";
import utilities from "utils";
import { Button, SingleLineInputWithError, GridSystem, GridCell } from "@dnnsoftware/dnn-react-common";
import "./style.less";

const inputStyle = { width: "100%" };
const blankChangePassword = {
    userId: 0,
    password: ""
};

class ChangePassword extends Component {
    constructor(props) {
        super(props);
        this.state = {
            changePassword: Object.assign({}, blankChangePassword),
            errors: {
                password: false,
                confirmPassword: false,
                passwordsMatch: false
            },
            confirmPassword: ""
        };
    }
    componentWillMount() {
        let {changePassword} = this.state;
        changePassword.userId = this.props.userId;
        changePassword.password = "";
        this.setState({
            changePassword
        });
    }
    componentWillReceiveProps(newProps) {
        this.clear(() => {
            let {changePassword} = this.state;
            changePassword.userId = newProps.userId;
            this.setState({
                changePassword
            });
        });
    }

    onChange(key, item) {
        if (key === "confirmPassword") {
            this.setState({
                confirmPassword: item.target.value
            }, () => {
                this.validateForm();
            });
        } else {
            let {changePassword} = this.state;
            changePassword[key] = item.target.value;
            this.setState({ changePassword }, () => {
                this.validateForm();
            });
        }
    }
    save() {
        if (this.validateForm()) {
            this.props.dispatch(CommonUsersActions.changePassword(this.state.changePassword, () => {
                this.cancel();
                utilities.notify(Localization.get("ChangeSuccessful"), 3000);
            }));
        }
    }
    validateForm() {
        let valid = true;
        let {errors} = this.state;
        errors.password = false;
        errors.confirmPassword = false;
        errors.passwordsMatch = false;
        let {changePassword} = this.state;
        let {confirmPassword} = this.state;
        if (changePassword.password === "") {
            errors.password = true;
            valid = false;
        }
        if (changePassword.confirmPassword === "") {
            errors.confirmPassword = true;
            valid = false;
        }
        else if (confirmPassword !== changePassword.password) {
            errors.passwordsMatch = true;
            valid = false;
        }
        this.setState({ errors });

        return valid;
    }
    clear(callback) {
        this.setState({
            changePassword: Object.assign({}, blankChangePassword),
            confirmPassword: "",
            errors: {
                password: false,
                confirmPassword: false,
                passwordsMatch: false
            }
        }, () => {
            if (typeof callback === "function") {
                callback();
            }
        });
    }
    cancel() {
        this.clear();
        if (typeof this.props.onCancel === "function")
            this.props.onCancel();
    }
    render() {
        let {state} = this;
        return this.props.visible && <div className="dnn-user-change-password">
            <GridCell className="do-not-close">
                <GridCell>
                    <div className="title">
                        {Localization.get("ChangePassword")}
                    </div>
                    <SingleLineInputWithError label={Localization.get("NewPassword") }
                        error={state.errors.password}
                        onChange={this.onChange.bind(this, "password") }
                        tooltipMessage={Localization.get("NewPassword.Help") }
                        errorMessage={Localization.get("NewPassword.Required") }
                        style={inputStyle}
                        type="password"
                        autoComplete="off"
                        inputStyle={{ marginBottom: 15 }}
                        value={state.changePassword.password}/>
                    <SingleLineInputWithError label={Localization.get("NewConfirm") }
                        error={state.errors.confirmPassword || state.errors.passwordsMatch}
                        onChange={this.onChange.bind(this, "confirmPassword") }
                        tooltipMessage={Localization.get("NewConfirm.Help") }
                        errorMessage={state.errors.confirmPassword ? Localization.get("NewConfirm.Required") : Localization.get("NewConfirmMismatch.ErrorMessage") }
                        style={inputStyle}
                        type="password"
                        inputStyle={{ marginBottom: 15 }}
                        autoComplete="off"
                        value={state.confirmPassword}/>
                </GridCell>
                <GridSystem>
                    <Button className="right do-not-close" id="cancelbtn"  type="secondary" onClick={this.cancel.bind(this) }>{Localization.get("btnCancel") }</Button>
                    <Button id="confirmbtn do-not-close" type="primary" onClick={this.save.bind(this) }>{Localization.get("btnApply") }</Button>
                </GridSystem>
            </GridCell>
        </div >;
    }
}
ChangePassword.propTypes = {
    dispatch: PropTypes.func.isRequired,
    userId: PropTypes.array.isRequired,
    visible: PropTypes.bool,
    onCancel: PropTypes.func
};
ChangePassword.defaultProps = {
    visible: true
};
function mapStateToProps(state) {
    return {
        userDetails: state.users.userDetails
    };
}

export default connect(mapStateToProps)(ChangePassword);