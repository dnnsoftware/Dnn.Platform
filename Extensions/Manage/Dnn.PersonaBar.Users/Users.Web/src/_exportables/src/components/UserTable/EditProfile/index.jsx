import PropTypes from 'prop-types';
import React, { Component } from "react";
import { connect } from "react-redux";
import { CommonUsersActions } from "../../../actions";
import "./style.less";

class EditProfile extends Component {
    constructor(props) {
        super(props);
        this.state = {
            userDetails: props.userDetails
        };
    }
    componentWillMount() {
        let {props} = this;
        if (props.userDetails === undefined || props.userDetails.userId !== props.userId) {
            this.getUserDetails(props);
        }
    }
    componentWillReceiveProps(newProps) {
        if (newProps.userDetails === undefined && newProps.userDetails.userId !== newProps.userId) {
            this.getUserDetails(newProps);
        }
    }
    getUserDetails(props) {
        props.dispatch(CommonUsersActions.getUserDetails({ userId: props.userId }, (data) => {
            let userDetails = Object.assign({}, data);
            this.setState({
                userDetails
            });
        }));
    }
    render() {
        return this.state.userDetails !== undefined && this.state.userDetails.editProfileUrl !== undefined  && <iframe 
            className="edit-profile" seamless
            src={this.state.userDetails.editProfileUrl}
            />;
    }
}

EditProfile.propTypes = {
    userDetails: PropTypes.object,
    userId:PropTypes.number
};

function mapStateToProps(state) {
    return {
        userDetails: state.users.userDetails
    };
}

export default connect(mapStateToProps)(EditProfile);