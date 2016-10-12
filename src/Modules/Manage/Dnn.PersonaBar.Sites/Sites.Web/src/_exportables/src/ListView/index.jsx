import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import PortalListItem from "./PortalListItem";
import Localization from "localization";
import { portal as PortalActions } from "actions";
import GridCell from "dnn-grid-cell";
import {
    TrashIcon,
    PreviewIcon,
    SettingsIcon,
    TemplateIcon
} from "dnn-svg-icons";
import styles from "./style.less";

function getUtility() {
    
    return window.dnn.initSites().utility;
}

class ListView extends Component {
    constructor() {
        super();
    }
    onDelete(portal, index) {
        const {props} = this;
        const utilities = getUtility();
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
    onExport(portal /*, index*/) {
        const {props} = this;
        props.onExportPortal(portal);
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
                portalButtons={this.getPortalButtons(portal, index) }
                portalStatisticInfo={props.getPortalMapping(portal) } />;
        });
    }

    render() {
        const portalList = this.getDetailList();

        return (
            <GridCell className={styles.siteList}>
                <GridCell className="portal-list-container">
                    {portalList}
                </GridCell>
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
        tabIndex: state.pagination.tabIndex,
        portals: state.portal.portals,
        totalCount: state.portal.totalCount,
        pagination: state.pagination,
        viewMode: state.viewMode
    };
}
export default connect(mapStateToProps)(ListView);