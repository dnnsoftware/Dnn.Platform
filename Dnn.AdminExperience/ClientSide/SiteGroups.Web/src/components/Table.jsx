import PropTypes from "prop-types";
import React from "react";
import Resx from "../localization";
import SiteGroupRow from "./Row";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import "./Table.less";

const tableFields = [
  {
    name: "GroupName",
    width: 45,
  },
  {
    name: "MasterSite",
    width: 45,
  },
  {
    name: "",
    width: 10,
  },
];

export default class SiteGroupsTable extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      renderIndex: -1,
    };
  }
  renderHeader() {
    let tableHeaders = tableFields.map((field) => {
      return (
        <GridCell
          key={field.name}
          columnSize={field.width}
          style={{ fontWeight: "bolder" }}
        >
          {field.name !== "" ? (
            <span>{Resx.get(field.name + ".Header")}</span>
          ) : (
            <span>&nbsp;</span>
          )}
        </GridCell>
      );
    });
    return (
      <div id="sitegroups-header-row" className="sitegroups-header-row">
        {tableHeaders}
      </div>
    );
  }
  renderList() {
    const existingGroupRows = (this.props.groups || []).map((group) => {
      return (
        <SiteGroupRow
          group={group}
          key={"PG_" + group.PortalGroupId.toString()}
          isOpened={
            (this.props.currentGroup &&
              this.props.currentGroup.PortalGroupId === group.PortalGroupId) ||
            false
          }
          unassignedSites={(this.props.unassignedSites || []).filter(
            (site) => site.PortalId !== group.MasterPortal.PortalId,
          )}
          onEditGroup={(g) => this.props.onEditGroup(g)}
          onSave={(g) => this.props.onSave(g)}
          onCancelEditing={() => this.props.onCancelEditing()}
          onDeleteGroup={(g) => this.props.onDeleteGroup(g)}
        ></SiteGroupRow>
      );
    });
    const newRow = this.props.currentGroup &&
      this.props.currentGroup.PortalGroupId === -1 && (
        <SiteGroupRow
          isOpened={true}
          key={"PG_new"}
          group={this.props.currentGroup}
          unassignedSites={(this.props.unassignedSites || []).filter(
            (site) =>
              site.PortalId !== this.props.currentGroup.MasterPortal.PortalId,
          )}
          onEditGroup={(g) => this.props.onEditGroup(g)}
          onSave={(g) => this.props.onSave(g)}
          onCancelEditing={() => this.props.onCancelEditing()}
          onDeleteGroup={(g) => this.props.onDeleteGroup(g)}
        ></SiteGroupRow>
      );
    const rows = newRow
      ? [newRow].concat(existingGroupRows)
      : existingGroupRows;
    if (rows.length > 0) {
      return rows;
    } else {
      return <div className="no-sitegroups-row">{Resx.get("NoData")}</div>;
    }
  }
  render() {
    return (
      <div className="sitegroups-list-container">
        <div className="container">
          {this.renderHeader()}
          {this.renderList()}
        </div>
      </div>
    );
  }
}
SiteGroupsTable.propTypes = {
  groups: PropTypes.array,
  unassignedSites: PropTypes.array,
  currentGroup: PropTypes.object,
  onEditGroup: PropTypes.func,
  onDeleteGroup: PropTypes.func,
  onCancelEditing: PropTypes.func,
  onSave: PropTypes.func,
};
