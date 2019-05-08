import React, { Component } from "react";
import PropTypes from "prop-types";
import { Collapsible } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import resx from "../../../resources";

/*eslint-disable quotes*/
const allowIcon = require(`!raw-loader!./../../svg/checkbox.svg`).default;
const denyIcon = require(`!raw-loader!./../../svg/cross_out.svg`).default;
const editIcon = require(`!raw-loader!./../../svg/edit.svg`).default;
const deleteIcon = require(`!raw-loader!./../../svg/trash.svg`).default;

class IpFilterRow extends Component {
    constructor() {
        super();
    }

    componentDidMount() {
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

    /* eslint-disable react/no-danger */
    getRuleTypeDisplay() {
        const {props} = this;
        if (props.id !== "add") {
            if (props.ruleType === 1) {
                return (
                    <div className="item-row-ruleType-display">
                        <div className="allow-icon" dangerouslySetInnerHTML={{ __html: allowIcon }} />
                        <div style={{ paddingLeft: "10px", paddingTop: 3, float: "left" }}>{resx.get("AllowIP") }</div>
                    </div>
                );
            }
            else {
                return (
                    <div className="item-row-ruleType-display">
                        <div className="deny-icon" dangerouslySetInnerHTML={{ __html: denyIcon }} />
                        <div style={{ paddingLeft: "10px", paddingTop: 3, float: "left" }}>{resx.get("DenyIP") }</div>
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
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);

        return (
            <div className={"collapsible-component-ipfilters" + (opened ? " row-opened" : "") }>
                {props.visible &&
                    <div className={"collapsible-ipfilters " + !opened} >
                        <div className={"row"}>
                            <div className="ip-filter-item item-row-ruleType">
                                {this.getRuleTypeDisplay() }
                            </div>
                            <div className="ip-filter-item item-row-ipAddress">
                                {props.ipFilter}
                            </div>
                            {props.id !== "add" && !props.readOnly &&
                                <div className="ip-filter-item item-row-editButton">
                                    <div className={opened ? "delete-icon-hidden" : "delete-icon"} dangerouslySetInnerHTML={{ __html: deleteIcon }} onClick={this.props.onDelete.bind(this) }></div>
                                    <div className={opened ? "edit-icon-active" : "edit-icon"} dangerouslySetInnerHTML={{ __html: editIcon }} onClick={this.toggle.bind(this) } />
                                </div>
                            }
                        </div>
                    </div>
                }
                <Collapsible className="ip-filter-wrapper" accordion={true} isOpened={opened} style={{ overflow: "visible", width: "100%" }}>{opened && props.children}</Collapsible>
            </div>
        );

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
    visible: PropTypes.bool,
    readOnly: PropTypes.bool
};

IpFilterRow.defaultProps = {
    collapsed: true,
    visible: true,
    readOnly: false
};
export default (IpFilterRow);
