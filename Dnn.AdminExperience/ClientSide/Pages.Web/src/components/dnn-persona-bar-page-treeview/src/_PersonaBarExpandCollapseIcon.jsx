import React, { Component } from "react";
import { PropTypes } from "prop-types";
import "./styles.less";
import { SvgIcons } from "@dnnsoftware/dnn-react-common";

export default class PersonaBarExpandCollapseIcon extends Component {
  render() {
    const { isOpen, item } = this.props;

    return (
      <div className="parent-expand-icon">
        {isOpen ? (
          <div
            id={`collapse-${item.name}`}
            className="treeview-parent-collapse-button"
          >
            <SvgIcons.CollapseTree />
          </div>
        ) : (
          <div
            id={`expand-${item.name}`}
            className="treeview-parent-expand-button"
          >
            <SvgIcons.ExpandTree />
          </div>
        )}
      </div>
    );
  }
}

PersonaBarExpandCollapseIcon.propTypes = {
  isOpen: PropTypes.bool,
  item: PropTypes.object.isRequired,
};
