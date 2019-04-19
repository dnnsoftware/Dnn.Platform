import React, {Component} from "react";
import PropTypes from "prop-types";
import Localization from "localization";
import { Tooltip, Checkbox, SvgIcons } from "@dnnsoftware/dnn-react-common";

class Module extends Component {

    onUpdateModules(key, e) {
        const value = e.target ? e.target.value : e;
        const {index} = this.props;
        const {cultureCode} = this.props;
        this.props.onUpdateModules(cultureCode, index, value, key);
    }

    onDeleteModule(TabModuleId) {
        this.props.onDeleteModule(TabModuleId);
    }

    onRestoreModule(TabModuleId) {
        this.props.onRestoreModule(TabModuleId);
    }

    toggleLink() {
        const {module} = this.props;
        const value = !module.IsLocalized;
        this.onUpdateModules("IsLocalized", value);
    }

    /* eslint-disable react/no-danger */
    render() {
        const {module} = this.props;
        const {isDefault} = this.props;

        if (module.Exist) {
            const className = "module-row" + (module.IsDeleted ? " deleted" : "");
            const toolTip = [module.IsShared ? Localization.get("SharedModule") : Localization.get("IsNotSharedModule"), module.ModuleInfoHelp].join("<br>");
            return <div className={className}>
                <Tooltip
                    messages={[toolTip]}
                    style={{ float: "left", position: "static" }}
                    />
                <input type="text" value={module.ModuleTitle} onChange={this.onUpdateModules.bind(this, "ModuleTitle") } aria-label="Title"/>
                {module.IsDeleted && <div className="icons-container">
                    <span className="icon" onClick={this.onDeleteModule.bind(this, module.TabModuleId) } dangerouslySetInnerHTML={{ __html: SvgIcons.TrashIcon }} />
                    <span className="icon" onClick={this.onRestoreModule.bind(this, module.TabModuleId) } dangerouslySetInnerHTML={{ __html: SvgIcons.CycleIcon }} />
                </div>}
                {!module.IsDeleted && !isDefault && <div className="icons-container">
                    <span
                        className={`icon float-left ${(module.IsShared ? " disabled" : (module.IsLocalized ? " blue" : ""))}`}
                        onClick={this.toggleLink.bind(this) }
                        dangerouslySetInnerHTML={{ __html: SvgIcons.LinkIcon }} />
                    {module.TranslatedVisible && <Checkbox
                        style={{ float: "left" }}
                        value={module.IsTranslated}
                        onChange={this.onUpdateModules.bind(this, "IsTranslated") } />}
                </div>}
            </div>;
        }
        return <div className="module-row copy">
            <Checkbox
                style={{ float: "left" }}
                value={module.CopyModule}
                onChange={this.onUpdateModules.bind(this, "CopyModule") } />
            <div>{Localization.get("CopyModule") }</div>
        </div>;
    }
}

Module.propTypes = {
    isDefault: PropTypes.bool.isRequired,
    module: PropTypes.object.isRequired,
    onUpdateModules: PropTypes.func,
    onDeleteModule: PropTypes.func,
    onRestoreModule: PropTypes.func,
    cultureCode: PropTypes.string,
    index: PropTypes.number
};

export default Module;