import React, { Component } from "react";
import PropTypes from "prop-types";
import {
  Button,
  GridCell,
  TextOverflowWrapper,
  IconButton,
  Collapsible,
  SingleLineInputWithError,
  SvgIcons,
} from "@dnnsoftware/dnn-react-common";
import Localization from "../../../../localization";
import util from "../../../../utils";
import "./style.less";
import serversTabService from "../../../../services/serversTabService";

export default class ServerRow extends Component {
  constructor(props) {
    super(props);
    this.state = {
      server: props.server,
      newUrl: props.server.url,
    };
  }

  toggleEdit() {
    if (this.props.inEdit) {
      this.props.collapse();
    } else {
      this.changeUrl(this.props.server.url);
      this.props.openCollapse();
    }
  }

  changeUrl(newUrl) {
    this.setState({
      newUrl: newUrl,
    });
  }

  updateUrl() {
    serversTabService
      .editServerUrl(this.props.server.serverId, this.state.newUrl)
      .then((response) => {
        if (response) {
          let server = this.state.server;
          server.url = this.state.newUrl;
          this.setState({
            server: server,
          });
        }
      })
      .catch(() => {
        util.notifyError(Localization.get("ServerUpdateUrlError"));
      });
    this.props.collapse();
  }

  deleteServer() {
    util.confirm(
      Localization.get("DeleteServerConfirm"),
      Localization.get("Confirm"),
      Localization.get("Cancel"),
      function () {
        this.props.deleteServer(this.props.server.serverId);
      }.bind(this),
    );
  }

  render() {
    const { server } = this.state;
    return (
      <div className={"collapsible-component1 " + this.props.inEdit}>
        <div className={"collapsible-header1 " + !this.props.inEdit}>
          <GridCell columnSize={5}>
            <span
              className={server.isActive ? "icon-flat active" : "icon-flat"}
            >
              <SvgIcons.ListViewIcon />
            </span>
          </GridCell>
          <GridCell columnSize={25}>
            <TextOverflowWrapper text={server.serverName} />
          </GridCell>
          <GridCell columnSize={40}>
            <TextOverflowWrapper text={server.url} />
          </GridCell>
          <GridCell columnSize={20}>
            {util.formatDateAndTime(server.lastActivityDate)}
          </GridCell>
          <GridCell columnSize={10}>
            <IconButton
              type="trash"
              className={"edit-icon"}
              onClick={this.deleteServer.bind(this)}
              title={Localization.get("Delete")}
            />
            <IconButton
              type="edit"
              className={"edit-icon " + !this.props.inEdit}
              onClick={this.toggleEdit.bind(this)}
              title={Localization.get("Edit")}
            />
          </GridCell>
        </div>
        <Collapsible
          accordion={true}
          isOpened={this.props.inEdit}
          className="role-row-collapsible"
        >
          <div key="editor-container-columnOne" className="editor-container">
            <SingleLineInputWithError
              value={this.state.newUrl}
              onChange={(e) => {
                this.changeUrl(e.target.value);
              }}
              label={Localization.get("Url")}
              tooltipMessage={Localization.get("Url.Help")}
              autoComplete="off"
              inputStyle={{ marginBottom: 0 }}
              tabIndex={1}
            />
          </div>
          <div className="buttons-box">
            <Button type="secondary" onClick={this.props.collapse.bind(this)}>
              {Localization.get("Cancel")}
            </Button>
            <Button type="primary" onClick={this.updateUrl.bind(this)}>
              {Localization.get("Save")}
            </Button>
          </div>
        </Collapsible>
      </div>
    );
  }
}

ServerRow.propTypes = {
  server: PropTypes.object,
  inEdit: PropTypes.bool,
  openCollapse: PropTypes.func,
  collapse: PropTypes.func,
  deleteServer: PropTypes.func,
};
