import React, {Component, PropTypes} from "react";
import GridCell from "dnn-grid-cell";
import { EditIcon, TrashIcon } from "dnn-svg-icons";
import styles from "./style.less";

class ModuleRow extends Component {
    render() {
        const {module, onEdit, onDelete} = this.props;

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
                    <div className="extension-action" dangerouslySetInnerHTML={{ __html: TrashIcon }} onClick={onDelete}></div>
                    <div className="extension-action" onClick={onEdit(module.id)} dangerouslySetInnerHTML={{ __html: EditIcon }}></div>
                </GridCell>
            </div>
            /* eslint-enable react/no-danger */
        );
    }
}

ModuleRow.propTypes = {
    module: PropTypes.object,
    onEdit: PropTypes.func,
    onDelete: PropTypes.func
};

export default ModuleRow;