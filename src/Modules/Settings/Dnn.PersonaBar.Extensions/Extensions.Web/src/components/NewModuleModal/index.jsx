import React, {PropTypes, Component} from "react";
import DropdownWithError from "dnn-dropdown-with-error";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import FromControl from "./FromControl";
import FromManifest from "./FromManifest";
import FromNew from "./FromNew";
import Dropdown from "dnn-dropdown";
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

class NewModuleModal extends Component {
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

    getCreateUI(selectedType) {
        switch (selectedType) {
            case "New":
                return <FromNew />;
            case "Control":
                return <FromControl />;
            case "Manifest":
                return <FromManifest />;
            default:
                return <div>Empty</div>;
        }
    }

    render() {
        const {props} = this;
        return (
            <div className={styles.newModuleModal}>
                <SocialPanelHeader title="Create New Module"/>
                <SocialPanelBody>
                    <GridCell className="new-module-box">
                        <GridCell columnSize={100} style={{ marginBottom: 15 }}>
                            <DropdownWithError
                                className="create-new-module-dropdown"
                                options={newModuleTypes}
                                tooltipMessage="Hey"
                                value={this.state.selectedType}
                                onSelect={this.onSelectNewModuleType.bind(this) }
                                label="Create New Module From:"
                                style={inputStyle}
                                />
                        </GridCell>
                        <GridCell>
                            {this.getCreateUI(this.state.selectedType) }
                        </GridCell>
                        <GridCell columnSize={100} className="modal-footer">
                            <Button type="secondary" onClick={props.onCancel.bind(this) }>Cancel</Button>
                            <Button type="primary" disabled={this.getCreateButtonActive() }>Next</Button>
                        </GridCell>
                    </GridCell>
                </SocialPanelBody>
            </div>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

NewModuleModal.PropTypes = {
    onCancel: PropTypes.func
};

export default NewModuleModal;