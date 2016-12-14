import React, {Component, PropTypes} from "react";
import GridCell from "dnn-grid-cell";
import { EditIcon, TrashIcon } from "dnn-svg-icons";
import styles from "./style.less";

class ModuleRow extends Component {

    render() {
        const { module, onDelete, onEditing, isEditingModule} = this.props;
        const editClassName = "extension-action" + (isEditingModule ? " selected" : "");
        return (
            /* eslint-disable react/no-danger */
            <div className={styles.moduleRow} >
                <GridCell columnSize={45} >
                    {module.title}
                </GridCell>
                <GridCell  columnSize={45} >
                    {module.friendlyName}
                </GridCell>
                <GridCell  columnSize={10} >
                    <div className="extension-action" dangerouslySetInnerHTML={{ __html: TrashIcon }} onClick={onDelete.bind(this, module)}></div>
                    <div className={editClassName} onClick={onEditing.bind(this, module)} dangerouslySetInnerHTML={{ __html: EditIcon }}></div>
                </GridCell>
            </div>
            /* eslint-enable react/no-danger */
        );
    }
}

ModuleRow.propTypes = {
    module: PropTypes.object.isRequired,
    isEditingModule: PropTypes.bool.isRequired,
    onDelete: PropTypes.func.isRequired,
    onEditing: PropTypes.func.isRequired
};

export default ModuleRow;