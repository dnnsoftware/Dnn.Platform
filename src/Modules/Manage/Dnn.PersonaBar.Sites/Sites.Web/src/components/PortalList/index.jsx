import React, {PropTypes, Component} from "react";
import { connect } from "react-redux";
import Button from "dnn-button";
import ListView from "dnn-sites-list-view";
import Localization from "localization";
import utilities from "utils";
import GridCell from "dnn-grid-cell";
import {portal as PortalActions, pagination as PaginationActions } from "actions";
import styles from "./style.less";
import Moment from "moment";

class PortalList extends Component {
    componentWillMount() {
        const {props} = this;
        props.dispatch(PortalActions.loadPortals({
            portalGroupId: -1,
            filter: "",
            pageIndex: 0,
            pageSize: 10
        }));
    }
    getDetailList(portal) {
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
        ];
    }
    onExport(portal /*, index*/) {
        const {props} = this;
        props.onExportPortal(portal);
    }
    onSearch(filter) {
        const {props} = this;
        props.dispatch(PaginationActions.searchPortals({
            portalGroupId: -1,
            filter: filter,
            pageIndex: props.pagination.pageIndex,
            pageSize: props.pagination.pageSize
        }));
    }
    onCardView() {

    }
    onListView() {

    }

    render() {
        const {props} = this;
        return (
            <GridCell className={styles.sitesPortalList}>
                <ListView
                    onAddNewSite={props.onAddNewSite.bind(this)} 
                    portals={props.portals}
                    getPortalMapping={this.getDetailList.bind(this) }
                    onExportPortal={this.onExport.bind(this)}
                    utilities={utilities}
                    PortalActions={PortalActions}
                    />
            </GridCell>
        );
    }
}

PortalList.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onAddNewSite: PropTypes.func,
    onEditSite: PropTypes.func,
    onExportPortal: PropTypes.func,
    portals: PropTypes.array
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

export default connect(mapStateToProps)(PortalList);