import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import "./style.less";
import Grid from "dnn-grid-system";
import Label from "dnn-label";
import Button from "dnn-button";
import {
    importExport as ImportExportActions
} from "../../../actions";
import util from "../../../utils";
import Localization from "localization";

class JobDetails extends Component {
    constructor() {
        super();
    }

    componentWillMount() {
        const { props } = this;
        if (props.jobId) {
            props.dispatch(ImportExportActions.getJobDetails(props.jobId));
        }
        else {
            this.setState({
                jobDetail: {}
            });
        }
    }

    onDownloadLog() {
        const { props } = this;
    }

    renderedLogItemList() {
        const { props } = this;
        if (props.jobDetail.Summary.length > 0) {
            return props.jobDetail.Summary.map((logItem, index) => {
                return (
                    <div className="item-row divider">
                        <Label label={logItem.Name} style={{ margin: "0 0 5px 0" }} />
                        <div className="item-value">{logItem.Value}</div>
                    </div>
                );
            });
        }
        else return <div />;
    }

    /* eslint-disable react/no-danger */
    render() {
        const { props } = this;        
        if (props.jobDetail !== undefined) {
            const data = "text/json;charset=utf-8," + encodeURIComponent(JSON.stringify(props.jobDetail));
            const columnOne = <div className="container left-column">
                {this.renderedLogItemList()}
            </div>;
            const columnTwo = <div className="container right-column">
                <div className="item-row divider">
                    <Label label="test" style={{ margin: "0 0 5px 0" }} />
                </div>
                <a href={"data:'" + data + "'"} download="data.log">
                    <Button
                        type="secondary"
                        onClick={this.onDownloadLog.bind(this)}>
                        {Localization.get("DownloadLog")}
                    </Button>
                </a>
            </div>;

            return (
                <div className="job-details">
                    <div className="summary-title">{props.jobDetail.JobType.includes("Export") ? Localization.get("ExportSummary") : Localization.get("ImportSummary")}</div>
                    <Grid children={[columnOne, columnTwo]} numberOfColumns={2} />
                </div>
            );
        }
        else return <div></div>;
    }
}

JobDetails.propTypes = {
    dispatch: PropTypes.func.isRequired,
    jobDetail: PropTypes.object,
    jobId: PropTypes.number,
    Collapse: PropTypes.func,
    id: PropTypes.string
};

function mapStateToProps(state) {
    return {
        jobDetail: state.importExport.job
    };
}

export default connect(mapStateToProps)(JobDetails);