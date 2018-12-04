import React, {Component} from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Button from "dnn-button";
import PersonaBarPageHeader from "dnn-persona-bar-page-header";
import Body from "./Body/Body";
import PersonaBarPage from "dnn-persona-bar-page";
import localization from "../localization";
import { bindActionCreators } from "redux";
import ServerActions from "../actions/server";
import utils from "../utils";

const restartAppButtonStyle = {
    "margin-right": "10px"
};

class App extends Component { 

    UNSAFE_componentWillReceiveProps(newProps) {       
        if (this.props.infoMessage !== newProps.infoMessage && newProps.infoMessage) {
            utils.notify(newProps.infoMessage);
        }

        if (newProps.reloadPage) {
            location.reload();
            return;
        }
        if (this.props.errorMessage !== newProps.errorMessage && newProps.errorMessage) {
            utils.notifyError(newProps.errorMessage);
        }
    }

    render() {
        const {props} = this;
        const buttonVisible = utils.isHostUser();
        return (
            <div className="servers-app personaBar-mainContainer">
                <PersonaBarPage isOpen={true}>
                    <PersonaBarPageHeader title="Servers">
                        {buttonVisible && 
                            <Button type="secondary" size="large" 
                                onClick={props.onClearCacheClicked}>{localization.get("clearCacheButtonLabel")}</Button>
                        }
                        {buttonVisible && 
                            <Button type="secondary" size="large" 
                                onClick={props.onRestartApplicationClicked} 
                                style={restartAppButtonStyle}>{localization.get("restartApplicationButtonLabel")}</Button>
                        }                        
                    </PersonaBarPageHeader>
                    <Body />
                </PersonaBarPage>                
            </div>
        );
    }
}

App.propTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    selectedPageVisibleIndex: PropTypes.number,
    onRestartApplicationClicked: PropTypes.func.isRequired,
    reloadPage: PropTypes.bool.isRequired,
    errorMessage: PropTypes.string,
    infoMessage: PropTypes.string
};

function mapStateToProps(state) {
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex,
        reloadPage: state.server.reloadPage,
        errorMessage: state.server.errorMessage,
        infoMessage: state.server.infoMessage
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators ({
            onRestartApplicationClicked: ServerActions.restartApplication,
            onClearCacheClicked: ServerActions.clearCache
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(App);