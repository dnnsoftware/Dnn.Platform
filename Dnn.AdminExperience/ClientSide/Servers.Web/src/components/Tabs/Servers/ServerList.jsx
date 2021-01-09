import React, { Component } from "react";
import PropTypes from "prop-types";
import {
  GridCell,
  TextOverflowWrapper as OverflowText,
} from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";
import util from "../../../utils";
import TextEdit from "./TextEdit";

export default class ServerList extends Component {
  constructor(props) {
    super(props);
    this.state = {
      serverUnderEdit: -1,
      newUrl: "",
    };
  }

  editServerUrl(server) {
    this.setState({
      serverUnderEdit: server.serverId,
      newUrl: server.url,
    });
  }

  getServerGridRows() {
    if (this.props.servers && this.props.servers.length > 0) {
      const rows = this.props.servers.map((field, i) => {
        return (
          <div className="row" key={i}>
            <GridCell columnSize={30}>
              <OverflowText text={field.serverName} />
            </GridCell>
            <GridCell columnSize={40}>
              <TextEdit
                text={
                  this.state.serverUnderEdit == field.serverId
                    ? this.state.newUrl
                    : field.url
                }
                inEdit={this.state.serverUnderEdit == field.serverId}
                onChange={(t) =>
                  this.setState({
                    newUrl: t,
                  })
                }
                toggleEdit={(e) => {
                  this.editServerUrl(field);
                }}
              />
            </GridCell>
            <GridCell columnSize={15}>
              {util.formatDateNoTime(field.lastActivityDate)}
            </GridCell>
            <GridCell columnSize={15}>TODO</GridCell>
          </div>
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
