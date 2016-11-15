import React, {Component, PropTypes} from "react";

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

    componentDidMount() {
        const iframe = this.refs.iframe;
        iframe.addEventListener("load", this.onIFrameLoad);
    }

    componentWillUnmount() {
        const iframe = this.refs.iframe;
        iframe.removeEventListener("load", this.onIFrameLoad);
    }

    render() {
        const moduleSettingControlPath = this.props.module.editSettingUrl + queryString;
        return (
            <iframe ref="iframe" src={moduleSettingControlPath} style={iFrameStyle} frameBorder={0}></iframe>
        );
    }
}

ModuleEdit.propTypes = {
    module: PropTypes.object.isRequired,
    onUpdatedModuleSettings: PropTypes.func.isRequired
};

export default ModuleEdit;