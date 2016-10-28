import React, {Component, PropTypes} from "react";
import ModulesRow from "./ModuleRow/ModuleRow";
import styles from "./style.less";
import GridCell from "dnn-grid-cell";
import Localization from "../../localization";

class Modules extends Component {

    onDeleteModule(){
        // TODO: to be implemented
    }

    onEditModule(){
        // TODO: to be implemented
    }

    getModules() {
        const {modules} = this.props;

        return modules.map((module, index) => {
            return (
                <ModulesRow 
                    key={index}
                    module={module} 
                    onDelete={this.onDeleteModule.bind(this)}
                    onEdit={this.onEditModule.bind(this)} />
            );
        });
    }

    render() {
        const moduleRows = this.getModules();
        return (
            <div className={styles.moduleContainer}>
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
        );
    }
}

Modules.propTypes = {
    modules: PropTypes.array
};

export default Modules;
