import React, {Component, PropTypes} from "react";
import ModulesRow from "./ModuleRow/ModuleRow";
import styles from "./style.less";
import GridCell from "dnn-grid-cell";
import Localization from "../../localization";
import utils from "../../utils";
import { ModuleIcon } from "dnn-svg-icons";
import Modal from "dnn-modal";
import ModelEdit from "./ModuleEdit/ModuleEdit";

class Modules extends Component {

    onDeleteModule(module) {
        const {onDeleteModule} = this.props;
        utils.confirm(
            Localization.get("DeleteModuleConfirm").replace("[MODULETITLE]", module.title),
            Localization.get("Delete"),
            Localization.get("Cancel"),
            function onConfirmDeletion() {
                onDeleteModule(module);
            });
    }

    getModules() {
        const {modules, onEditingModule, editingSettingModuleId, showCopySettings, onModuleCopyChange} = this.props;

        if (modules.length === 0) {
            return <GridCell className="no-modules" columnSize={100} >
                        {Localization.get("NoModules")}
                    </GridCell>;
        }

        return modules.map((module, index) => {
            const isEditingModule = module.id === editingSettingModuleId;
            return (
                <ModulesRow 
                    key={index}
                    module={module} 
                    onDelete={this.onDeleteModule.bind(this)}
                    onEditing={onEditingModule}
                    isEditingModule={isEditingModule} 
                    onCopyChange={onModuleCopyChange}
                    showCopySettings={showCopySettings} />
            );
        });
    }

    render() {
        const {modules, onCancelEditingModule, editingSettingModuleId, showCopySettings} = this.props;
        const moduleRows = this.getModules();
        const editingModule = modules.find(m => m.id === editingSettingModuleId);
        return (
            /* eslint-disable react/no-danger */
            <div className={styles.moduleContainer}>
                <div className="module-title">
                        <div className="module-icon" dangerouslySetInnerHTML={{ __html: ModuleIcon }} />
                        <div className="sectionTitle">{Localization.get("ModulesOnThisPage")}</div>
                    </div>
                <div className="module-table">    
                    <div className="header-row">
                        {showCopySettings &&
                            <GridCell columnSize={10} />
                        }
                        <GridCell columnSize={showCopySettings ? 25: 45} >
                            {Localization.get("Title")}
                        </GridCell>
                        <GridCell  columnSize={showCopySettings ? 25: 45} >
                            {Localization.get("Module")}
                        </GridCell>
                    </div>
                    {moduleRows}
                </div>
                <Modal isOpen={editingModule} header={Localization.get("ModuleSettings")} onRequestClose={onCancelEditingModule}>
                    {editingModule && <ModelEdit module={editingModule} onUpdatedModuleSettings={onCancelEditingModule} /> }
                </Modal>      
            </div>      
            /* eslint-enable react/no-danger */
        );
    }
}

Modules.propTypes = {
    modules: PropTypes.array.isRequired,
    onDeleteModule: PropTypes.func.isRequired,
    onEditingModule: PropTypes.func.isRequired,
    onCancelEditingModule: PropTypes.func.isRequired,
    editingSettingModuleId: PropTypes.number,
    onModuleCopyChange: PropTypes.func,
    showCopySettings: PropTypes.bool
};

export default Modules;
