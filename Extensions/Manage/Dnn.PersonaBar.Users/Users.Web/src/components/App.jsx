import PropTypes from 'prop-types';
import React, { Component } from "react";
import { connect } from "react-redux";
import Body from "./Body";
import {CommonUsersActions } from "dnn-users-common-actions";
import { PersonaBarPage } from "@dnnsoftware/dnn-react-common";

class App extends Component {
    constructor() {
        super();
    }
    componentWillMount() {
        const {props} = this;
        props.dispatch(CommonUsersActions.getUsers({
            searchText: "",
            filter: 0,
            pageIndex: 0,
            pageSize: 10,
            sortColumn: "",
            sortAscending: false
        }));
    }
    render() {
        return (
            <div className="boilerplate-app personaBar-mainContainer">
                <PersonaBarPage isOpen={true}>
                    <Body />
                </PersonaBarPage>
            </div>
        );
    }
}

App.PropTypes = {
    dispatch: PropTypes.func.isRequired
};


function mapStateToProps() {
    return {};
}


export default connect(mapStateToProps)(App);