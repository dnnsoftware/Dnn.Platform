import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import { Tab, Tabs, TabList, TabPanel } from "react-tabs";
import {
    pagination as PaginationActions
} from "../../actions";
import Button from "dnn-button";
import GridCell from "dnn-grid-cell";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

class MoreSettingsPanelBody extends Component {
    constructor() {
        super();
    }

    getMicroservicesExtensions(){
        let extensions = window.dnn && window.dnn.personaBarExtensions && window.dnn.personaBarExtensions.siteSettings
                            ? window.dnn.personaBarExtensions.siteSettings : [];

        return extensions.filter( e => {
            return e.type === "microservices-settings";
        });
    }

    onCancel(){
        let extensions = this.getMicroservicesExtensions();
        for(let i  = 0;i < extensions.length; i++){
            let ext = extensions[i].name;
            if(this.refs[ext] && typeof this.refs[ext].cancel === "function"){
                this.refs[ext].cancel();
            }
        }
    }

    onSave(){
        util.utilities.confirm(resx.get("SaveConfirm"), resx.get("Save"), resx.get("No"), () => {
            let extensions = this.getMicroservicesExtensions();
            for(let i  = 0;i < extensions.length; i++){
                let ext = extensions[i].name;
                if(this.refs[ext] 
                    && typeof this.refs[ext].save === "function"
                    && (typeof this.refs[ext].hasChange !== "function" || this.refs[ext].hasChange())){
                    this.refs[ext].save();
                }
            }
        });
    }

    renderExtensions(){
        const {props, state} = this;

        let extensions = this.getMicroservicesExtensions();

        if(!extensions.length){
            return null;
        }

        return <GridCell>
            <div className="sectionTitle">{resx.get("MicroServices")}</div>
            <div className="messageBox">{resx.get("MicroServicesDescription")}</div>
            {
                extensions.map(function(ext){
                    if(typeof ext.init === "function"){
                        ext.init({utility: util.utilities});
                    }

                    let ExtensionComponent = ext.extension;
                    return <GridCell columnSize="50">
                        <ExtensionComponent ref={ext.name} />
                    </GridCell>;
                })
            }
        </GridCell>;
    }

    renderActions(){
        const {props, state} = this;

        let extensions = this.getMicroservicesExtensions();

        if(!extensions.length){
            return null;
        }

        return <div className="buttons-box">
            <Button
                type="secondary"
                onClick={this.onCancel.bind(this)}>
                {resx.get("Cancel")}
            </Button>
            <Button
                type="primary"
                onClick={this.onSave.bind(this)}>
                {resx.get("Save")}
            </Button>
        </div>;
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;

        return (
            <div className={styles.moreSettings}>
                {this.renderExtensions()}
                <div className="sectionTitle">{resx.get("HtmlEditor")}</div>
                <div className="htmlEditorWrapper">
                    <div className="htmlEditorWrapper-left">
                        <div className="htmlEditorWarning">{resx.get("HtmlEditorWarning")}</div>
                    </div>
                    <div className="htmlEditorWrapper-right">
                        <Button
                            type="secondary"
                            onClick={props.openHtmlEditorManager.bind(this)}>
                            {resx.get("OpenHtmlEditor")}
                        </Button>
                    </div>
                </div>
                {this.renderActions()}
            </div>
        );
    }
}

MoreSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    portalId: PropTypes.number,
    openHtmlEditorManager: PropTypes.func
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(MoreSettingsPanelBody);