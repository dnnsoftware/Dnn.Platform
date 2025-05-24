import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import { Dropdown, GridSystem as Grid, Checkbox, Button, Label, SingleLineInput } from "@dnnsoftware/dnn-react-common";
import resx from "../../../resources";
import util from "../../../utils";

let isHost = false;
let isAdmin = false;

class CreateApiToken extends Component {
    constructor(props) {
        super(props);
        this.state = {
            currentScope: 2,
            apiTokenKeysFiltered: [],
            selectedKeys: [],
            selectedTimespan: props.timespanOptions.length > 0 ? props.timespanOptions[0].value : 0,
            tokenName: "",
            scopeOptions: (props.apiTokenSettings && props.apiTokenSettings.AllowApiTokens) ? props.scopeOptions : props.scopeOptions.slice(2)
        };
        isHost = util.settings.isHost;
        isAdmin = util.settings.isAdmin;
    }

    componentDidMount() {
        this.resetApiFilterList();
    }

    resetApiFilterList() {
        const f = this.props.apiTokenKeys.filter((item) => {
            return item.Scope === this.state.currentScope;
        });
        this.setState({
            apiTokenKeysFiltered: f,
            selectedKeys: []
        });
    }

    render() {
        return (
            <div className="apitokens-details">
                <Grid numberOfColumns={2}>
                    <div key="editor-container-columnOne" className="editor-container">
                        <Label
                            label={resx.get("TokenName")}
                            tooltipMessage={resx.get("TokenName.Help")}
                            tooltipPlace={"top"} />
                        <div>
                            <SingleLineInput
                                style={{ marginBottom: "0px", width: "100%" }}
                                value={this.state.tokenName}
                                onChange={(event) => {
                                    this.setState({
                                        tokenName: event.target.value
                                    });
                                }}
                                maxLength={100}
                            />
                        </div>
                        <Label
                            label={resx.get("Scope")}
                            tooltipMessage={resx.get("Scope.Help")}
                            tooltipPlace={"top"} />
                        <div>
                            <Dropdown
                                value={this.state.currentScope}
                                style={{ width: "100%" }}
                                options={this.state.scopeOptions}
                                withBorder={false}
                                onSelect={(value) => {
                                    this.setState({
                                        currentScope: value.value
                                    }, () => {
                                        this.resetApiFilterList();
                                    });
                                }}
                            />
                        </div>
                        {(isHost || isAdmin) && (
                            <>
                                <Label
                                    label={resx.get("ExpiresIn")}
                                    tooltipMessage={resx.get("ExpiresIn.Help")}
                                    tooltipPlace={"top"} />
                                <div>
                                    <Dropdown
                                        value={this.state.selectedTimespan}
                                        style={{ width: "100%" }}
                                        options={this.props.timespanOptions}
                                        withBorder={false}
                                        onSelect={(value) => {
                                            this.setState({
                                                selectedTimespan: value.value
                                            });
                                        }}
                                    />
                                </div>
                            </>
                        )}
                    </div>
                    <div key="editor-container-columnTwo" className="editor-container right-column">
                        <Label
                            label={resx.get("ApiKeys")}
                            tooltipMessage={resx.get("ApiKeys.Help")}
                            tooltipPlace={"top"} />
                        <div className="apitokenkeys">
                            {this.state.apiTokenKeysFiltered.map((item) => {
                                return (
                                    <div key={item.Key}>
                                        <Checkbox
                                            label={item.Name}
                                            value={this.state.selectedKeys.indexOf(item.Key) > -1}
                                            labelPlace="right"
                                            onChange={(e) => {
                                                let selectedKeys = this.state.selectedKeys;
                                                if (e) {
                                                    selectedKeys.push(item.Key);
                                                }
                                                else {
                                                    selectedKeys.splice(selectedKeys.indexOf(item.Key), 1);
                                                }
                                                this.setState({
                                                    selectedKeys: selectedKeys
                                                });
                                            }}
                                            tooltipMessage={item.Description} />
                                    </div>
                                );
                            })}
                        </div>
                    </div>
                </Grid>
                <div className="buttons-box">
                    <Button
                        type="neutral"
                        onClick={() => {
                            this.props.onCancel();
                        }}>
                        {resx.get("Cancel")}
                    </Button>
                    <Button
                        type="primary"
                        disabled={this.state.selectedKeys.length === 0}
                        onClick={() => {
                            this.props.onCreateApiToken(
                                this.state.tokenName,
                                this.state.currentScope,
                                this.state.selectedTimespan,
                                this.state.selectedKeys.join(","));
                        }}>
                        {resx.get("Add")}
                    </Button>
                </div>
            </div>);
    }
}

CreateApiToken.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    onCreateApiToken: PropTypes.func,
    apiTokenKeys: PropTypes.array,
    scopeOptions: PropTypes.array,
    timespanOptions: PropTypes.array,
    apiTokenSettings: PropTypes.object
};

function mapStateToProps(state) {
    return {
        apiTokenSettings: state.security.apiTokenSettings
    };
}

export default connect(mapStateToProps)(CreateApiToken);

