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

    // getMicroservicesExtensions() {
    //     let extensions = window.dnn && window.dnn.personaBarExtensions && window.dnn.personaBarExtensions.siteSettings
    //         ? window.dnn.personaBarExtensions.siteSettings : [];
    //     console.log(extensions);
    //     return extensions.filter(e => {
    //         return e.type === "microservices-settings";
    //     });
    // }

    // onCancel() {
    //     let extensions = this.getMicroservicesExtensions();
    //     for (let i = 0; i < extensions.length; i++) {
    //         let ext = extensions[i].name;
    //         if (this.refs[ext] && typeof this.refs[ext].cancel === "function") {
    //             this.refs[ext].cancel();
    //         }
    //     }
    // }

    // onSave() {
    //     util.utilities.confirm(resx.get("SaveConfirm"), resx.get("Save"), resx.get("No"), () => {
    //         let extensions = this.getMicroservicesExtensions();
    //         for (let i = 0; i < extensions.length; i++) {
    //             let ext = extensions[i].name;
    //             if (this.refs[ext]
    //                 && typeof this.refs[ext].save === "function"
    //                 && (typeof this.refs[ext].hasChange !== "function" || this.refs[ext].hasChange())) {
    //                 this.refs[ext].save();
    //             }
    //         }
    //     });
    // }

    // renderExtensions() {
    //     const {props, state} = this;

    //     let extensions = this.getMicroservicesExtensions();

    //     if (!extensions.length) {
    //         return null;
    //     }

    //     return <GridCell>
    //         <div className="sectionTitle">{resx.get("MicroServices")}</div>
    //         <div className="messageBox">{resx.get("MicroServicesDescription")}</div>
    //         {
    //             extensions.map(function (ext, index) {
    //                 if (typeof ext.init === "function") {
    //                     ext.init({ utility: util.utilities });
    //                 }

    //                 let className = index % 2 === 0 ? "left-column" : "right-column";
    //                 let ExtensionComponent = ext.extension;
    //                 return <GridCell columnSize="50">
    //                     <div className={className}>
    //                         <ExtensionComponent ref={ext.name} />
    //                     </div>
    //                 </GridCell>;
    //             })
    //         }
    //     </GridCell>;
    // }

    // renderActions() {
    //     const {props, state} = this;

    //     let extensions = this.getMicroservicesExtensions();

    //     if (!extensions.length) {
    //         return null;
    //     }

    //     return <div className="buttons-box">
    //         <Button
    //             type="secondary"
    //             onClick={this.onCancel.bind(this)}>
    //             {resx.get("Cancel")}
    //         </Button>
    //         <Button
    //             type="primary"
    //             onClick={this.onSave.bind(this)}>
    //             {resx.get("Save")}
    //         </Button>
    //     </div>;
    // }

    renderSiteBehaviorExtensions() {
        const SiteBehaviorExtras = window.dnn.SiteSettings && window.dnn.SiteSettings.SiteBehaviorExtras;
        if (!SiteBehaviorExtras || SiteBehaviorExtras.length === 0) {
            return;
        }
        return SiteBehaviorExtras.sort(function (a, b) {
            if (a.RenderOrder < b.RenderOrder) return -1;
            if (a.RenderOrder > b.RenderOrder) return 1;
            return 0;
        }).map((data) => {
            return data.Component;
        });
    }

    onSaveSiteBehaviorExtras() {
        const SiteBehaviorExtras = window.dnn.SiteSettings && window.dnn.SiteSettings.SiteBehaviorExtras;

        if (SiteBehaviorExtras && SiteBehaviorExtras.length > 0) {
            SiteBehaviorExtras.forEach((extra) => {
                if (typeof extra.SaveMethod === "function") {
                    //Call the Save Method of each SiteBehaviorExtra.
                    this.props.dispatch(extra.SaveMethod(
                        Object.assign({ formDirty: this.props[extra.ReducerKey].formDirty }, this.props[extra.ReducerKey].onSavePayload)
                    ));
                }
            });
        }
    }

    getSiteBehaviorExtensionsDirty() {
        let formDirty = false;
        const SiteBehaviorExtras = window.dnn.SiteSettings && window.dnn.SiteSettings.SiteBehaviorExtras;
        if (SiteBehaviorExtras && SiteBehaviorExtras.length > 0) {
            SiteBehaviorExtras.forEach((extra) => {
                if (this.props[extra.ReducerKey].formDirty) {
                    formDirty = true;
                }
            });
        }
        return formDirty;
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;

        return (
            <div className={styles.moreSettings}>
                {this.renderSiteBehaviorExtensions()}
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
                {(window.dnn.SiteSettings && window.dnn.SiteSettings.SiteBehaviorExtras) &&
                    <div className="buttons-box">
                        <Button
                            type="secondary"
                            disabled={!this.getSiteBehaviorExtensionsDirty()}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            type="primary"
                            disabled={!this.getSiteBehaviorExtensionsDirty()}
                            onClick={this.onSaveSiteBehaviorExtras.bind(this)}>
                            {resx.get("Save")}
                        </Button>
                    </div>
                }
            </div>
        );
    }
}

MoreSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    portalId: PropTypes.number,
    openHtmlEditorManager: PropTypes.func
};

function mapStateToProps(state) {
    return {
        ...state
    };
}

export default connect(mapStateToProps)(MoreSettingsPanelBody);