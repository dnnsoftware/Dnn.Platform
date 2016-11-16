import React, { Component, PropTypes } from "react";
import ReactDOM from "react-dom";
import Collapse from "react-collapse";
import "./style.less";
import { EditIcon } from "dnn-svg-icons";
import resx from "../../../resources";

const allowIcon = require(`!raw!./../../svg/checkbox.svg`);
const denyIcon = require(`!raw!./../../svg/cross_out.svg`);
const editIcon = require(`!raw!./../../svg/edit.svg`);
const deleteIcon = require(`!raw!./../../svg/trash.svg`);

class IpFilterRow extends Component {
    constructor() {
        super();
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
    getRuleTypeDisplay() {
        const {props} = this;
        if (props.id !== "add") {
            if (props.ruleType === 1) {
                return (
                    <div className="item-row-ruleType-display">
                        <div className="allow-icon" dangerouslySetInnerHTML={{ __html: allowIcon }} />
                        <div>{resx.get("AllowIP")}</div>
                    </div>
                );
            }
            else {
                return (
                    <div className="item-row-ruleType-display">
                        <div className="deny-icon" dangerouslySetInnerHTML={{ __html: denyIcon }} />
                        <div>{resx.get("DenyIP")}</div>
                    </div>
                );
            }
        }
        else {
            return "-";
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        if (props.visible) {
            return (
                <div className={"collapsible-component1"}>
                    <div className={"collapsible-header1 " + !opened} >
                        <div className={"row"}>
                            <div className="ip-filter-item item-row-ruleType">
                                {this.getRuleTypeDisplay()}
                            </div>
                            <div className="ip-filter-item item-row-ipAddress">
                                {props.ipFilter}
                            </div>
                            {props.id !== "add" &&
                                <div className="ip-filter-item item-row-editButton">
                                    <div className={opened ? "delete-icon-hidden" : "delete-icon"} dangerouslySetInnerHTML={{ __html: deleteIcon }} onClick={this.props.onDelete.bind(this)}></div>
                                    <div className={opened ? "edit-icon-active" : "edit-icon"} dangerouslySetInnerHTML={{ __html: editIcon }} onClick={this.toggle.bind(this)} />
                                </div>
                            }
                        </div>
                    </div>
                    <Collapse accordion={true} isOpened={opened} style={{ float: "left", width: "100%" }}>{opened && props.children}</Collapse>
                </div>
            );
        }
        else {
            return <div />;
        }
    }
}

IpFilterRow.propTypes = {
    ipFilterId: PropTypes.number,
    ipFilter: PropTypes.string,
    ruleType: PropTypes.number,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    onDelete: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    visible: PropTypes.bool
};

IpFilterRow.defaultProps = {
    collapsed: true,
    visible: true
};
export default (IpFilterRow);
