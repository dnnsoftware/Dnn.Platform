import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import FolderPicker from "../FolderPicker/FolderPicker";
import utils from "../../utils";
import {
    templateActions as TemplateActions
} from "../../actions";

class SaveAsTemplate extends Component {

    render() {
        const {template, onChangeField} = this.props;
        const serviceFramework = utils.getServiceFramework();

        return (
            <div>
                Work in progress
                <FolderPicker
                    serviceFramework={serviceFramework}
                    selectedFolder={template.folder}
                    onSelectFolder={(folder) => onChangeField("folder", folder)}
                    onServiceError={() => {}} />
            </div>
        );
    }
}

SaveAsTemplate.propTypes = {
    template: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        template: state.template.template
    };
}

function mapDispatchToProps(dispatch) {
    return bindActionCreators ({
        onChangeField: TemplateActions.changeTemplateField
    }, dispatch);
}

export default connect(mapStateToProps, mapDispatchToProps)(SaveAsTemplate);