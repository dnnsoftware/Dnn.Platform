import PropTypes from "prop-types";
import React from "react";
import Resx from "../localization";
import {
  GridSystem as Grid,
  GridCell,
  SingleLineInputWithError,
  MultiLineInputWithError,
  Button,
} from "@dnnsoftware/dnn-react-common";
import AssignedSelector from "./AssignedSelector";
import "./Editor.less";

export default class SiteGroupEditor extends React.Component {
  constructor(props) {
    super(props);
  }

  render() {
    return (
      <div className="sitegroup-details-editor">
        <GridCell>
          <Grid numberOfColums={2}>
            <div className="editor-container">
              <div className="editor-row divider">
                <SingleLineInputWithError
                  value={this.props.group.MasterPortal.PortalName}
                  enabled={false}
                  label={Resx.get("MasterSite.Label")}
                  tooltipMessage={Resx.get("MasterSite.Help")}
                  inputStyle={{ marginBottom: 15 }}
                  tabIndex={1}
                />
              </div>
              <div className="editor-row divider">
                <SingleLineInputWithError
                  value={this.props.portalGroupName}
                  enabled={true}
                  onChange={(e) =>
                    this.props.onGroupNameChanged(e.target.value)
                  }
                  maxLength={50}
                  error={this.props.errors.groupName}
                  label={Resx.get("GroupName.Label")}
                  tooltipMessage={Resx.get("GroupName.Help")}
                  errorMessage={Resx.get("GroupName.Required")}
                  autoComplete="off"
                  inputStyle={{ marginBottom: 15 }}
                  tabIndex={2}
                />
              </div>
            </div>
            <div className="editor-container  right-column">
              <div className="editor-row divider">
                <SingleLineInputWithError
                  value={this.props.authenticationDomain}
                  enabled={true}
                  onChange={(e) =>
                    this.props.onAuthenticationDomainChanged(e.target.value)
                  }
                  maxLength={50}
                  label={Resx.get("AuthenticationDomain.Label")}
                  tooltipMessage={Resx.get("AuthenticationDomain.Help")}
                  autoComplete="off"
                  inputStyle={{ marginBottom: 15 }}
                  tabIndex={3}
                />
              </div>
              <div className="editor-row divider">
                <MultiLineInputWithError
                  value={this.props.description}
                  enabled={true}
                  onChange={(e) =>
                    this.props.onDescriptionChanged(e.target.value)
                  }
                  maxLength={50}
                  label={Resx.get("Description.Label")}
                  tooltipMessage={Resx.get("Description.Help")}
                  autoComplete="off"
                  inputStyle={{ marginBottom: 15 }}
                  tabIndex={4}
                />
              </div>
            </div>
          </Grid>
          <div className="selector-container">
            <AssignedSelector
              assignedPortals={this.props.portals}
              unassignedPortals={this.props.unassignedSites}
              onClickOnPortal={(i, t) => this.props.onClickOnPortal(i, t)}
              moveItemsLeft={() => this.props.onMoveItemsLeft()}
              moveItemsRight={() => this.props.onMoveItemsRight()}
              moveAll={(direction) => this.props.onMoveAll(direction)}
            />
          </div>
        </GridCell>
        <div className="buttons-box">
          {!this.props.isNew && (
            <Button
              type="secondary"
              onClick={() => this.props.onDeleteGroup(this.props.group)}
            >
              {Resx.get("Delete.Button")}
            </Button>
          )}
          <Button type="secondary" onClick={() => this.props.onCancel()}>
            {Resx.get("Cancel.Button")}
          </Button>
          <Button type="primary" onClick={() => this.props.onSave()}>
            {Resx.get("Save.Button")}
          </Button>
        </div>
      </div>
    );
  }
}

SiteGroupEditor.propTypes = {
  portalGroupName: PropTypes.string,
  errors: PropTypes.object,
  authenticationDomain: PropTypes.string,
  description: PropTypes.string,
  portals: PropTypes.array,
  group: PropTypes.object,
  unassignedSites: PropTypes.array,
  onCancel: PropTypes.func,
  onDeleteGroup: PropTypes.func,
  onSave: PropTypes.func,
  onGroupNameChanged: PropTypes.func,
  onDescriptionChanged: PropTypes.func,
  onAuthenticationDomainChanged: PropTypes.func,
  onClickOnPortal: PropTypes.func,
  onMoveItemsLeft: PropTypes.func,
  onMoveItemsRight: PropTypes.func,
  onMoveAll: PropTypes.func,
  isNew: PropTypes.bool,
};
