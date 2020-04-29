import React, { PropTypes } from "react";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import itemDetailsActions from "../actions/itemDetailsActions";
import FolderDetails from "../components/FolderDetails";
import FileDetails from "../components/FileDetails";
import itemService from "../services/itemsService";
import localizeService from "../services/localizeService";

class ItemDetailsContainer extends React.Component {
    constructor(props) {
        super(props);
    }

    validateForm() {
        const {itemEditing, setValidationErrors} = this.props;
        let result = true;
        let validationErrors = {};

        if ((!itemEditing.isFolder && !itemEditing.fileName) || (itemEditing.isFolder && !itemEditing.folderName)) {
            validationErrors.fileName = localizeService.getString("ItemNameRequiredMessage");
            result = false;
        }

        setValidationErrors(validationErrors);
        return result;
    }
    formSubmit() {
        if (!this.validateForm()) {
            return;
        }

        const {itemEditing, saveItem} = this.props;
        saveItem(itemEditing);
    }
    render() {
        const {itemEditing, cancelEditItem,
            changeName, changeTitle, changeDescription} = this.props;
        const onSave = this.formSubmit.bind(this);
        const handlers = {changeName, changeTitle, changeDescription, cancelEditItem, onSave};
        let fileName, folderName, title, description;
        fileName = folderName = title = description = "";
        let item = {fileName, folderName, title, description};
        const isFolder = itemEditing ? itemEditing.isFolder : false;
        const validationErrors = this.props.validationErrors || {};

        if (itemEditing) {
            let iconStyle = {backgroundImage: "url('" + itemService.getIconUrl(itemEditing) + "')"};
            item = {...itemEditing, iconStyle};
        }

        return (
            isFolder ? 
                <FolderDetails folder={item} handlers={handlers} validationErrors={validationErrors} /> :
                <FileDetails file={item} handlers={handlers} validationErrors={validationErrors} />
        );
    }
}

ItemDetailsContainer.propTypes = {
    itemEditing: PropTypes.object,
    cancelEditItem: PropTypes.func,
    changeName: PropTypes.func,
    changeTitle: PropTypes.func,
    changeDescription: PropTypes.func,
    saveItem: PropTypes.func,
    setValidationErrors: PropTypes.func,
    validationErrors: PropTypes.object
};

function mapStateToProps(state) {
    const itemDetailsState = state.itemDetails;
    return {
        itemEditing: itemDetailsState.itemEditing,
        validationErrors: itemDetailsState.validationErrors
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            cancelEditItem: itemDetailsActions.cancelEditItem,
            changeName: itemDetailsActions.changeName,
            changeTitle: itemDetailsActions.changeTitle,
            changeDescription: itemDetailsActions.changeDescription,
            saveItem: itemDetailsActions.saveItem,
            setValidationErrors: itemDetailsActions.setValidationErrors
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(ItemDetailsContainer);
