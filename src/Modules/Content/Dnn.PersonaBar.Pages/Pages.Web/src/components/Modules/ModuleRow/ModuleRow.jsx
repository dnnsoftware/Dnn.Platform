import React, {Component, PropTypes} from "react";
import GridCell from "dnn-grid-cell";
import Collapse from "react-collapse";
import { EditIcon, TrashIcon } from "dnn-svg-icons";
import styles from "./style.less";
import ModuleEdit from "./ModuleEdit/ModuleEdit";

class ModuleRow extends Component {

    render() {
        const { module, onDelete, onToggleEditing, isEditingModule} = this.props;
        const editClassName = "extension-action" + (isEditingModule ? " selected" : "");
        return (
            /* eslint-disable react/no-danger */
            <div className={styles.moduleRow} >
                <GridCell columnSize={40} >
                    {module.title}
                </GridCell>
                <GridCell  columnSize={40} >
                    {module.friendlyName}
                </GridCell>
                <GridCell  columnSize={10} >
                    <div className="extension-action" dangerouslySetInnerHTML={{ __html: TrashIcon }} onClick={onDelete.bind(this, module)}></div>
                    <div className={editClassName} onClick={onToggleEditing.bind(this, module)} dangerouslySetInnerHTML={{ __html: EditIcon }}></div>
                </GridCell>
                <Collapse accordion={true} isOpened={isEditingModule} keepCollapsedContent={true} className="module-settings">
                    {isEditingModule && 
                        <ModuleEdit module={module} />}
                </Collapse>
            </div>
            /* eslint-enable react/no-danger */
        );
    }
}

ModuleRow.propTypes = {
    module: PropTypes.object.isRequired,
    isEditingModule: PropTypes.bool.isRequired,
    onDelete: PropTypes.func.isRequired,
    onToggleEditing: PropTypes.func.isRequired
};

export default ModuleRow;