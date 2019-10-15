import React, {Component} from "react";
import PropTypes from "prop-types";
import ModulesRow from "./ModuleRow/ModuleRow";
import styles from "./style.less";
import { GridCell, SvgIcons, Modal } from "@dnnsoftware/dnn-react-common";
import Localization from "../../localization";
import utils from "../../utils";
import ModuleEdit from "./ModuleEdit/ModuleEdit";

class Modules extends Component {

    constructor() {
        super();

        this.state = {
            editType: ''
        };
    }

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

    onEditingModule(editType, module){
        const { onEditingModule } = this.props;
        this.setState({editType: editType}, () => {
            onEditingModule(module);
        });
    }

    getModules() {
        const {modules, editingSettingModuleId, showCopySettings, onModuleCopyChange} = this.props;

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
                    onEditing={this.onEditingModule.bind(this, 'content')}
                    onSetting={this.onEditingModule.bind(this, 'settings')}
                    isEditingModule={isEditingModule}
                    onCopyChange={onModuleCopyChange}
                    showCopySettings={showCopySettings} />
            );
        });
    }

    render() {
        const { state } = this;
        const {modules, onCancelEditingModule, editingSettingModuleId, showCopySettings, selectedPage} = this.props;
        const moduleRows = this.getModules();
        const editingModule = modules.find(m => m.id === editingSettingModuleId);
        return (
            /* eslint-disable react/no-danger */
            <div className={styles.moduleContainer}>
                <div className="module-title">
                        <div className="module-icon" dangerouslySetInnerHTML={{ __html: SvgIcons.ModuleIcon }} />
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
                <Modal 
                    isOpen={editingModule && state.editType !== ''} 
                    header={state.editType === "content" ? Localization.get("EditContent") : Localization.get("ModuleSettings")} 
                    onRequestClose={onCancelEditingModule}>
                    {editingModule && 
                        <ModuleEdit 
                            module={editingModule}
                            editType={state.editType}
                            onUpdatedModuleSettings={onCancelEditingModule}
                            selectedPage={selectedPage}
                            /> }
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
    showCopySettings: PropTypes.bool,
    selectedPage: PropTypes.object.isRequired
};

export default Modules;
