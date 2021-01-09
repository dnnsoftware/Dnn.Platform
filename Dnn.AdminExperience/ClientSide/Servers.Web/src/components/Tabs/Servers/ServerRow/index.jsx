import React, { Component } from "react";
import PropTypes from "prop-types";
import {
  GridCell,
  TextOverflowWrapper,
  IconButton,
  Collapsible,
} from "@dnnsoftware/dnn-react-common";
import Localization from "../../../../localization";
import util from "../../../../utils";
import "./style.less";

export default class ServerRow extends Component {
  constructor(props) {
    super(props);
    this.state = {
      serverUnderEdit: -1,
      newUrl: "",
    };
  }

  toggleEdit() {
    if (this.props.inEdit) {
      this.props.collapse();
    } else {
      this.props.openCollapse();
    }
  }

  render() {
    const { server } = this.props;
    return (
      <div className={"collapsible-component1 " + this.props.inEdit}>
        <div className={"collapsible-header1 " + !this.props.inEdit}>
          <GridCell columnSize={30}>
            <TextOverflowWrapper text={server.serverName} />
          </GridCell>
          <GridCell columnSize={40}>
            <TextOverflowWrapper text={server.url} />
          </GridCell>
          <GridCell columnSize={15}>
            {util.formatDateNoTime(server.lastActivityDate)}
          </GridCell>
          <GridCell columnSize={15}>
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
          Some Content
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
};
