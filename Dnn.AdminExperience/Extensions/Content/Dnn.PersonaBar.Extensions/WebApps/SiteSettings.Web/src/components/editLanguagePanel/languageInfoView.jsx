import React, { Component } from "react";
import PropTypes from "prop-types";
import resx from "resources";
import ResourceTree from "./resourceTree";
import { Switch, Button, Label, Flag, RadioButtons, GridCell, TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";
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
                        <div className="language-flag"><Flag culture={languageBeingEdited.Code} title={languageBeingEdited.NativeName} /></div>
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
                        <Switch label={resx.get("HighlightPendingTranslations")}
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={highlightPendingTranslations} onChange={onHighlightPendingTranslations} />
                    </GridCell>
                    <GridCell columnSize={50} className="translation-action-buttons">
                        <Button type="secondary" onClick={onCancel}>
                            <TextOverflowWrapper text={resx.get("Cancel") } maxWidth={100}/>
                        </Button>
                        <Button type="primary" onClick={onSaveTranslations} disabled={!resxBeingEdited}>
                            <TextOverflowWrapper text={resx.get("SaveTranslationsToFile") } maxWidth={100}/>
                        </Button>
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
    resxBeingEditedDisplay: PropTypes.string,
    selectedMode: PropTypes.string,
    onSelectMode: PropTypes.func,
    onCancel: PropTypes.func,
    onSaveTranslations: PropTypes.func,
    onHighlightPendingTranslations: PropTypes.func,
    highlightPendingTranslations: PropTypes.bool,
    portalName: PropTypes.string
};

export default LanguageInfoView;