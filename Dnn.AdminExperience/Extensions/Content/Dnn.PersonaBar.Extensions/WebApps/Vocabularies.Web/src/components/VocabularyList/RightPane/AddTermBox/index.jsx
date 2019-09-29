import React from "react";
import PropTypes from "prop-types";
import { Scrollbars } from "react-custom-scrollbars";
import { 
    MultiLineInputWithError,
    SingleLineInputWithError,
    Button,
    InputGroup
} from "@dnnsoftware/dnn-react-common";
import Collapsible from "react-collapse";
import LocalizedResources from "../../../../resources";
import styles from "./style.less";

const parentTermTreeStyle = {
    width: "100%",
    height: "110px",
    border: "1px solid #BFBFBF",
    borderTop: "none",
    background: "white",
    boxSizing: "border-box"
};

const AddTermBox = ({
    isOpened,
    editMode,
    termBeingEdited,
    termTreeVisible,
    parentDisplay,
    parentTermTree,
    parentTreeOpened,
    toggleParentTree,
    onTermValueChange,
    deleteTerm,
    closeAddTerm,
    onUpdateTerm,
    error
}) => (
    <Collapsible
        isOpened={isOpened}>
        <div className={styles.addTermBox}>
            <p className="add-term-title">{editMode ? LocalizedResources.get("CurrentTerm") : LocalizedResources.get("AddTerm") }</p>
            <InputGroup>
                <SingleLineInputWithError
                    inputId={"create-term-name"}
                    withLabel={true}
                    label={LocalizedResources.get("TermName") + "*"}
                    value={termBeingEdited.Name}
                    onChange={onTermValueChange.bind(this, "Name") }
                    error={error}
                    errorMessage={LocalizedResources.get("TermValidationError.Message") }
                />
            </InputGroup>
            <InputGroup>
                <MultiLineInputWithError
                    inputId={"create-term-description"}
                    withLabel={true}
                    label={LocalizedResources.get("Description")}
                    value={termBeingEdited.Description}
                    onChange={onTermValueChange.bind(this, "Description") }/>
            </InputGroup>
            {termTreeVisible &&
                <InputGroup style={{ marginBottom: 32 }}>
                    <label>{LocalizedResources.get("ParentTerm") }</label>
                    <p className="parent-display" onClick={toggleParentTree}>{parentDisplay && parentDisplay.Name || ""}</p>
                    <Collapsible
                        isOpened={parentTreeOpened}
                        fixedHeight={115}
                        keepCollapsedContent={true}>
                        <Scrollbars style={parentTermTreeStyle}>
                            <ul className="term-ul root-level parent-tree">
                                {parentTermTree}
                            </ul>
                        </Scrollbars>
                    </Collapsible>
                </InputGroup>
            }
            <div className="add-term-footer">
                {editMode && <Button type="secondary" onClick={deleteTerm}>{LocalizedResources.get("DeleteTerm") }</Button>}
                <Button type="secondary" onClick={closeAddTerm}>{LocalizedResources.get("cancelCreate") }</Button>
                <Button type="primary" onClick={onUpdateTerm}>{LocalizedResources.get("SaveTerm") }</Button>
            </div>
        </div>
    </Collapsible>
);


AddTermBox.propTypes = {
    isOpened: PropTypes.bool,
    editMode: PropTypes.bool,
    termBeingEdited: PropTypes.object,
    termTreeVisible: PropTypes.bool,
    parentDisplay: PropTypes.object,
    parentTermTree: PropTypes.node,
    parentTreeOpened: PropTypes.bool,
    toggleParentTree: PropTypes.func,
    onTermValueChange: PropTypes.func,
    deleteTerm: PropTypes.func,
    closeAddTerm: PropTypes.func,
    onUpdateTerm: PropTypes.func,
    error: PropTypes.bool
};

export default AddTermBox;