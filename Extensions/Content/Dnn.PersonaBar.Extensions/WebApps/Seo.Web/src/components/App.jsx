import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { PersonaBarPage, PersonaBarPageHeader } from "@dnnsoftware/dnn-react-common";
import Body from "./body";
import resx from "../resources";

class App extends Component {
    constructor() {
        super();
    }

    render() {
        const {props} = this;
        return (
            <div className="seo-app">
                <PersonaBarPage isOpen={props.selectedPage === 0}>
                    <PersonaBarPageHeader title={resx.get("nav_Seo")}>
                    </PersonaBarPageHeader>
                    <Body />
                </PersonaBarPage>
            </div>
        );
    }
}

App.propTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number
};


function mapStateToProps(state) {
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex
    };
}


export default connect(mapStateToProps)(App);