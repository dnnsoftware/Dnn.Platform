import React, { Component } from "react";
import PropTypes from "prop-types";
import {
  GridCell,
  TextOverflowWrapper as OverflowText,
} from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";
import ServerRow from "./ServerRow";

export default class ServerList extends Component {
  constructor(props) {
    super(props);
  }

  getServerGridRows() {
    if (this.props.servers && this.props.servers.length > 0) {
      const rows = this.props.servers.map((field, i) => {
        return <ServerRow server={field} key={field.serverId} />;
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
          <GridCell columnSize={15}>
            {Localization.get("LastActivityDate")}
          </GridCell>
          <GridCell columnSize={15}>{Localization.get("Actions")}</GridCell>
        </div>
        {rows}
      </div>
    );
  }
}

ServerList.propTypes = {
  servers: PropTypes.array,
};
