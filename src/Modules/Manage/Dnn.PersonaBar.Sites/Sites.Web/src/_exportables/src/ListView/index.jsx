import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import PortalListItem from "./PortalListItem";
import Localization from "localization";
import PortalActions from "../actions/PortalActions";
import { visiblePanel as VisiblePanelActions } from "actions";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import GridCell from "dnn-grid-cell";
import utilities from "utils";
import PersonaBarPage from "dnn-persona-bar-page";
import Button from "dnn-button";
import ExportPortal from "../ExportPortal";
import {
    TrashIcon,
    PreviewIcon,
    SettingsIcon,
    TemplateIcon
} from "dnn-svg-icons";
import styles from "./style.less";

class ListView extends Component {
    constructor() {
        super();
    }
    onDelete(portal, index) {
        const {props} = this;
        utilities.confirm(Localization.get("deletePortal").replace("{0}", portal.PortalName),
            Localization.get("ConfirmPortalDelete"),
            Localization.get("CancelPortalDelete"),
            () => {
                props.dispatch(PortalActions.deletePortal(portal.PortalID, index));
            });
    }
    onSetting(portal /*, index*/) {
        alert("Not yet implemented!");
    }
    onPreview(portal /*, index*/) {
        if (portal.PortalAliases && portal.PortalAliases.length > 0) {
            window.open(portal.PortalAliases[0].link);
        }
    }
    navigateMap(page, event) {
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(page));
    }
    onExport(portalBeingExported) {
        const {props} = this;
        props.dispatch(PortalActions.setPortalBeingExported(portalBeingExported, this.navigateMap.bind(this, 2)));
    }
    getPortalButtons(portal, index) {
        let portalButtons = [
            {
                icon: PreviewIcon,
                onClick: this.onPreview.bind(this, portal, index)
            },
            {
                icon: SettingsIcon,
                onClick: this.onSetting.bind(this, portal, index)
            },
            {
                icon: TemplateIcon,
                onClick: this.onExport.bind(this, portal, index)
            }
        ];
        if (portal.allowDelete) {
            portalButtons = portalButtons.concat([{ icon: TrashIcon, onClick: this.onDelete.bind(this, portal, index) }]);
        }

        /*eslint-disable react/no-danger*/
        return portalButtons.map((_button) => {
            return <div dangerouslySetInnerHTML={{ __html: _button.icon }} onClick={_button.onClick}></div>;
        });
    }
    getDetailList() {
        const {props} = this;
        return props.portals.map((portal, index) => {
            return <PortalListItem
                key={"portal-" + portal.PortalID}
                portal={portal}
                portalButtons={this.getPortalButtons(portal, index)}
                portalStatisticInfo={props.getPortalMapping(portal)} />;
        });
    }
    cancelExport(event) {
        if (event !== undefined)
            event.preventDefault();
        this.setState({
            portalBeingExported: {}
        });
        this.navigateMap(0);
    }
    render() {
        const portalList = this.getDetailList(), {props} = this;

        return (
            <GridCell className={styles.siteList}>
                {props.selectedPage === 0 &&
                    <GridCell className="portal-list-container">
                        <SocialPanelHeader title={"Sites"}>
                            <Button type="primary" onClick={props.onAddNewSite.bind(this)}>{"Add New Site"}</Button>
                        </SocialPanelHeader>
                        <SocialPanelBody>
                            {portalList}
                        </SocialPanelBody>
                    </GridCell>
                }
                {props.selectedPage === 2 &&
                    <GridCell className="export-portal-container">

                        <SocialPanelHeader title={Localization.get("ControlTitle_template")} />
                        <SocialPanelBody>
                            <ExportPortal
                                portalBeingExported={props.portalBeingExported}
                                onCancel={this.cancelExport.bind(this)} />
                        </SocialPanelBody>
                    </GridCell>
                }
            </GridCell>
        );
    }
}

ListView.propTypes = {
    dispatch: PropTypes.func.isRequired,
    getPortalMapping: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    portals: PropTypes.array,
    totalCount: PropTypes.number,
    onEditSite: PropTypes.func,
    onExportPortal: PropTypes.func,
    onSettingClick: PropTypes.func,
    onDeletePortal: PropTypes.func,
    onPreviewPortal: PropTypes.func
};
function mapStateToProps(state) {
    return {
        selectedPage: state.visiblePanel.selectedPage,
        portalBeingExported: state.portal.portalBeingExported,
        tabIndex: state.pagination.tabIndex,
        portals: state.portal.portals,
        totalCount: state.portal.totalCount,
        pagination: state.pagination,
        viewMode: state.viewMode
    };
}
export default connect(mapStateToProps)(ListView);
