import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Modal, Button, Dropdown, Label } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import resx from "localization";
import { ExtensionActions } from "actions";

const modalStyles = {
    overlay: {
        zIndex: "99999",
        backgroundColor: "rgba(0, 0, 0, 0)"        
    },
    content: {
        top: "250px",
        left: "220px",
        position: "absolute",
        padding: "30px 30px 30px 30px",
        borderRadius: 0,
        border: "none",
        width: "550px",
        height: "380px",
        backgroundColor: "#FFF",
        userSelect: "none",
        WebkitUserSelect: "none",
        MozUserSelect: "none",
        MsUserSelect: "none",
        boxShadow: "0 0 20px 0 rgba(0, 0, 0, .2)"
    }
};

class InUseModal extends Component {
    constructor() {
        super();
        this.state = {
            usageFilter: undefined,
            portalId: -1
        };
    }

    UNSAFE_componentWillMount() {
        const {props, state} = this;

        if (props.usageFilter) {
            this.setState({
                usageFilter: props.usageFilter
            });
            if (state.portalId === -1) {
                let portalId = props.usageFilter.find(p => p.IsCurrentPortal === true).PortalID;
                this.setState({
                    usageFilter: props.usageFilter,
                    portalId: portalId
                });
                props.dispatch(ExtensionActions.getPackageUsage(portalId, props.packageId));
            }
            return;
        }
        props.dispatch(ExtensionActions.GetPackageUsageFilter());
    }

    UNSAFE_componentWillReceiveProps(props) {
        let {state} = this;

        if (state.portalId === -1) {
            let portalId = props.usageFilter.find(p => p.IsCurrentPortal === true).PortalID;
            this.setState({
                usageFilter: props.usageFilter,
                portalId: portalId
            });
            props.dispatch(ExtensionActions.getPackageUsage(portalId, props.packageId));
        }
    }

    onClose() {
        const {props} = this;
        props.onClose();
    }

    getPortalOptions() {
        const {props} = this;
        let options = [];
        if (props.usageFilter !== undefined) {
            options = props.usageFilter.map((portal) => {
                return {
                    label: portal.PortalName, value: portal.PortalID
                };
            });
        }
        return options;
    }

    onSelectPortal(event) {
        let {props} = this;

        this.setState({
            portalId: event.value
        });

        props.dispatch(ExtensionActions.getPackageUsage(event.value, props.packageId));
    }

    getCurrentUsageFilter() {
        let {props, state} = this;
        if (props.usageFilter && state.portalId !== -1) {
            return props.usageFilter.find(p => p.PortalID === state.portalId).PortalName;
        }
        else return null;
    }

    getUsageDetailSubject() {
        let {props} = this;
        if (props.tabUrls) {
            if (props.tabUrls.length > 0) {
                return resx.get("ModuleUsageDetail").replace("{0}", props.tabUrls.length).replace("{1}", this.getCurrentUsageFilter());
            }
            else return resx.get("NoModuleUsageDetail").replace("{0}", this.getCurrentUsageFilter());
        }
    }

    /* eslint-disable react/no-danger */
    renderUsageDetail() {
        const {props} = this;
        if (props.tabUrls) {
            return props.tabUrls.map((item, i) => {
                return (
                    <div key={i} className="usage-detail-taburl" dangerouslySetInnerHTML={{ __html: item.TabUrl }}></div>
                );
            });
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <Modal
                fixedHeight={props.fixedHeight}
                isOpen={props.isOpened}
                style={modalStyles}>
                {props.fixedHeight &&
                    <div className="modepanel-content-wrapper" style={{ height: "calc(100% - 100px)" }}>
                        <div className="modepanel-content-title">{resx.get("ModuleUsageTitle").replace("{0}", props.packageName)}</div>
                        <div className="modepanel-content-filter">
                            <Label
                                style={{ width: "auto", marginTop: "8px" }}
                                label={resx.get("PagesFromSite")}
                            />
                            {props.usageFilter && <Dropdown
                                enabled={props.usageFilter.length > 1}
                                value={state.portalId}
                                style={{ width: "150px" }}
                                options={this.getPortalOptions()}
                                withBorder={false}
                                onSelect={this.onSelectPortal.bind(this)}
                            />
                            }
                        </div>
                        <div className="modepanel-content-detail-wrapper">
                            <Label
                                label={this.getUsageDetailSubject()}
                            />
                            <div className="usage-detail">
                                {this.renderUsageDetail()}
                            </div>
                        </div>
                        <div className="button-box">
                            <Button
                                type="secondary"
                                onClick={this.onClose.bind(this)}>
                                {resx.get("Close")}
                            </Button>
                        </div>
                    </div>
                }
            </Modal>
        );
    }
}

InUseModal.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    usageFilter: PropTypes.array,
    tabUrls: PropTypes.array,
    packageName: PropTypes.string,
    packageId: PropTypes.number,
    fixedHeight: PropTypes.number,
    collapsibleWidth: PropTypes.number,
    collapsibleHeight: PropTypes.number,
    keepCollapsedContent: PropTypes.bool,
    scrollAreaStyle: PropTypes.object,
    isOpened: PropTypes.bool,
    onClose: PropTypes.func.isRequired,
    isHost: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        usageFilter: state.extension.usageFilter,
        tabUrls: state.extension.tabUrls
    };
}

export default connect(mapStateToProps)(InUseModal);