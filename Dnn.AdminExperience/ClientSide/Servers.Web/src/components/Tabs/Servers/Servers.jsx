import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridSystem, Label, GridCell } from "@dnnsoftware/dnn-react-common";
import InfoBlock from "../../common/InfoBlock";
import Localization from "../../../localization";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import ServersTabActions from "../../../actions/serversTab";
import utils from "../../../utils";

import "../tabs.less";
import ServerList from "./ServerList";

const defaultPlaceHolder = "...";

class Servers extends Component {
  componentDidMount() {
    this.props.onRetrieveServers();
  }

  UNSAFE_componentWillReceiveProps(newProps) {
    if (
      this.props.errorMessage !== newProps.errorMessage &&
      newProps.errorMessage
    ) {
      utils.notifyError(newProps.errorMessage);
    }
  }

  render() {
    const { props } = this;
    let serverName = "";
    props.servers.forEach((element) => {
      if (element.isCurrent) {
        serverName = element.serverName;
      }
    });

    return (
      <GridCell>
        <GridCell className="dnn-servers-info-panel">
          <GridSystem>
            <GridCell>
              <InfoBlock
                label={Localization.get("Servers_CurrentServerName")}
                tooltip={Localization.get("Servers_CurrentServerName.Help")}
                text={serverName}
              />
            </GridCell>
            <GridCell></GridCell>
          </GridSystem>
        </GridCell>
        <GridCell className="dnn-servers-grid-panel">
          <Label
            className="header-title"
            label={Localization.get("plWebServers")}
          />
          <ServerList servers={props.servers} />
        </GridCell>{" "}
      </GridCell>
    );
  }
}

Servers.propTypes = {
  errorMessage: PropTypes.string,
  onRetrieveServers: PropTypes.func.isRequired,
};

function mapStateToProps(state) {
  return {
    servers: state.serversTab.servers,
    errorMessage: state.serversTab.errorMessage,
  };
}

function mapDispatchToProps(dispatch) {
  return {
    ...bindActionCreators(
      {
        onRetrieveServers: ServersTabActions.loadServers,
      },
      dispatch
    ),
  };
}

export default connect(mapStateToProps, mapDispatchToProps)(Servers);
