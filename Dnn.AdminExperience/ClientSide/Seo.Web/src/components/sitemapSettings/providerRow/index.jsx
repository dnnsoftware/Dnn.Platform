import React, { Component } from "react";
import PropTypes from "prop-types";
import {
  Collapsible as Collapse,
  SvgIcons,
} from "@dnnsoftware/dnn-react-common";
import resx from "../../../resources";
import "./style.less";

class ProviderRow extends Component {
  constructor() {
    super();
  }

  UNSAFE_componentWillMount() {
    let opened =
      this.props.openId !== "" && this.props.name === this.props.openId;
    this.setState({
      opened,
    });
  }

  toggle() {
    if (this.props.openId !== "" && this.props.name === this.props.openId) {
      //this.props.Collapse();
    } else {
      this.props.OpenCollapse(this.props.name);
    }
  }

  getEnabledDisplay() {
    const { props } = this;
    if (props.enabled) {
      return (
        <div className="item-row-enabled-display">
          <div className="enabled-icon">
            <SvgIcons.CheckMarkIcon />
          </div>
        </div>
      );
    } else {
      return <div>&nbsp;</div>;
    }
  }

  render() {
    const { props } = this;
    let opened =
      this.props.openId !== "" && this.props.name === this.props.openId;
    if (props.visible) {
      return (
        <div
          className={
            "collapsible-component-providers" + (opened ? " row-opened" : "")
          }
        >
          <div className={"collapsible-providers " + !opened}>
            <div className={"row"}>
              <div className="provider-item item-row-name">{props.name}</div>
              <div className="provider-item item-row-enabled">
                {this.getEnabledDisplay()}
              </div>
              <div className="provider-item item-row-priority">
                {props.overridePriority ? props.priority : resx.get("None")}
              </div>
              <div className="provider-item item-row-editButton">
                <div
                  className={opened ? "edit-icon-active" : "edit-icon"}
                  onClick={this.toggle.bind(this)}
                >
                  <SvgIcons.EditIcon />
                </div>
              </div>
            </div>
          </div>
          <Collapse
            accordion={true}
            isOpened={opened}
            className="collapsible-providers-body"
          >
            {opened && props.children}
          </Collapse>
        </div>
      );
    } else {
      return <div />;
    }
  }
}

ProviderRow.propTypes = {
  name: PropTypes.string,
  enabled: PropTypes.bool,
  priority: PropTypes.number,
  overridePriority: PropTypes.bool,
  OpenCollapse: PropTypes.func,
  Collapse: PropTypes.func,
  id: PropTypes.string,
  openId: PropTypes.string,
  visible: PropTypes.bool,
  children: PropTypes.node,
};

ProviderRow.defaultProps = {
  collapsed: true,
  visible: true,
};

export default ProviderRow;
