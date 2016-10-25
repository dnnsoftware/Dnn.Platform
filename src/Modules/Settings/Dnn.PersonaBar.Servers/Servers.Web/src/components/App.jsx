import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import Button from "dnn-button";
import SocialPanelHeader from "dnn-social-panel-header";
import Body from "./Body";
import PersonaBarPage from "dnn-persona-bar-page";
import localization from "../localization";

const restartAppButtonStyle = {
    "margin-right": "10px"
};

class App extends Component { 
    render() {
        return (
            <div className="servers-app personaBar-mainContainer">
                <PersonaBarPage isOpen={true}>
                    <SocialPanelHeader title="Servers">
                        <Button type="secondary" size="large">{localization.get("clearCacheButtonLabel")}</Button>
                        <Button type="secondary" size="large" style={restartAppButtonStyle}>{localization.get("restartApplicationButtonLabel")}</Button>                        
                    </SocialPanelHeader>
                    <Body />
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