import React, {Component, PropTypes} from "react";
import GridCell from "dnn-grid-cell";
import Collapse from "react-collapse";
import { EditIcon, TrashIcon } from "dnn-svg-icons";
import styles from "./style.less";
import ModuleEdit from "./ModuleEdit/ModuleEdit";

class ModuleRow extends Component {

    constructor() {
        super();
        this.state = {
            editing: false
        };
    }

    onEdit() {
        const editing = this.state.editing;
        this.setState({editing: !editing});
    }

    render() {
        const {module, onDelete, absolutePageUrl} = this.props;
        const editClassName = "extension-action" + (this.state.editing ? " selected" : "");
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
                    <div className={editClassName} onClick={this.onEdit.bind(this)} dangerouslySetInnerHTML={{ __html: EditIcon }}></div>
                </GridCell>
                <Collapse accordion={true} isOpened={this.state.editing} keepCollapsedContent={true} className="module-settings">
                    {this.state.editing && 
                        <ModuleEdit absolutePageUrl={absolutePageUrl} module={module} />}
                </Collapse>
            </div>
            /* eslint-enable react/no-danger */
        );
    }
}

ModuleRow.propTypes = {
    module: PropTypes.object.isRequired,
    absolutePageUrl: PropTypes.string.isRequired,
    onDelete: PropTypes.func.isRequired
};

export default ModuleRow;