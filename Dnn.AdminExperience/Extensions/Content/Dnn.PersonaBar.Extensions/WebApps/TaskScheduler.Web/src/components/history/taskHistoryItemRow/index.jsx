import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import styles from "./style.less";

/*eslint-disable quotes*/
const svgIcon = require(`!raw-loader!./svg/checkmark.svg`).default;

class TaskHistoryItemRow extends Component {
    constructor() {
        super();
        this.state = {
            collapsed: true,
            collapsedClass: true,
            repainting: false
        };
        this.timeout = 0;
        // setInterval(() => {
        //     console.log("Repainting: ", this.state.repainting);
        // }, 500);
        this.handleClick = this.handleClick.bind(this);
    }

    componentDidMount() {
        document.addEventListener("click", this.handleClick);
        this._isMounted = true;
    }

    componentWillUnmount() {
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
    }

    /*eslint-disable eqeqeq*/
    handleClick(event) {
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }

        if (!this.node.contains(event.target) && (typeof event.target.className == "string" && event.target.className.indexOf("do-not-close") == -1)) {

            this.timeout = 475;
        } else {

            this.timeout = 0;
        }
    }

    /* eslint-disable react/no-danger */
    getSucceededDisplay() {
        if (this.props.succeeded) {
            return <div className="checkMarkIcon" dangerouslySetInnerHTML={{ __html: svgIcon }}></div>;
        }
        else return <span>&nbsp; </span>;
    }

    /* eslint-disable react/no-danger */
    getLogNotesDisplay() {
        if (this.props.friendlyName.length > 0 || this.props.logNotes.length > 0) {
            return (
                <div>
                    <div>{this.props.friendlyName}</div>
                    <div dangerouslySetInnerHTML={{ __html: this.props.logNotes }}></div>
                </div>
            );
        }
        else return <span>&nbsp; </span>;
    }

    getStartEndDisplay() {
        let display = "<span>&nbsp;</span>";
        if (this.props.startDate) {
            display = "<p>S: " + this.props.startDate + "</p>";
        }
        if (this.props.endDate) {
            display += "<p>E: " + this.props.endDate + "</p>";
        }
        if (this.props.nextStart) {
            display += "<p>N: " + this.props.nextStart + "</p>";
        }
        return <div dangerouslySetInnerHTML={{ __html: display }}></div>;
    }

    render() {
        return (
            <div className={styles.taskHistoryItemRow} ref={node => this.node = node}>
                <div className="term-label-logNotes">
                    <div className="term-label-wrapper">
                        {this.getLogNotesDisplay()}
                    </div>
                </div>
                <div className="term-label-server">
                    <div className="term-label-wrapper">
                        <span>{this.props.server}&nbsp; </span>
                    </div>
                </div>
                <div className="term-label-elapsedTime">
                    <div className="term-label-wrapper">
                        <span>{this.props.elapsedTime}&nbsp; </span>
                    </div>
                </div>
                <div className="term-label-succeeded">
                    <div className="term-label-wrapper">
                        {this.getSucceededDisplay()}
                    </div>
                </div>
                <div className="term-label-startEnd">
                    <div className="term-label-wrapper">
                        {this.getStartEndDisplay()}
                    </div>
                </div>
            </div>
        );
    }
}

TaskHistoryItemRow.propTypes = {
    friendlyName: PropTypes.string,
    logNotes: PropTypes.string,
    server: PropTypes.string,
    elapsedTime: PropTypes.number,
    succeeded: PropTypes.bool,
    startDate: PropTypes.string,
    endDate: PropTypes.string,
    nextStart: PropTypes.string,
    children: PropTypes.node
};

function mapStateToProps() {
    return {};
}

export default connect(mapStateToProps)(TaskHistoryItemRow);
