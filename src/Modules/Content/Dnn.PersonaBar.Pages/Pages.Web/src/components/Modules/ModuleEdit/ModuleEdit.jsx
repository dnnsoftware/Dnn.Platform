import React, {Component, PropTypes} from "react";
import {pageActions as PageActions} from "../../../actions";
import utils from "../../../utils";

const queryString = "?popUp=true&HideCancel=true&HideDelete=true&NoRedirectOnUpdate=true";

const iFrameStyle = { 
    width: "100%", 
    height: "450px", 
    marginTop: "20px" 
};

class ModuleEdit extends Component {
    
    constructor() {
        super();
        this.onIFrameLoad = this.onIFrameLoad.bind(this);
        this.closeOnEndRequest = false;
        this.state = {
            userMode: utils.getUserMode().toLowerCase()
        };
    }

    onIFrameLoad() {
        const iframe = this.refs.iframe;
        const pageRequestManager = iframe.contentWindow.Sys.WebForms.PageRequestManager.getInstance();
        pageRequestManager.add_beginRequest(this.beginRequestHandler.bind(this));
        pageRequestManager.add_endRequest(this.endRequestHandler.bind(this));
    }

    endRequestHandler() {
        if (this.closeOnEndRequest) {
            this.props.onUpdatedModuleSettings();
        }
    }

    beginRequestHandler(pageRequestManager, beginRequestEventArgs) {
        const postBackElement = beginRequestEventArgs._postBackElement;
        this.closeOnEndRequest = postBackElement.id === ("dnn_ctr" + this.props.module.id + "_ModuleSettings_cmdUpdate");
    }

    componentWillMount(){
        const {state, props} = this;
        if(state.userMode !== "edit"){
            PageActions.viewPage(props.selectedPage.tabId, null, () => {
                this.setState({
                    userMode: 'edit'
                }, () => {
                    this.addEventListener();
                });
            });
        }
    }

    addEventListener(){
        const iframe = this.refs.iframe;
        if(iframe){
            iframe.addEventListener("load", this.onIFrameLoad);
        }
    }

    removeEventListener(){
        const iframe = this.refs.iframe;
        if(iframe){
            iframe.removeEventListener("load", this.onIFrameLoad);
        }
    }

    componentDidMount() {
        this.addEventListener();
    }

    componentWillUnmount() {
        this.removeEventListener();
    }

    render() {
        const {state, props} = this;

        const moduleSettingControlPath = this.props.module.editSettingUrl + queryString;
        return (state.userMode === "edit" && 
            <iframe ref="iframe" src={moduleSettingControlPath} style={iFrameStyle} frameBorder={0}></iframe>
        );
    }
}

ModuleEdit.propTypes = {
    module: PropTypes.object.isRequired,
    onUpdatedModuleSettings: PropTypes.func.isRequired
};

export default ModuleEdit;