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
    TemplateIcon,
    DownloadIcon,
    UploadIcon
} from "dnn-svg-icons";
import styles from "./style.less";

class ListView extends Component {
    constructor() {
        super();
    }
    onDelete(portal, index) {
        const { props } = this;
        utilities.confirm(Localization.get("deletePortal").replace("{0}", portal.PortalName),
            Localization.get("ConfirmPortalDelete"),
            Localization.get("CancelPortalDelete"),
            () => {
                props.dispatch(CommonPortalListActions.deletePortal(portal.PortalID, index));
            });
    }
    backToSites() {
        utilities.loadPanel(this.props.sitesModule, {});
    }
    onSetting(portal /*, index*/) {
        let event = document.createEvent("Event");

        event.initEvent("portalIdChanged", true, true);

        let settings = {
            portalId: portal.PortalID,
            referrer: this.props.sitesModule,
            referrerText: Localization.get("BackToSites"),
            backToReferrerFunc: this.backToSites.bind(this),
            isHost: true
        };

        event = Object.assign(event, settings);

        utilities.loadPanel(this.props.siteSettingModule, {
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
        const { props } = this;
        props.dispatch(CommonVisiblePanelActions.selectPanel(page));
    }

    onImportExport(portal, type) {
        let event = document.createEvent("Event");

        event.initEvent("siteImportExport", true, true);

        let settings = {
            importExportJob: {
                portalId: portal.PortalID,
                portalName: portal.PortalName,
                jobType: type
            },
            referrer: this.props.sitesModule,
            referrerText: Localization.get("BackToSites"),
            backToReferrerFunc: this.backToSites.bind(this)            
        };

        event = Object.assign(event, settings);

        utilities.loadPanel(this.props.siteImportExportModule, {
            settings
        });

        document.dispatchEvent(event);
    }

    getPortalButtons(portal, index) {
        let portalButtons = [
            {
                icon: PreviewIcon,
                onClick: this.onPreview.bind(this, portal, index),
                title: Localization.get("ViewSite")
            },
            {
                icon: SettingsIcon,
                onClick: this.onSetting.bind(this, portal, index),
                title: Localization.get("SiteSettings")
            },
            {
                icon: UploadIcon,
                onClick: this.onImportExport.bind(this, portal, "Export", index),
                title: Localization.get("SiteExport")
            },
            {
                icon: DownloadIcon,
                onClick: this.onImportExport.bind(this, portal, "Import", index),
                title: Localization.get("SiteImport")
            }
        ];
        if (portal.allowDelete) {
            portalButtons = portalButtons.concat([
                {
                    icon: TrashIcon,
                    onClick: this.onDelete.bind(this, portal, index),
                    title: Localization.get("DeleteSite")
                }
            ]);
        }

        /*eslint-disable react/no-danger*/
        return portalButtons.map((_button) => {
            return <div dangerouslySetInnerHTML={{ __html: _button.icon }} title={_button.title} onClick={_button.onClick}></div>;
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
                value: Moment(portal.LastModifiedOnDate).locale(this.props.culture).fromNow()
            },
            {
                label: Localization.get("SiteDetails_Pages"),
                value: portal.Pages
            }
        ].concat((this.props.getPortalMapping && this.props.getPortalMapping(portal)) || []);
    }
    getDetailList() {
        const { props } = this;
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
        const portalList = this.getDetailList(), { props } = this;

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
    sitesModule: PropTypes.string,
    culture: PropTypes.string,
    siteImportExportModule: PropTypes.string
};

ListView.defaultProps = {
    siteSettingModule: "Dnn.SiteSettings",
    sitesModule: "Dnn.Sites",
    siteImportExportModule: "Dnn.SiteImportExport",
    culture: 'en-US'
};

function mapStateToProps(state) {
    return {
        portals: state.portal.portals,
        totalCount: state.portal.totalCount
    };
}
export default connect(mapStateToProps)(ListView);
