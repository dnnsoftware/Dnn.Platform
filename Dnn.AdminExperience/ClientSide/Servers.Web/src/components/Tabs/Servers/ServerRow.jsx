import React, { Component } from "react";
import PropTypes from "prop-types";
import {
  GridCell,
  OverflowText,
} from "@dnnsoftware/dnn-react-common";
import util from "../../../utils";

export default class ServerRow extends Component {
  constructor(props) {
    super(props);
    this.state = {
      serverUnderEdit: -1,
      newUrl: "",
    };
  }

  render() {
    const { server } = this.props;
    return (
      <div className="row">
        <GridCell columnSize={30}>
          <OverflowText text={server.serverName} />
        </GridCell>
        <GridCell columnSize={40}>
          <OverflowText text={server.url} />
        </GridCell>
        <GridCell columnSize={15}>
          {util.formatDateNoTime(server.lastActivityDate)}
        </GridCell>
        <GridCell columnSize={15}>TODO</GridCell>
      </div>
    );
  }
}

ServerRow.propTypes = {
  server: PropTypes.object,
  inEdit: PropTypes.bool,
};
