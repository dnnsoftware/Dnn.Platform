import React, {Component, PropTypes} from "react";
import Button from "dnn-button";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import FolderPicker from "../FolderPicker/FolderPicker";
import utils from "../../utils";
import styles from "./style.less";
import Localization from "../../localization";
import Label from "dnn-label";
import Switch from "dnn-switch";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import {
    templateActions as TemplateActions
} from "../../actions";

class SaveAsTemplate extends Component {

    onChangeField(key, event) {
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
    }

    render() {
        const {template, onChangeField, onSave, onCancel, errors} = this.props;
        const serviceFramework = utils.getServiceFramework();

        return (
            <div className={styles.saveAsTemplate}>
                <div className="grid-columns">
                    <div className="left-column">
                        <div className="input-group">
                            <Label
                                tooltipMessage={Localization.get("FolderTooltip")}
                                label={Localization.get("Folder")} />
                            <FolderPicker
                                serviceFramework={serviceFramework}
                                selectedFolder={template.folder}
                                onSelectFolder={(folder) => onChangeField("folder", folder)}
                                onServiceError={() => {}} />
                        </div>
                    </div>
                    <div className="right-column">
                        <div className="input-group">
                            <SingleLineInputWithError
                                label={Localization.get("TemplateName") + "*"}
                                tooltipMessage={Localization.get("TemplateNameTooltip")}
                                error={!!errors.name}
                                errorMessage={errors.name}
                                value={template.name} 
                                onChange={this.onChangeField.bind(this, "name")} />
                        </div>
                    </div>
                </div>
                <div className="input-group">
                    <MultiLineInputWithError
                        label={Localization.get("Description") + "*"}
                        value={template.description}
                        onChange={this.onChangeField.bind(this, "description")}
                        tooltipMessage={Localization.get("TemplateDescriptionTooltip")}
                        error={!!errors.description}
                        errorMessage={errors.description} />
                </div>
                <div className="input-group">
                    <Label
                        labelType="inline"
                        tooltipMessage={Localization.get("IncludeContentTooltip")}
                        label={Localization.get("IncludeContent")} />
                    <Switch
                        value={template.includeContent}
                        onChange={(value) => onChangeField("includeContent", value)} />
                    <div style={{clear: "both"}}></div>
                </div>
                <div className="buttons-box">
                    <Button
                        type="secondary"
                        onClick={onCancel}>
                        {Localization.get("Cancel")}
                    </Button>
                    <Button
                        type="primary"
                        onClick={onSave}
                        disabled={!template.name || !template.description}>
                        {Localization.get("Save")}
                    </Button>
                </div>
            </div>
        );
    }
}

SaveAsTemplate.propTypes = {
    template: PropTypes.object.isRequired,
    errors: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired,
    onSave: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        template: state.template.template,
        errors: state.template.errors
    };
}

function mapDispatchToProps(dispatch) {
    return bindActionCreators ({
        onChangeField: TemplateActions.changeTemplateField,
        onSave: TemplateActions.savePageAsTemplate
    }, dispatch);
}

export default connect(mapStateToProps, mapDispatchToProps)(SaveAsTemplate);