import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import DropdownWithError from "dnn-dropdown-with-error";
import Collapse from "react-collapse";
import { EditIcon, TrashIcon } from "dnn-svg-icons";
import Switch from "dnn-switch";
import Button from "dnn-button";
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
                            label="Key"
                            style={inputStyle}
                            tooltipMessage={"Placeholder"}
                            value={props.controlBeingEdited.key}
                            onChange={props.onChange.bind(this, "key")} />
                        <DropdownWithError
                            label="Source Folder"
                            style={inputStyle}
                            options={props.sourceFolders.map((folder) => {
                                return {
                                    label: folder.Value,
                                    value: folder.Value
                                };
                            })}
                            onSelect={props.onSelectSourceFolder.bind(this)}
                            value={props.selectedSourceFolder}
                            />
                        <DropdownWithError
                            label="Type"
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
                            onSelect={this.onSelect.bind(this, "type")}
                            />
                        <DropdownWithError
                            label="Icons"
                            style={inputStyle}
                            options={props.icons.map((icon) => {
                                return {
                                    label: icon.Value,
                                    value: icon.Value
                                };
                            })}
                            value={props.controlBeingEdited.icon}
                            onSelect={this.onSelect.bind(this, "icon")}
                            />
                        <Switch value={props.controlBeingEdited.supportPopups}
                            label={"Supports Popups?"}
                            tooltipMessage="Placeholder"
                            onChange={props.onChange.bind(this, "supportPopups")} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label="Title"
                            style={inputStyle}
                            value={props.controlBeingEdited.title}
                            onChange={props.onChange.bind(this, "title")}
                            tooltipMessage={"Placeholder"} />
                        <DropdownWithError
                            label="Source File*"
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
                        <SingleLineInputWithError
                            label="View Order"
                            style={inputStyle}
                            tooltipMessage={"Placeholder"}
                            value={props.controlBeingEdited.order}
                            onChange={props.onChange.bind(this, "order")} />
                        <SingleLineInputWithError
                            label="Help URL"
                            style={inputStyle}
                            tooltipMessage={"Placeholder"}
                            value={props.controlBeingEdited.helpUrl}
                            onChange={props.onChange.bind(this, "helpUrl")} />
                        <Switch
                            value={props.controlBeingEdited.supportPartialRendering}
                            onChange={props.onChange.bind(this, "supportPartialRendering")}
                            label={"Supports Partial Rendering?"}
                            tooltipMessage="Placeholder" />
                    </div>
                </GridSystem>
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary" onClick={props.onCancel.bind(this)}>Cancel</Button>
                    <Button type="primary" onClick={props.onSave.bind(this)}>Save</Button>
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