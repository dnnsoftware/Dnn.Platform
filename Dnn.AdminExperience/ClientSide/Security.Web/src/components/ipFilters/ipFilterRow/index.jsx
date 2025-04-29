import React, { Component } from "react";
import PropTypes from "prop-types";
import { Collapsible } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import resx from "../../../resources";
import AllowIcon from "./../../svg/checkbox.svg";
import DenyIcon from "./../../svg/cross_out.svg";
import EditIcon from "./../../svg/edit.svg";
import DeleteIcon from "./../../svg/trash.svg";

class IpFilterRow extends Component {
  constructor() {
    super();
  }

  componentDidMount() {
    let opened =
      this.props.openId !== "" && this.props.id === this.props.openId;
    this.setState({
      opened,
    });
  }

  toggle() {
    if (this.props.openId !== "" && this.props.id === this.props.openId) {
      this.props.Collapse();
    } else {
      this.props.OpenCollapse(this.props.id);
    }
  }

  getRuleTypeDisplay() {
    const { props } = this;
    if (props.id !== "add") {
      if (props.ruleType === 1) {
        return (
          <div className="item-row-ruleType-display">
            <div className="allow-icon">
              <AllowIcon />
            </div>
            <div style={{ paddingLeft: "10px", paddingTop: 3, float: "left" }}>
              {resx.get("AllowIP")}
            </div>
          </div>
        );
      } else {
        return (
          <div className="item-row-ruleType-display">
            <div className="deny-icon">
              <DenyIcon />
            </div>
            <div style={{ paddingLeft: "10px", paddingTop: 3, float: "left" }}>
              {resx.get("DenyIP")}
            </div>
          </div>
        );
      }
    } else {
      return "-";
    }
  }

  render() {
    const { props } = this;
    let opened =
      this.props.openId !== "" && this.props.id === this.props.openId;

    return (
      <div
        className={
          "collapsible-component-ipfilters" + (opened ? " row-opened" : "")
        }
      >
        {props.visible && (
          <div className={"collapsible-ipfilters " + !opened}>
            <div className={"row"}>
              <div className="ip-filter-item item-row-ruleType">
                {this.getRuleTypeDisplay()}
              </div>
              <div className="ip-filter-item item-row-ipAddress">
                {props.ipFilter}
              </div>
              <div className="ip-filter-item item-row-notes">{props.notes}</div>
              {props.id !== "add" && !props.readOnly && (
                <div className="ip-filter-item item-row-editButton">
                  <div
                    className={opened ? "delete-icon-hidden" : "delete-icon"}
                    onClick={this.props.onDelete.bind(this)}
                  >
                    <DeleteIcon />
                  </div>
                  <div
                    className={opened ? "edit-icon-active" : "edit-icon"}
                    onClick={this.toggle.bind(this)}
                  >
                    <EditIcon />
                  </div>
                </div>
              )}
            </div>
          </div>
        )}
        <Collapsible
          className="ip-filter-wrapper"
          accordion={true}
          isOpened={opened}
        >
          {opened && props.children}
        </Collapsible>
      </div>
    );
  }
}

IpFilterRow.propTypes = {
  ipFilterId: PropTypes.number,
  ipFilter: PropTypes.string,
  notes: PropTypes.string,
  ruleType: PropTypes.number,
  OpenCollapse: PropTypes.func,
  Collapse: PropTypes.func,
  onDelete: PropTypes.func,
  id: PropTypes.string,
  openId: PropTypes.string,
  visible: PropTypes.bool,
  readOnly: PropTypes.bool,
  children: PropTypes.node,
};

IpFilterRow.defaultProps = {
  collapsed: true,
  visible: true,
  readOnly: false,
};
export default IpFilterRow;
