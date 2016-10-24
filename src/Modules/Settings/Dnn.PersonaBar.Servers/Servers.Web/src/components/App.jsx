import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import Button from "dnn-button";
import SocialPanelHeader from "dnn-social-panel-header";
import Body from "./Body";
import PersonaBarPage from "dnn-persona-bar-page";
import {visiblePanel as VisiblePanelActions } from "../actions";
class App extends Component {
    constructor() {
        super();
    }

    navigateMap(page, event) {
        event.preventDefault();
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(page));
    }
    render() {
        const {props} = this;
        return (
            <div className="servers-app personaBar-mainContainer">
                <PersonaBarPage isOpen={props.selectedPage === 0}>
                    <SocialPanelHeader title="Servers">
                        <Button type="primary" onClick={this.navigateMap.bind(this, 1) }>Primary Action</Button>
                    </SocialPanelHeader>
                    <Body />
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 1}>
                    <SocialPanelHeader title="Pane 2">
                        <Button type="primary" onClick={this.navigateMap.bind(this, 0) }>Go back</Button>
                    </SocialPanelHeader>
                </PersonaBarPage>
            </div>
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