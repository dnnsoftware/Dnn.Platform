import React, {Component, PropTypes} from "react";

const moduleControlPath = "/ctl/Module/ModuleId/";
const queryString = "?popUp=true&HideCancel=true&HideDelete=true&NoRedirectOnUpdate=true";

const iFrameStyle = { 
    width: "100%", 
    height: "450px", 
    marginTop: "20px" 
};

class ModuleEdit extends Component {
    render() {
        const moduleSettingControlPath = this.props.absolutePageUrl + moduleControlPath + this.props.module.id + queryString;
        return (
            <iframe src={moduleSettingControlPath} style={iFrameStyle} frameBorder={0}></iframe>
        );
    }
}

ModuleEdit.propTypes = {
    module: PropTypes.object.isRequired,
    absolutePageUrl: PropTypes.string.isRequired
};

export default ModuleEdit;