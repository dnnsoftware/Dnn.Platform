import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import {users as UserActions} from "../../../../actions";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import util from "../../../../utils";
import Button from "dnn-button";
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
            this.props.dispatch(UserActions.changePassword(this.state.changePassword, (data) => {
                if (data.Success) {
                    this.cancel();
                    util.utilities.notify("Password changed successfully.");
                }
                else {
                    util.utilities.notify(data.Message);
                }
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
        if (confirmPassword !== changePassword.password) {
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
        this.props.onCancel();
    }
    render() {
        let {state} = this;
        return this.props.visible && <div className="dnn-user-change-password">
            <GridCell className="do-not-close">
                <GridCell>
                    <div className="title">
                        Change Password
                    </div>
                    <SingleLineInputWithError label={Localization.get("label_Password") }
                        error={state.errors.password}
                        onChange={this.onChange.bind(this, "password") }
                        tooltipMessage="Enter new password."
                        style={inputStyle}
                        type="password"
                        inputStyle={{ marginBottom: 15 }}
                        value={state.changePassword.password}/>
                    <SingleLineInputWithError label={Localization.get("label_ConfirmPassword") }
                        error={state.errors.confirmPassword || state.errors.passwordsMatch}
                        onChange={this.onChange.bind(this, "confirmPassword") }
                        tooltipMessage="Enter confirm password."
                        style={inputStyle}
                        type="password"
                        inputStyle={{ marginBottom: 15 }}
                        value={state.confirmPassword}/>
                </GridCell>
                <GridSystem>
                    <Button className="right do-not-close" id="cancelbtn"  type="secondary" onClick={this.cancel.bind(this) }>{Localization.get("btn_Cancel") }</Button>
                    <Button id="confirmbtn do-not-close" type="primary" onClick={this.save.bind(this) }>{Localization.get("btn_Apply") }</Button>
                </GridSystem>
            </GridCell>
        </div >;
    }
}
ChangePassword.propTypes = {
    dispatch: PropTypes.func.isRequired,
    userId: PropTypes.array.isRequired,
    visible: PropTypes.bool,
    onCancel: PropTypes.bool
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