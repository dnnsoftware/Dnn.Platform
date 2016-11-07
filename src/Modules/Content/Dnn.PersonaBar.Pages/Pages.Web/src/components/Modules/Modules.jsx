import React, {Component, PropTypes} from "react";
import ModulesRow from "./ModuleRow/ModuleRow";
import styles from "./style.less";
import GridCell from "dnn-grid-cell";
import Localization from "../../localization";

class Modules extends Component {

    onDeleteModule() {
        // TODO: to be implemented
    }

    getModules() {
        const {modules, absolutePageUrl} = this.props;

        return modules.map((module, index) => {
            return (
                <ModulesRow 
                    key={index}
                    module={module} 
                    absolutePageUrl={absolutePageUrl}
                    onDelete={this.onDeleteModule.bind(this)} />
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
    modules: PropTypes.array.isRequired,
    absolutePageUrl: PropTypes.string.isRequired
};

export default Modules;
