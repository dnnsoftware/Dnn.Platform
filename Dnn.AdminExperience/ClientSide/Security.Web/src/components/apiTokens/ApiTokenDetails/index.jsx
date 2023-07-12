import React, { Component } from "react";
import { connect } from "react-redux";
import "./style.less";
import { Dropdown as Select, GridSystem as Grid, Switch, Button, SingleLineInputWithError, MultiLineInput, Label } from "@dnnsoftware/dnn-react-common";
import resx from "../../../resources";
import utils from "../../../utils";

class ApiTokenDetails extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        let scope = "";
        switch (this.props.apiToken.Scope) {
            case 0:
                scope = resx.get("User");
                break;
            case 1:
                scope = resx.get("Portal");
                break;
            case 2:
                scope = resx.get("Host");
                break;
        }
        let status = "";
        let statusClass = "inactive";
        if (utils.dateInPast(new Date(this.props.apiToken.ExpiresOn), new Date())) {
            status = resx.get("Expired");
        } else {
            if (this.props.apiToken.IsRevoked) {
                status = resx.get("Revoked");
            } else {
                status = resx.get("Active");
                statusClass = "active";
            }
        }

        return (
            <div className="apitokens-details">
                <Grid numberOfColumns={2}>
                    <div key="editor-container-columnOne" className="editor-container">
                        <Label
                            label={resx.get("CreatedBy")}
                            tooltipMessage={resx.get("CreatedBy.Help")}
                            tooltipPlace={"top"} />
                        <div>
                            {this.props.apiToken.CreatedByUser} ({this.props.apiToken.CreatedByUsername})
                        </div>
                        <Label
                            label={resx.get("CreatedOnDate")} />
                        <div>
                            {utils.formatDateTime(this.props.apiToken.CreatedOnDate)}
                        </div>
                        <Label
                            label={resx.get("ExpiresOn")}
                            tooltipMessage={resx.get("ExpiresOn.Help")}
                            tooltipPlace={"top"} />
                        <div>
                            {utils.formatDateTime(this.props.apiToken.ExpiresOn)}
                        </div>
                        <Label
                            label={resx.get("ApiTokenStatus")} />
                        <div className={statusClass}>
                            {status}
                        </div>
                        {this.props.apiToken.IsRevoked && (
                            <>
                                <Label
                                    label={resx.get("RevokedBy")}
                                    tooltipMessage={resx.get("RevokedBy.Help")}
                                    tooltipPlace={"top"} />
                                <div>
                                    {this.props.apiToken.RevokedByUser} ({this.props.apiToken.RevokedByUsername})
                                </div>
                                <Label
                                    label={resx.get("RevokedOn")} />
                                <div>
                                    {utils.formatDateTime(this.props.apiToken.RevokedOnDate)}
                                </div>
                            </>
                        )}
                    </div>
                    <div key="editor-container-columnTwo" className="editor-container right-column">
                        <Label
                            label={resx.get("Scope")}
                            tooltipMessage={resx.get("Scope.Help")}
                            tooltipPlace={"top"} />
                        <div>
                            {scope}
                        </div>
                        <Label
                            label={resx.get("ApiKeys")}
                            tooltipMessage={resx.get("ApiKeys.Help")}
                            tooltipPlace={"top"} />
                        <div className="apitokenkeys">
                            {this.props.apiToken.Keys.split(",").map((item) => {
                                let k = this.props.apiTokenKeys.filter((key) => {
                                    return key.Scope == this.props.apiToken.Scope && key.Key == item;
                                });
                                if (k.length > 0) {
                                    return (
                                        <Label
                                            label={k[0].Name}
                                            tooltipMessage={k[0].Description}
                                            tooltipPlace={"top"} />
                                    );
                                } else {
                                    return null;
                                }
                            })}
                        </div>
                    </div>
                </Grid>
                <div className="buttons-box">
                    <Button
                        type="secondary"
                        onClick={() => {
                            utils.utilities.confirm(resx.get("DeleteApiKey.Confirm"), resx.get("Yes"), resx.get("No"), () => {
                                alert("delete");
                            });
                        }}>
                        {resx.get("DeleteApiKey")}
                    </Button>
                    {statusClass == "active" && (
                        <Button
                            type="secondary"
                            onClick={() => {
                                utils.utilities.confirm(resx.get("Revoke.Confirm"), resx.get("Yes"), resx.get("No"), () => {
                                    alert("revoke");
                                });
                            }}>
                            {resx.get("Revoke")}
                        </Button>
                    )}
                </div>
            </div>);
    }

}

function mapStateToProps(state) {
    return {};
}

export default connect(mapStateToProps)(ApiTokenDetails);

