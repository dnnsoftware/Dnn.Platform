import React, { PropTypes, Component } from "react";
import resx from "resources";
import Switch from "dnn-switch";
import Button from "dnn-button";
import Label from "dnn-label";
import RadioButtons from "dnn-radio-buttons";
import GridCell from "dnn-grid-cell";
import ResourceTree from "./resourceTree";
import util from "utils";

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
            highlightPendingTranslations,
            portalName
        } = this.props;
        return (
            <GridCell className="edit-language-info">
                <GridCell className="edit-language-top-bar">
                    <GridCell columnSize={40} className="language-info">
                        <img src={languageBeingEdited.Icon} className="language-flag" alt={languageBeingEdited.NativeName} />
                        <p>{languageBeingEdited.NativeName}</p>
                    </GridCell>
                    <GridCell columnSize={60} className="mode-container">
                        {
                            util.settings.isHost && <RadioButtons tooltipMessage={resx.get("Mode.HelpText")} label={resx.get("Mode.Label")} buttonGroup="mode" options={ModeOptions} value={selectedMode} onChange={onSelectMode} />
                        }
                        {
                            !util.settings.isHost && <Label label={portalName} />
                        }
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
                    <GridCell columnSize={50} className="translation-action-buttons">
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
    highlightPendingTranslations: PropTypes.bool,
    portalName: PropTypes.string
};

export default LanguageInfoView;