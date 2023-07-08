import React, { Component } from "react";
import { connect } from "react-redux";
import "./style.less";
import { Dropdown, GridSystem as Grid, Checkbox, Button, DatePicker, Label } from "@dnnsoftware/dnn-react-common";
import resx from "../../../resources";
import * as dayjs from "dayjs";

class CreateApiToken extends Component {
    constructor(props) {
        super(props);
        this.state = {
            currentScope: 0,
            apiTokenKeysFiltered: [],
            selectedKeys: [],
            selectedDate: dayjs().add(props.apiTokenSettings.MaximumTimespan, this.props.apiTokenSettings.MaximumTimespanMeasure).toDate()
        };
    }

    componentDidMount() {
        this.resetApiFilterList();
    }

    resetApiFilterList() {
        const f = this.props.apiTokenKeys.filter((item) => {
            return item.Scope == this.state.currentScope;
        });
        this.setState({
            apiTokenKeysFiltered: f,
            selectedKeys: []
        });
    }

    render() {
        const lastDate = dayjs().add(this.props.apiTokenSettings.MaximumTimespan, this.props.apiTokenSettings.MaximumTimespanMeasure).toDate();
        return (
            <div className="apitokens-details">
                <Grid numberOfColumns={2}>
                    <div key="editor-container-columnOne" className="editor-container">
                        <Label
                            label={resx.get("Scope")}
                            tooltipMessage={resx.get("Scope.Help")}
                            tooltipPlace={"top"} />
                        <div>
                            <Dropdown
                                value={this.state.currentScope}
                                style={{ width: "100%" }}
                                options={this.props.scopeOptions}
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
                        <Label
                            label={resx.get("ExpiresOn")}
                            tooltipMessage={resx.get("ExpiresOn.Help")}
                            tooltipPlace={"top"} />
                        <DatePicker
                            date={this.state.selectedDate}
                            updateDate={(date) => {
                                this.setState({
                                    selectedDate: date
                                });
                            }}
                            isDateRange={false}
                            hasTimePicker={true}
                            minDate={new Date()}
                            maxDate={lastDate}
                            showClearDateButton={false} />
                    </div>
                    <div key="editor-container-columnTwo" className="editor-container right-column">
                        <Label
                            label={resx.get("ApiKeys")}
                            tooltipMessage={resx.get("ApiKeys.Help")}
                            tooltipPlace={"top"} />
                        <div className="apitokenkeys">
                            {this.state.apiTokenKeysFiltered.map((item) => {
                                return (
                                    <div>
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
                        type="secondary"
                        onClick={() => {
                            this.props.onCancel();
                        }}>
                        {resx.get("Cancel")}
                    </Button>
                    <Button
                        type="primary"
                        disabled={this.state.selectedKeys.length === 0}
                        onClick={() => {
                            this.props.onCreateApiToken(this.state.currentScope, this.state.selectedDate.toISOString(), this.state.selectedKeys.join(","));
                        }}>
                        {resx.get("Add")}
                    </Button>
                </div>
            </div>);
    }
}

function mapStateToProps(state) {
    return {
        apiTokenSettings: state.security.apiTokenSettings
    };
}

export default connect(mapStateToProps)(CreateApiToken);

