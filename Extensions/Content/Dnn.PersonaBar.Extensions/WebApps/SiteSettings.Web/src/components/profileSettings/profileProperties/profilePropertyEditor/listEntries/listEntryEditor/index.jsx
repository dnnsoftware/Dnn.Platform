import React, { Component } from "react";
import PropTypes from "prop-types";
import "./style.less";
import { SingleLineInputWithError, GridSystem, Switch, Label, Button, InputGroup } from "@dnnsoftware/dnn-react-common";
import resx from "resources";

class ListEntryEditor extends Component {
    constructor(props) {
        super();

        this.state = {
            entryDetail: {
                EntryId: props.entryId,
                PortalId: props.portalId,
                ListName: props.listName,
                Text: props.text,
                Value: props.value,
                EnableSortOrder: props.enableSortOrder
            },
            error: {
                Text: props.text === "" ? true : false,
                Value: props.value === "" ? true : false
            },
            triedToSubmit: false
        };
    }

    onSettingChange(key, event) {
        let { state } = this;
        let entryDetail = Object.assign({}, state.entryDetail);
        let value = typeof (event) === "object" ? event.target.value : event;
        state.error[key] = value === "" ? true : false;
        entryDetail[key] = value;

        this.setState({
            entryDetail: entryDetail,
            triedToSubmit: false,
            error: state.error
        });
    }

    onSave() {
        const { props, state } = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.Text || state.error.Value) {
            return;
        }

        props.onUpdate(state.entryDetail);
    }

    onCancel() {
        const { props } = this;
        props.Collapse();
    }

    /* eslint-disable react/no-danger */
    render() {
        if (this.state.entryDetail !== undefined || this.props.id === "add") {
            const columnOne = <div className="left-column">
                <InputGroup>
                    <Label
                        label={resx.get("ListEntryText")}
                    />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={this.state.error.Text && this.state.triedToSubmit}
                        errorMessage={resx.get("InvalidEntryText")}
                        value={this.state.entryDetail.Text}
                        onChange={this.onSettingChange.bind(this, "Text")}
                    />
                </InputGroup>
                {this.props.enableSortOrder === null &&
                    <InputGroup>
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("EnableSortOrder.Help")}
                            label={resx.get("EnableSortOrder")}
                        />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={this.state.entryDetail.EnableSortOrder}
                            onChange={this.onSettingChange.bind(this, "EnableSortOrder")}
                        />
                    </InputGroup>
                }
            </div>;
            const columnTwo = <div className="right-column">
                <InputGroup>
                    <Label
                        label={resx.get("ListEntryValue")}
                    />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={this.state.error.Value && this.state.triedToSubmit}
                        errorMessage={resx.get("InvalidEntryValue")}
                        value={this.state.entryDetail.Value}
                        onChange={this.onSettingChange.bind(this, "Value")}
                    />
                </InputGroup>
            </div>;

            return (
                <div className="entry-editor">
                    <GridSystem numberOfColumns={2}>{[columnOne, columnTwo]}</GridSystem>
                    <div className="editor-buttons-box">
                        <Button
                            type="secondary"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            type="primary"
                            onClick={this.onSave.bind(this)}>
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

ListEntryEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    entryDetail: PropTypes.object,
    entryId: PropTypes.number,
    listName: PropTypes.string,
    text: PropTypes.string,
    value: PropTypes.value,
    Collapse: PropTypes.func,
    onUpdate: PropTypes.func,
    id: PropTypes.string,
    portalId: PropTypes.number,
    enableSortOrder: PropTypes.bool
};

export default (ListEntryEditor);