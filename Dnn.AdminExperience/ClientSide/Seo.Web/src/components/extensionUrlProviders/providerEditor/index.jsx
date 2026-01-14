import React, { Component } from "react";
import PropTypes from "prop-types";
import styles from "./style.module.less";

class ProviderEditor extends Component {
    constructor() {
        super();
    }

     
    render() {        
        return <div className={styles.providerSettingEditor}>
            <iframe className="edit-provider" seamless src={this.props.settingUrl} />
        </div>;
    }
}

ProviderEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    settingUrl: PropTypes.string,
    Collapse: PropTypes.func
};

export default (ProviderEditor);