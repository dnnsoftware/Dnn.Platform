import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import PortalListItem from "./PortalListItem";
import Localization from "localization";
import { CommonVisiblePanelActions, CommonPortalListActions, CommonExportPortalActions } from "actions";
import GridCell from "dnn-grid-cell";
import utilities from "utils";
import Moment from "moment";
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
                props.dispatch(CommonPortalListActions.deletePortal(portal.PortalID, index));
            });
    }
    backToSites(){
        utilities.loadPanel(this.props.sitesModule, {
            identifier: "Sites", // This is always SiteSettings, to call the proper GetExtensions method.
            moduleName: this.props.sitesModule,
            path: this.props.sitesModule,
            query: ""
        });
    }
    onSetting(portal /*, index*/) {
        let event = document.createEvent("Event");

        event.initEvent("portalIdChanged", true, true);

        let settings = {
            portalId: portal.PortalID,
            referrer: this.props.siteSettingModule,  
            referrerText: Localization.get("BackToSites"),
            backToReferrerFunc: this.backToSites.bind(this),
            isHost: true
        };

        event = Object.assign(event, settings);

        utilities.loadPanel(this.props.siteSettingModule, {
            identifier: "SiteSettings", // This is always SiteSettings, to call the proper GetExtensions method.
            moduleName: this.props.siteSettingModule,
            path: this.props.siteSettingModule,
            query: "",
            settings
        });

        document.dispatchEvent(event);
    }
    onPreview(portal /*, index*/) {
        if (portal.PortalAliases && portal.PortalAliases.length > 0) {
            window.open(portal.PortalAliases[0].link);
        }
    }
    navigateMap(page) {
        const {props} = this;
        props.dispatch(CommonVisiblePanelActions.selectPanel(page));
    }
    onExport(portalBeingExported) {
        const {props} = this;
        props.dispatch(CommonExportPortalActions.setPortalBeingExported(portalBeingExported, this.navigateMap.bind(this, 2)));
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
    getPortalMapping(portal) {
        return [
            {
                label: Localization.get("SiteDetails_SiteID"),
                value: portal.PortalID
            },
            {
                label: Localization.get("SiteDetails_Users"),
                value: portal.Users
            },
            {
                label: Localization.get("SiteDetails_Updated"),
                value: Moment(portal.LastModifiedOnDate).fromNow()
            },
            {
                label: Localization.get("SiteDetails_Pages"),
                value: portal.Pages
            }
        ].concat((this.props.getPortalMapping && this.props.getPortalMapping(portal)) || []);
    }
    getDetailList() {
        const {props} = this;
        return props.portals.map((portal, index) => {
            return <PortalListItem
                key={"portal-" + portal.PortalID}
                portal={portal}
                portalButtons={this.getPortalButtons(portal, index)}
                portalStatisticInfo={this.getPortalMapping(portal)} />;
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
                <GridCell className={"portal-list-container " + props.className}>
                    {portalList}
                </GridCell>
            </GridCell>
        );
    }
}

ListView.propTypes = {
    dispatch: PropTypes.func.isRequired,
    siteSettingModule: PropTypes.string,
    getPortalMapping: PropTypes.func.isRequired,
    portals: PropTypes.array,
    totalCount: PropTypes.number,
    onEditSite: PropTypes.func,
    onExportPortal: PropTypes.func,
    onSettingClick: PropTypes.func,
    onDeletePortal: PropTypes.func,
    onPreviewPortal: PropTypes.func,
    sitesModule: PropTypes.string
};

ListView.defaultProps = {
    siteSettingModule: "SiteSettings",
    sitesModule: "Sites"
};

function mapStateToProps(state) {
    return {
        portals: state.portal.portals,
        totalCount: state.portal.totalCount
    };
}
export default connect(mapStateToProps)(ListView);
