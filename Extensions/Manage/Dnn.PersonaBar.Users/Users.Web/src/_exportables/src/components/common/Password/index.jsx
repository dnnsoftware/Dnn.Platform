import React, { Component  } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { SingleLineInputWithError }from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import { getPasswordStrength } from "utils/PasswordStrength";
import {CommonUsersActions as UserActions } from "../../../actions";

import "./style.less";

class Password extends Component {
    constructor(props) {
        super(props);
    }

    componentDidMount() {
        this.props.loadPasswordStrengthOptions();
    }
   
    render() {
        return (
            <div>
                <SingleLineInputWithError label={Localization.get("Password") }
                    error={this.props.error.password}
                    onChange={this.props.onChangePassword }
                    tooltipMessage={Localization.get("Password.Help")}
                    errorMessage={Localization.get("Password.Required") }
                    style={this.props.style}
                    inputStyle={!this.props.requiresQuestionAndAnswer ? { marginBottom: 15 } : { marginBottom: 0 }}
                    type="password"
                    autoComplete="off"
                    value={this.props.UserDetails.password}  tabIndex={7}/>

                <div id="passwordStrengthBar" className={"passwordStrength " + getPasswordStrength(this.props.UserDetails.password, this.props.passwordStrengthOptions)}></div>
                <div id="passwordStrengthLabel" className={"passwordStrengthLabel " + getPasswordStrength(this.props.UserDetails.password, this.props.passwordStrengthOptions)}>
                    {getPasswordStrength(this.props.UserDetails.password, this.props.passwordStrengthOptions)}
                </div>
            </div>
        );
    }
}

Password.propTypes = {
    error:PropTypes.object,
    style: PropTypes.object.isRequired,
    UserDetails: PropTypes.object.isRequired,
    requiresQuestionAndAnswer : PropTypes.bool.isRequired,
    onChangePassword : PropTypes.func.isRequired,
    passwordStrengthOptions : PropTypes.object,
    loadPasswordStrengthOptions : PropTypes.func
};

const mapStateToProps = (state) => {
    return {
        passwordStrengthOptions : state.users.passwordStrengthOptions
    };
};

const mapDispatchToProps = (dispatch) => {
    return {
        loadPasswordStrengthOptions : () =>{
            dispatch(UserActions.passwordStrength());
        }
    };
};

export default connect(mapStateToProps,mapDispatchToProps)(Password) ;