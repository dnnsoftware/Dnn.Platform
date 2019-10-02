import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { PersonaBarPage } from "@dnnsoftware/dnn-react-common";
import Body from "./Body";

class App extends Component {
    constructor() {
        super();
        this.state = {
            editMode: false,
            portalBeingExported: {}
        };
    }

    render() {
        const {props} = this;
        return (
            <div className="themes-Root">
                <PersonaBarPage isOpen={props.selectedPage === 0}>
                    <Body />
                </PersonaBarPage>
            </div>
        );
    }
}

App.propTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    selectedPageVisibleIndex: PropTypes.number
};


function mapStateToProps(state) {
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex
    };
}


export default connect(mapStateToProps)(App);