import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import { Tab, Tabs, TabList, TabPanel } from "react-tabs";
import {
    pagination as PaginationActions,
    siteInfo as SiteInfoActions
} from "../../actions";
import InputGroup from "dnn-input-group";
import SingleLineInput from "dnn-single-line-input";
import Dropdown from "dnn-dropdown";
import PagePicker from "dnn-page-picker";
import Switch from "dnn-switch";
import Label from "dnn-label";
import Button from "dnn-button";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

class BasicSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            basicSettings: undefined
        };
    }

    componentWillMount() {
        
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;

        if (state.basicSettings) {
            return (
                <div className={styles.basicSettings}>
                    
                </div>
            );
        }
        else return <div/>;
    }
}

BasicSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,    
    basicSettings: PropTypes.object
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,        
        basicSettings: state.siteInfo.settings
    };
}

export default connect(mapStateToProps)(BasicSettingsPanelBody);