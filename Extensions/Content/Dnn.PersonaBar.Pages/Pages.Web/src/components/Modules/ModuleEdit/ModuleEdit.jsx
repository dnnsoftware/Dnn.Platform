import React, {Component} from "react";
import PropTypes from "prop-types";
import {pageActions as PageActions} from "../../../actions";
import utils from "../../../utils";

const queryString = {"HideCancel": "true", "HideDelete": "true", "NoRedirectOnUpdate": "true"};

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
            userMode: utils.getUserMode().toLowerCase(),
            editUrl: ''
        };
    }

    onIFrameLoad() {
        const iframe = this.iframeRef;
        const location = iframe.contentWindow.location.href;
        if(location.indexOf("popUp") === -1){
            if(this.closeOnEndRequest){
                this.props.onUpdatedModuleSettings();
            } else {
                this.redirectUrl(location);
            }
        } else {
            const pageRequestManager = iframe.contentWindow.Sys.WebForms.PageRequestManager.getInstance();
            pageRequestManager.add_beginRequest(this.beginRequestHandler.bind(this));
            pageRequestManager.add_endRequest(this.endRequestHandler.bind(this));
        }
    }

    endRequestHandler() {
        if (this.closeOnEndRequest) {
            this.props.onUpdatedModuleSettings();
        }
    }

    beginRequestHandler(pageRequestManager, beginRequestEventArgs) {
        const postBackElement = beginRequestEventArgs._postBackElement;
        this.closeOnEndRequest = postBackElement.id === ("dnn_ctr" + this.props.module.id + "_ModuleSettings_cmdUpdate");
        if(!this.closeOnEndRequest){
            this.closeOnEndRequest = postBackElement.id.indexOf("dnn_ctr" + this.props.module.id) > -1
                && postBackElement.id.match(/Close$|Cancel$|Save$/) !== null;
        }
    }

    checkUrlType(){
        const {props} = this;
        let editUrl = "";
        switch(props.editType){
            case "content":
                editUrl = props.module.editContentUrl;
                break;
            case "settings":
                editUrl = props.module.editSettingUrl;
                break;
        }

        if(editUrl !== ""){
            if(editUrl.indexOf('popUp') === -1){
                this.redirectUrl(editUrl);
            } else {
                editUrl = utils.url.appendQueryString(editUrl, queryString);
                this.setState({editUrl: editUrl});
            }
            
        }
    }

    redirectUrl(url){
        utils.getUtilities().closePersonaBar(function () {
            window.parent.location = url;
        });
    }

    addEventListener(){
        const iframe = this.iframeRef;
        if(iframe){
            iframe.addEventListener("load", this.onIFrameLoad);
        }
    }

    removeEventListener(){
        const iframe = this.iframeRef;
        if(iframe){
            iframe.removeEventListener("load", this.onIFrameLoad);
        }
    }

    componentDidMount() {
        const {props} = this;

        PageActions.viewPage(props.selectedPage.tabId, null, () => {
            this.setState({
                userMode: 'edit'
            }, () => {
                this.checkUrlType();
                this.addEventListener();
            });
        });
        this.addEventListener();
    }

    componentWillUnmount() {
        this.removeEventListener();
    }

    render() {
        const {state} = this;

        return (state.userMode === "edit" && 
            <iframe ref={node => this.iframeRef = node} src={state.editUrl} style={iFrameStyle} frameBorder={0}></iframe>
        );
    }
}

ModuleEdit.propTypes = {
    module: PropTypes.object.isRequired,
    editType: PropTypes.string.isRequired,
    onUpdatedModuleSettings: PropTypes.func.isRequired
};

export default ModuleEdit;