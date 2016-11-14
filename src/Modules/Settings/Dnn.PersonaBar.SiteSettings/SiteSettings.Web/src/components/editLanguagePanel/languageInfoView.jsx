import React, { PropTypes, Component } from "react";
import ReactDOM from "react-dom";
import resx from "resources";
import DropdownWithError from "dnn-dropdown-with-error";
import Switch from "dnn-switch";
import Button from "dnn-button";
import RadioButtons from "dnn-radio-buttons";
import GridCell from "dnn-grid-cell";
import { Scrollbars } from "react-custom-scrollbars";
import { ArrowDownIcon } from "dnn-svg-icons";
import Folder from "./folder";
import Collapse from "react-collapse";
import ResourceTree from "./resourceTree";

class LanguageInfoView extends Component {
    render() {
        const {
            languageBeingEdited,
            ModeOptions,
            languageFolders,
            getChildFolders,
            getResxEntries,
            resxBeingEdited,
            resxBeingEditedDisplay,
            selectedMode,
            onSelectMode,
            onCancel,
            onSaveTranslations,
            onHighlightPendingTranslations,
            highlightPendingTranslations
        } = this.props;
        return (
            <GridCell className="edit-language-info">
                <GridCell className="edit-language-top-bar">
                    <GridCell columnSize={40} className="language-info">
                        <img src={languageBeingEdited.Icon} className="language-flag" />
                        <p>{languageBeingEdited.NativeName}</p>
                    </GridCell>
                    <GridCell columnSize={60} className="mode-container">
                        <RadioButtons tooltipMessage="Placeholder" label="Mode:" buttonGroup="mode" options={ModeOptions} value={selectedMode} onChange={onSelectMode} />
                    </GridCell>
                </GridCell>
                <GridCell className="edit-resource-files">
                    <ResourceTree
                        languageFolders={languageFolders}
                        getChildFolders={getChildFolders}
                        getResxEntries={getResxEntries}
                        resxBeingEdited={resxBeingEdited}
                        resxBeingEditedDisplay={resxBeingEditedDisplay} />
                    <GridCell columnSize={50}>
                        <Switch label={resx.get("HighlightPendingTranslations")} value={highlightPendingTranslations} onChange={onHighlightPendingTranslations} />
                    </GridCell>
                    <GridCell columnSize={100} className="translation-action-buttons">
                        <Button type="secondary" onClick={onCancel}>{resx.get("Cancel")}</Button>
                        <Button type="primary" onClick={onSaveTranslations} disabled={!resxBeingEdited}>{resx.get("SaveTranslationsToFile")}</Button>
                    </GridCell>
                </GridCell>
            </GridCell>
        );
    }
}

LanguageInfoView.propTypes = {
    languageBeingEdited: PropTypes.object,
    ModeOptions: PropTypes.array,
    languageFolders: PropTypes.array,
    languageFiles: PropTypes.array,
    getChildFolders: PropTypes.func,
    getResxEntries: PropTypes.func,
    resxBeingEdited: PropTypes.string,
    onToggleTree: PropTypes.func,
    resxBeingEditedDisplay: PropTypes.func,
    selectedMode: PropTypes.string,
    onSelectMode: PropTypes.func,
    onCancel: PropTypes.func,
    onSaveTranslations: PropTypes.func,
    onHighlightPendingTranslations: PropTypes.func,
    highlightPendingTranslations: PropTypes.bool
};

export default LanguageInfoView;