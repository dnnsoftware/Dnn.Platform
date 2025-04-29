import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";
import ServerRow from "./ServerRow";

export default class ServerList extends Component {
  constructor(props) {
    super(props);
    this.state = {
      openId: -1,
    };
  }

  uncollapse(id) {
    setTimeout(() => {
      this.setState({
        openId: id,
      });
    }, this.timeout);
  }
  collapse() {
    if (this.state.openId !== -1) {
      this.setState({
        openId: -1,
      });
    }
  }
  toggle(openId) {
    if (openId !== -1) {
      this.uncollapse(openId);
    } else {
      this.collapse();
    }
  }

  getServerGridRows() {
    if (this.props.servers && this.props.servers.length > 0) {
      const rows = this.props.servers.map((field) => {
        return (
          <ServerRow
            server={field}
            key={field.serverId}
            inEdit={this.state.openId === field.serverId}
            openCollapse={this.toggle.bind(this, field.serverId)}
            collapse={this.collapse.bind(this, field.serverId)}
            deleteServer={this.props.deleteServer.bind(this)}
          />
        );
      });
      return rows;
    }
    return false;
  }

  render() {
    const rows = this.getServerGridRows();

    return (
      <div className="grid">
        <div className="row header">
          <GridCell columnSize={30}>{Localization.get("Name")}</GridCell>
          <GridCell columnSize={40}>{Localization.get("Url")}</GridCell>
          <GridCell columnSize={20}>
            {Localization.get("LastActivityDate")}
          </GridCell>
          <GridCell columnSize={10} className="right">
            {Localization.get("Actions")}
          </GridCell>
        </div>
        {rows}
      </div>
    );
  }
}

ServerList.propTypes = {
  servers: PropTypes.array,
  deleteServer: PropTypes.func,
};
