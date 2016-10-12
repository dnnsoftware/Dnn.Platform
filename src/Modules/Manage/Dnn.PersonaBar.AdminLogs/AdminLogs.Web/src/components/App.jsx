import React, {Component, PropTypes} from "react";
import PersonaBarPagesContainer from "../containers/personaBarPagesContainer";
import SocialPanelHeader from "./socialPanelHeader";
import SocialPanelBody from "./SocialPanelBody";
import { connect } from "react-redux";
import {visiblePanel as VisiblePanelActions } from "../actions";
require("es6-object-assign").polyfill();
require("array.prototype.find").shim();
require("array.prototype.findindex").shim();

class App extends Component {
    constructor() {
        super();
    }

    navigateMap(page, index, event) {
        event.preventDefault();
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(page, index));
    }
    render() {
        const {props} = this;
        return (
            <PersonaBarPagesContainer pages={[
                <div>
                    <SocialPanelHeader title={("Admin Logs") }>
                    </SocialPanelHeader>
                    <SocialPanelBody />
                </div>]}
                selectedPage={props.selectedPage}
                selectedPageVisibleIndex={props.selectedPageVisibleIndex}
                repaintChildren={true}/>
        );
    }
}
App.PropTypes = {
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