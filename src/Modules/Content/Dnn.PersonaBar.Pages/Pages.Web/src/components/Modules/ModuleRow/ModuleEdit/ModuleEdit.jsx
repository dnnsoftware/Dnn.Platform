import React, {Component, PropTypes} from "react";

const queryString = "?popUp=true&HideCancel=true&HideDelete=true&NoRedirectOnUpdate=true";

const iFrameStyle = { 
    width: "100%", 
    height: "450px", 
    marginTop: "20px" 
};

class ModuleEdit extends Component {
    render() {
        const moduleSettingControlPath = this.props.module.editSettingUrl + queryString;
        return (
            <iframe src={moduleSettingControlPath} style={iFrameStyle} frameBorder={0}></iframe>
        );
    }
}

ModuleEdit.propTypes = {
    module: PropTypes.object.isRequired
};

export default ModuleEdit;