import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell, GridSystem, SingleLineInputWithError, DropdownWithError, Switch, Button } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import "./style.less";

const inputStyle = { width: "100%" };

class ControlFields extends Component {
    constructor() {
        super();
    }
    onSelect(key, option) {
        const { props } = this;
        props.onChange(key, option.value);
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        return (
            <GridCell className="edit-module-control-box">
                <GridSystem>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("AddModuleControl_Key.Label")}
                            style={inputStyle}
                            tooltipMessage={Localization.get("AddModuleControl_Key.HelpText")}
                            value={props.controlBeingEdited.key}
                            onChange={props.onChange.bind(this, "key")} />
                      
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("AddModuleControl_Title.Label")}
                            tooltipMessage={Localization.get("AddModuleControl_Title.HelpText")}
                            style={inputStyle}
                            value={props.controlBeingEdited.title}
                            onChange={props.onChange.bind(this, "title")} />
                    </div>
                </GridSystem>
                <GridCell>
                    <DropdownWithError
                        label={Localization.get("AddModuleControl_SourceFolder.Label")}
                        tooltipMessage={Localization.get("AddModuleControl_SourceFolder.HelpText")}
                        style={inputStyle}
                        options={props.sourceFolders.map((folder) => {
                            return {
                                label: folder.Value,
                                value: folder.Value
                            };
                        })}
                        onSelect={props.onSelectSourceFolder.bind(this)}
                        value={props.selectedSourceFolder} />
                    <DropdownWithError
                        label={Localization.get("AddModuleControl_Source.Label") + "*"}
                        tooltipMessage={Localization.get("AddModuleControl_Source.HelpText")}
                        style={inputStyle}
                        options={props.sourceFiles.map((file) => {
                            return {
                                label: file.Value,
                                value: file.Value
                            };
                        })}
                        value={props.controlBeingEdited.source}
                        error={props.triedToSave && props.error.source}
                        onSelect={this.onSelect.bind(this, "source")}
                    />
                </GridCell>
                <GridSystem>
                    <div>
                        <DropdownWithError
                            label={Localization.get("AddModuleControl_Type.Label")}
                            tooltipMessage={Localization.get("AddModuleControl_Type.HelpText")}
                            style={inputStyle}
                            options={[
                                {
                                    label: "Theme Object",
                                    value: -2
                                },
                                {
                                    label: "Anonymous",
                                    value: -1
                                },
                                {
                                    label: "View",
                                    value: 0
                                },
                                {
                                    label: "Edit",
                                    value: 1
                                },
                                {
                                    label: "Admin",
                                    value: 2
                                },
                                {
                                    label: "Host",
                                    value: 3
                                }
                            ]}
                            value={props.controlBeingEdited.type}
                            onSelect={this.onSelect.bind(this, "type")} />
                        <DropdownWithError
                            label={Localization.get("AddModuleControl_Icon.Label")}
                            tooltipMessage={Localization.get("AddModuleControl_Icon.HelpText")}
                            style={inputStyle}
                            options={props.icons.map((icon) => {
                                return {
                                    label: icon.Value,
                                    value: icon.Value
                                };
                            })}
                            value={props.controlBeingEdited.icon}
                            onSelect={this.onSelect.bind(this, "icon")} />
                        <Switch value={props.controlBeingEdited.supportPopups}
                            label={Localization.get("AddModuleControl_SupportsPopups.Label")}
                            onText={Localization.get("SwitchOn")}
                            offText={Localization.get("SwitchOff")}
                            tooltipMessage={Localization.get("AddModuleControl_SupportsPopups.HelpText")}
                            onChange={props.onChange.bind(this, "supportPopups")} />
 
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("AddModuleControl_ViewOrder.Label")}
                            tooltipMessage={Localization.get("AddModuleControl_ViewOrder.HelpText")}
                            style={inputStyle}
                            value={props.controlBeingEdited.order}
                            onChange={props.onChange.bind(this, "order")} />
                        <SingleLineInputWithError
                            label={Localization.get("AddModuleControl_HelpURL.Label")}
                            tooltipMessage={Localization.get("AddModuleControl_HelpURL.HelpText")}
                            style={inputStyle}
                            value={props.controlBeingEdited.helpUrl}
                            onChange={props.onChange.bind(this, "helpUrl")} />
                        <Switch
                            label={Localization.get("AddModuleControl_SupportsPartialRendering.Label")}
                            onText={Localization.get("SwitchOn")}
                            offText={Localization.get("SwitchOff")}
                            tooltipMessage={Localization.get("AddModuleControl_SupportsPartialRendering.HelpText")}
                            value={props.controlBeingEdited.supportPartialRendering}
                            onChange={props.onChange.bind(this, "supportPartialRendering")} />
                    </div>
                </GridSystem>
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary" onClick={props.onCancel.bind(this)}>{Localization.get("AddModuleControl_Cancel.Button")}</Button>
                    <Button type="primary" onClick={props.onSave.bind(this)}>{Localization.get("AddModuleControl_Update.Button")}</Button>
                </GridCell>
            </GridCell>
        );
    }
}

ControlFields.PropTypes = {
    moduleDefinition: PropTypes.object,
    moduleDefinitionBeingEdited: PropTypes.object,
    onCancel: PropTypes.func,
    onSave: PropTypes.func,
    onChange: PropTypes.func,
    onEdit: PropTypes.func,
    error: PropTypes.object,
    triedToSave: PropTypes.bool
};

export default ControlFields;
