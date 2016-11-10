import React, {PropTypes} from "react";
import Button from "dnn-button";
import Localization from "../../localization";
import styles from "./style.less";

const AddPages = ({addPagesData, onCancel, onSave, onChangeField}) => {
    return (
        <div className={styles.addPages}>
            <div className="left-column">
                <div className="column-heading">
                    {Localization.get("BulkPageSettings")}
                </div>
            </div>
            <div className="right-column">
                <div className="column-heading">
                    {Localization.get("BulkPagesToAdd")}
                </div>
            </div>
            <div className="buttons-box">
                <Button
                    type="secondary"
                    onClick={onCancel}>
                    {Localization.get("Cancel")}
                </Button>
                <Button
                    type="primary"
                    onClick={onSave}
                    disabled={!addPagesData.names}>
                    {Localization.get("AddPages")}
                </Button>
            </div>
        </div>
    );
};

AddPages.propTypes = {
    addPagesData: PropTypes.object.isRequired,
    onCancel: PropTypes.func.isRequired,
    onSave: PropTypes.func.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default AddPages;