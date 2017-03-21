import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    jobs as JobsActions
} from "../../actions";
import DropDown from "dnn-dropdown";
import Pager from "dnn-pager";
import Button from "dnn-button";
import Label from "dnn-label";
import "./style.less";
import util from "../../utils";
import Localization from "localization";
import TextOverflowWrapper from "dnn-text-overflow-wrapper";

let isHost = false;
let currentPortalId = -1;

/*eslint-disable eqeqeq*/
class DashboardPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            jobs: [],
            portals: [],
            portalId: -1,
            pageIndex: 0,
            pageSize: 10
        };
        isHost = util.settings.isHost;
        currentPortalId = util.settings.portalId;
    }

    componentWillMount() {
        const { state, props } = this;
        this.setState({
            portalId: props.portalId || currentPortalId
        }, () => {
            if (isHost) {
                props.dispatch(JobsActions.getPortals(() => {
                }),
                    props.dispatch(JobsActions.getAllJobs(this.getNextPage()))
                );
            }
        });
    }

    getNextPage() {
        const { state } = this;
        return {
            portalId: state.portalId,
            pageIndex: state.pageIndex || 0,
            pageSize: state.pageSize
        };
    }

    getPortalOptions() {
        const { state, props } = this;
        let options = [];
        if (props.portals !== undefined) {
            options = props.portals.map((item) => {
                return {
                    label: Localization.get("SiteSelectionPrefix") + item.PortalName,
                    value: item.PortalID
                };
            });
        }
        return options;
    }

    onPortalChange(option) {
        const { props, state } = this;
        if (option.value !== state.portalId) {
            this.setState({
                portalId: option.value,
                pageIndex: 0
            }, () => {
                props.dispatch(JobsActions.getAllJobs(this.getNextPage()));
            });
        }
    }

    onImportData() {
        const { props } = this;
        props.dispatch(JobsActions.import());
    }

    onExportData() {
        const { props } = this;
        props.dispatch(JobsActions.export());
    }

    render() {
        const { props, state } = this;
        return (
            <div className="top-panel">
                <div className="site-selection">
                    <DropDown
                        options={this.getPortalOptions()}
                        value={state.portalId}
                        onSelect={this.onPortalChange.bind(this)}
                    />
                </div>
                <div className="last-actions">
                    <div className="action-labels">
                        <Label
                            labelType="block"
                            label={Localization.get("LastImport")} />
                        <Label
                            labelType="block"
                            label={Localization.get("LastExport")} />
                        <Label
                            labelType="block"
                            label={Localization.get("LastUpdate")} />
                    </div>
                    <div className="action-dates">
                        <div>1/31/2017 12:45 PM</div>
                        <div>1/31/2017 12:45 PM</div>
                        <div>1/31/2017 12:45 PM</div>
                    </div>
                </div>
                <div className="action-buttons">                    
                    <Button
                        className="action-button"
                        type="secondary"
                        onClick={this.onExportData.bind(this)}>
                        {Localization.get("ExportButton")}
                    </Button>
                    <Button
                        className="action-button"
                        type="secondary"
                        onClick={this.onImportData.bind(this)}>
                        {Localization.get("ImportButton")}
                    </Button>
                </div>
            </div>
        );
    }
}

DashboardPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    jobs: PropTypes.array,
    portals: PropTypes.array
};

function mapStateToProps(state) {
    return {
        jobs: state.jobs.jobs,
        portals: state.jobs.portals
    };
}

export default connect(mapStateToProps)(DashboardPanelBody);