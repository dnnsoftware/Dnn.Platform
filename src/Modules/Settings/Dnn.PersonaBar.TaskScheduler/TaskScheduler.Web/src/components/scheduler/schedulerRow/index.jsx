import React, {Component, PropTypes } from "react";
import Collapse from "dnn-collapsible";
import "./style.less";
import { EditIcon } from "dnn-svg-icons";
import ItemHistory from "../itemHistory";
import resx from "../../../resources";

/*eslint-disable quotes*/
const svgIcon = require(`!raw!./../../svg/checkmark.svg`);
const svgIcon2 = require(`!raw!./../../svg/history.svg`);

class SchedulerRow extends Component {
    constructor() {
        super();
        this.state = {
            historyPanelOpen: false
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
            //this.props.Collapse();
        } else {
            this.props.OpenCollapse(this.props.id);
        }
    }

    /* eslint-disable react/no-danger */
    getEnabledDisplay() {
        if (this.props.id !== "add") {
            if (this.props.enabled) {
                return <div className="checkMarkIcon" dangerouslySetInnerHTML={{ __html: svgIcon }}></div>;
            }
            else return <span>&nbsp; </span>;
        }
        else return <span>-</span>;
    }

    toggleHistoryPanel() {
        this.setState({
            historyPanelOpen: !this.state.historyPanelOpen
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        return (
            <div className={"collapsible-component-scheduler" + (opened ? " row-opened" : "")}>
                <div className={"collapsible-header-scheduler " + !opened} >
                    <div className={"row"}>
                        <div title={props.name} className="schedule-item item-row-name">
                            {props.name}&nbsp; </div>
                        <div className="schedule-item item-row-frequency">
                            {props.frequency}</div>
                        <div className="schedule-item item-row-retryTimeLapse">
                            {props.retryTimeLapse}</div>
                        <div className="schedule-item item-row-nextStart">
                            {props.nextStart}&nbsp; </div>
                        <div className="schedule-item item-row-enabled">
                            {this.getEnabledDisplay() }</div>                        
                        { props.id !== "add" &&
                            <div className="schedule-item item-row-historyButton">
                                <div className="history-icon" title={resx.get("ControlTitle_history")} dangerouslySetInnerHTML={{ __html: svgIcon2 }} onClick={this.toggleHistoryPanel.bind(this) }>
                                </div>
                                <div className="collapsible-content">
                                    <ItemHistory
                                        fixedHeight={500}
                                        isOpened={state.historyPanelOpen}
                                        onClose={this.toggleHistoryPanel.bind(this) }
                                        scheduleId={props.scheduleId}
                                        scheduleName={props.name}>
                                    </ItemHistory>
                                </div>
                            </div>
                        }
                        <div className="schedule-item item-row-editButton">
                            <div className={opened ? "edit-icon-active" : "edit-icon"} title={resx.get("ControlTitle_edit")} dangerouslySetInnerHTML={{ __html: EditIcon }} onClick={this.toggle.bind(this) }>
                            </div>
                        </div>
                    </div>
                </div>
                <Collapse autoScroll={true} isOpened={opened} style={{ float: "left" }} fixedHeight={600}>{opened && props.children }</Collapse>
            </div>
        );
    }
}

SchedulerRow.propTypes = {
    scheduleId: PropTypes.number,
    name: PropTypes.string,
    frequency: PropTypes.string,
    retryTimeLapse: PropTypes.string,
    enabled: PropTypes.bool,
    nextStart: PropTypes.string,
    disabled: PropTypes.bool,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string
};

SchedulerRow.defaultProps = {
    collapsed: true
};
export default (SchedulerRow);
