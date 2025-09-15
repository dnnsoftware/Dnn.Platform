import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import {
    PersonaBarPageHeader,
    PersonaBarPageBody,
    GridCell,
    Button
} from "@dnnsoftware/dnn-react-common";
import { ListView } from "dnn-sites-common-components";
import Localization from "localization";
import utilities from "utils/applicationSettings";
import {
    CommonPaginationActions,
    CommonPortalListActions
} from "dnn-sites-common-actions";
import styles from "./style.module.less";

class PortalList extends Component {
    UNSAFE_componentWillMount() {
        const { props } = this;
        props.dispatch(
            CommonPortalListActions.loadPortals({
                portalGroupId: props.pagination.portalGroupId,
                filter: props.pagination.filter,
                pageIndex: props.pagination.pageIndex,
                pageSize: props.pagination.pageSize
            })
        );
    }

    loadMore(event) {
        if (event) {
            event.preventDefault();
        }
        const { props } = this;
        props.dispatch(
            CommonPaginationActions.loadMore(() => {
                props.dispatch(
                    CommonPortalListActions.loadPortals(
                        {
                            portalGroupId: props.pagination.portalGroupId,
                            filter: props.pagination.filter,
                            pageIndex: props.pagination.pageIndex + 1,
                            pageSize: props.pagination.pageSize
                        },
                        true
                    )
                );
            })
        );
    }
    render() {
        const { props } = this;
        return (
            <GridCell className={styles.sitesPortalList}>
                <PersonaBarPageHeader title={Localization.get("Sites")}>
                    <Button
                        type="primary"
                        onClick={props.onAddNewSite.bind(this)}
                        size="large"
                    >
                        {Localization.get("AddNewSite")}
                    </Button>
                </PersonaBarPageHeader>
                <PersonaBarPageBody>
                    <ListView
                        onAddNewSite={props.onAddNewSite.bind(this)}
                        onExportPortal={props.onExportPortal.bind(this)}
                        culture={utilities.applicationSettings.cultureCode}
                    />

                    {props.portals.length < props.totalCount && (
                        <GridCell className="load-more-button">
                            <Button type="primary" onClick={this.loadMore.bind(this)}>
                                {Localization.get("LoadMore.Button")}
                            </Button>
                        </GridCell>
                    )}
                </PersonaBarPageBody>
            </GridCell>
        );
    }
}

PortalList.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onAddNewSite: PropTypes.func,
    onEditSite: PropTypes.func,
    onExportPortal: PropTypes.func,
    portals: PropTypes.array,
    pagination: PropTypes.object,
    totalCount: PropTypes.number,
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
