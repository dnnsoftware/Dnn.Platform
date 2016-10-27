import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import Button from "dnn-button";
import { ListView } from "dnn-sites-common-components";
import Localization from "localization";
import utilities from "utils";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import GridCell from "dnn-grid-cell";
import { portal as PortalActions } from "actions";
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
                        portals={props.portals}
                        utilities={utilities}
                        PortalActions={PortalActions}
                        />
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
        viewMode: state.viewMode
    };
}

export default connect(mapStateToProps)(PortalList);