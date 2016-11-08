import React, {Component, PropTypes} from "react";
import ModulesRow from "./ModuleRow/ModuleRow";
import styles from "./style.less";
import GridCell from "dnn-grid-cell";
import Localization from "../../localization";
import utils from "../../utils";
import { ModuleIcon } from "dnn-svg-icons";

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
        const {modules, absolutePageUrl, onToggleEditModule, editingSettingModuleId} = this.props;

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
                    absolutePageUrl={absolutePageUrl}
                    onDelete={this.onDeleteModule.bind(this)}
                    onToggleEditing={onToggleEditModule}
                    isEditingModule={isEditingModule} />
            );
        });
    }

    render() {
        const moduleRows = this.getModules();
        return (
            /* eslint-disable react/no-danger */
            <div className={styles.moduleContainer}>
                <div className="module-title">
                        <div className="module-icon" dangerouslySetInnerHTML={{ __html: ModuleIcon }} />
                        <div className="sectionTitle">{Localization.get("ModulesOnThisPage")}</div>
                    </div>
                <div className="module-table">    
                    <div className="header-row">
                        <GridCell columnSize={42} >
                            {Localization.get("Title")}
                        </GridCell>
                        <GridCell  columnSize={42} >
                            {Localization.get("Module")}
                        </GridCell>
                    </div>
                    {moduleRows}
                </div>      
            </div>      
            /* eslint-enable react/no-danger */
        );
    }
}

Modules.propTypes = {
    modules: PropTypes.array.isRequired,
    absolutePageUrl: PropTypes.string.isRequired,
    onDeleteModule: PropTypes.func.isRequired,
    onToggleEditModule: PropTypes.func.isRequired,
    editingSettingModuleId: PropTypes.number
};

export default Modules;
