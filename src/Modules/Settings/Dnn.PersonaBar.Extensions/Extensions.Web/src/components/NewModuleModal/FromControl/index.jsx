import React, {PropTypes, Component} from "react";
import DropdownWithError from "dnn-dropdown-with-error";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import Switch from "dnn-switch";
import Button from "dnn-button";
import styles from "./style.less";

const inputStyle = { width: "100%" };

const newModuleTypes = [{
    label: "New",
    value: "New"
},
    {
        label: "Control",
        value: "Control"
    }, {
        label: "Manifest",
        value: "Manifest"
    }];

class FromControl extends Component {
    constructor() {
        super();
        this.state = {
            selectedType: ""
        };
    }

    onSelectNewModuleType(option) {
        this.setState({
            selectedType: option.value
        });
    }

    getCreateButtonActive() {
        return this.state.selectedType !== "New" && this.state.selectedType !== "Control" && this.state.selectedType !== "Manifest";
    }

    render() {
        const {props} = this;
        return (
            <div className={styles.fromControl}>
                <GridSystem className="with-right-border top-half">
                    <div>
                        <DropdownWithError
                            options={[{ label: "Container", value: "blah" }]}
                            tooltipMessage="Hey"
                            label="Owner Folder"
                            defaultDropdownValue="00"
                            className="owner-folder"
                            />
                        <DropdownWithError
                            options={[{ label: "Container", value: "blah" }]}
                            tooltipMessage="Hey"
                            label="Module Folder"
                            defaultDropdownValue="00"
                            className="module-folder"
                            />
                        <SingleLineInputWithError label="Resource" tooltipMessage="hey" style={inputStyle} inputStyle={{ marginBottom: 0 }}/>
                    </div>
                    <div>
                        <SingleLineInputWithError label="Module Name" tooltipMessage="hey" style={inputStyle} inputStyle={{ marginBottom: 32 }}/>
                        <MultiLineInputWithError label="Description" tooltipMessage="hey" style={inputStyle} inputStyle={{ marginBottom: 32, height: 126 }}/>
                        <Switch value={true} label="Add a Test Page:"/>
                    </div>
                </GridSystem>
            </div>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

FromControl.PropTypes = {
    onCancel: PropTypes.func
};

export default FromControl;