import React, { Component, PropTypes } from "react";
import "./style.less";
import styles from "./style.less";

class ProviderEditor extends Component {
    constructor() {
        super();
    }

    componentWillMount() {
    }    

    /* eslint-disable react/no-danger */
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