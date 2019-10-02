import React, { Component } from "react";
import PropTypes from "prop-types";
import Localization from "localization";
import "./style.less";
import { Collapsible, SvgIcons, TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";

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

    componentDidMount() {
        let opened = (this.props.openId !== "" && this.props.jobId === this.props.openId);
        this.setState({
            opened
        });
    }

    toggle() {
        if ((this.props.openId !== "" && this.props.jobId === this.props.openId)) {
            this.props.Collapse();
        } else {
            this.props.OpenCollapse(this.props.jobId);
        }
    }

    getTypeIndicator() {
        const { props } = this;
        return props.jobType.indexOf("Import") >= 0 ? <div className="jobIndicator-import"></div> : <div className="jobIndicator-export"></div>;
    }

    /* eslint-disable react/no-danger */
    render() {
        const { props, state } = this;
        let opened = (props.openId !== "" && this.props.jobId === props.openId);
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
                                    <TextOverflowWrapper text={Localization.get("JobStatus" + (props.jobCancelled ? 4 : props.jobStatus))} maxWidth={80} />                                    
                                    {props.jobStatus === 1 && ! props.jobCancelled &&
                                        <div className="cycle-icon" dangerouslySetInnerHTML={{ __html: SvgIcons.CycleIcon }} />
                                    }
                                </span>
                            </div>
                        </div>
                        <div className="term-label-arrow" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <div className="arrow-icon" dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowDownIcon }} />
                            </div>
                        </div>
                    </div>
                </div>
                <Collapsible autoScroll={true} isOpened={opened} fixedHeight={480} style={{ float: "left", width: "100%" }}>{opened && props.children}</Collapsible>
            </div>
        );
    }
}

JobRow.propTypes = {
    jobId: PropTypes.number,
    jobType: PropTypes.string,
    jobDate: PropTypes.string,
    jobUser: PropTypes.string,
    jobPortal: PropTypes.string,
    jobStatus: PropTypes.number,
    jobCancelled: PropTypes.bool,
    children: PropTypes.node,
    className: PropTypes.string,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    openId: PropTypes.string
};

export default JobRow;
