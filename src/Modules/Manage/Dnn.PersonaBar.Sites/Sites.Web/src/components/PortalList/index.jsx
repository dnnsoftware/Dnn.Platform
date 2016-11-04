import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import Button from "dnn-button";
import { ListView } from "dnn-sites-common-components";
import Localization from "localization";
import utilities from "utils";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import GridCell from "dnn-grid-cell";
import { CommonPaginationActions, CommonPortalListActions } from "dnn-sites-common-actions";
import styles from "./style.less";
import Moment from "moment";

class PortalList extends Component {
    componentWillMount() {
        const {props} = this;
        // props.dispatch(CommonPortalListActions.deletePortal(16));
        props.dispatch(CommonPortalListActions.loadPortals({
            portalGroupId: props.pagination.portalGroupId,
            filter: props.pagination.filter,
            pageIndex: props.pagination.pageIndex,
            pageSize: props.pagination.pageSize
        }));
    }

    loadMore(event) {
        if (event) {
            event.preventDefault();
        }
        const { props } = this;
        props.dispatch(CommonPaginationActions.loadMore(() => {
            props.dispatch(CommonPortalListActions.loadPortals({
                portalGroupId: props.pagination.portalGroupId,
                filter: props.pagination.filter,
                pageIndex: props.pagination.pageIndex + 1,
                pageSize: props.pagination.pageSize
            }, true));
        }));
    }
    render() {
        const {props} = this;
        return (
            <GridCell className={styles.sitesPortalList}>
                <SocialPanelHeader title={"Sites"}>
                    <Button type="primary" onClick={props.onAddNewSite.bind(this)}>{"Add New Site"}</Button>
                </SocialPanelHeader>
                <SocialPanelBody>
                    <ListView
                        onAddNewSite={props.onAddNewSite.bind(this)}
                        />

                    {props.portals.length < props.totalCount &&
                        <GridCell className="load-more-button">
                            <Button type="primary" onClick={this.loadMore.bind(this)}>{Localization.get("LoadMore.Button")}</Button>
                        </GridCell>
                    }
                </SocialPanelBody>
            </GridCell>
        );
    }
}

PortalList.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onAddNewSite: PropTypes.func,
    onEditSite: PropTypes.func,
    portals: PropTypes.array
};
function mapStateToProps(state) {
    return {
        portals: state.portal.portals,
        totalCount: state.portal.totalCount,
        viewMode: state.viewMode,
        pagination: state.pagination
    };
}

export default connect(mapStateToProps)(PortalList);