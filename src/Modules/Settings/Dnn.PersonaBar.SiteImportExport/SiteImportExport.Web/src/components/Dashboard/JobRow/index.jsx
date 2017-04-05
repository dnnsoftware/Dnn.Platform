import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import Collapse from "dnn-collapsible";
import { ArrowDownIcon, CycleIcon } from "dnn-svg-icons";
import Localization from "localization";
import "./style.less";

/*eslint-disable eqeqeq*/
class JobRow extends Component {
    constructor() {
        super();
        this.state = {
            collapsed: true,
            collapsedClass: true,
            repainting: false
        };
    }

    componentWillMount() {
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });
    }

    toggle() {
        if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
            this.props.Collapse();
        } else {
            this.props.OpenCollapse(this.props.id);
        }
    }

    getTypeIndicator() {
        const { props } = this;
        return props.jobType.includes("Import") ? <div className="jobIndicator-import"></div> : <div className="jobIndicator-export"></div>;
    }

    /* eslint-disable react/no-danger */
    render() {
        const { props, state } = this;
        let opened = (props.openId !== "" && this.props.id === props.openId);
        return (
            <div className={"collapsible-jobdetail " + !opened + (props.className ? (" " + props.className) : "")}>
                <div className={"collapsible-jobdetail-header " + state.collapsed}>
                    <div className="term-header">
                        <div className="term-label-cssclass" onClick={this.toggle.bind(this)}>
                            <div className="jobIndicator">
                                {this.getTypeIndicator()}
                            </div>
                        </div>
                        <div className="term-label-createdate" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span>{props.jobDate}</span>
                            </div>
                        </div>
                        <div className="term-label-jobtype" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span>{props.jobType}</span>
                            </div>
                        </div>
                        <div className="term-label-username" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span>{props.jobUser}&nbsp; </span>
                            </div>
                        </div>
                        <div className="term-label-portalname" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span>{props.jobPortal}&nbsp; </span>
                            </div>
                        </div>
                        <div className="term-label-jobstatus" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span className={"job-status" + (props.jobCancelled ? 4 : props.jobStatus)}>
                                    {Localization.get("JobStatus" + (props.jobCancelled ? 4 : props.jobStatus))}&nbsp;
                                    {props.jobStatus === 1 &&
                                        <div className="cycle-icon" dangerouslySetInnerHTML={{ __html: CycleIcon }} />
                                    }
                                </span>
                            </div>
                        </div>
                        <div className="term-label-arrow" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <div className="arrow-icon" dangerouslySetInnerHTML={{ __html: ArrowDownIcon }} />
                            </div>
                        </div>
                    </div>
                </div>
                <Collapse autoScroll={true} isOpened={opened} fixedHeight={450} style={{ float: "left", width: "100%" }}>{opened && props.children}</Collapse>
            </div>
        );
    }
}

JobRow.propTypes = {
    jobId: PropTypes.string,
    jobType: PropTypes.string,
    jobDate: PropTypes.string,
    jobUser: PropTypes.string,
    jobPortal: PropTypes.string,
    jobStatus: PropTypes.string,
    jobCancelled: PropTypes.bool,
    children: PropTypes.node,
    className: PropTypes.string,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string
};

function mapStateToProps(state) {
    return {
    };
}

export default connect(mapStateToProps)(JobRow);
