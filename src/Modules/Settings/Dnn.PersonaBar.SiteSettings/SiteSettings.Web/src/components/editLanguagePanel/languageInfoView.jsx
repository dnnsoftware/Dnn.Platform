import React, { PropTypes } from "react";
import resx from "resources";
import DropdownWithError from "dnn-dropdown-with-error";
import Switch from "dnn-switch";
import Button from "dnn-button";
import RadioButtons from "dnn-radio-buttons";
import GridCell from "dnn-grid-cell";
const languageInfoView = ({languageBeingEdited, ModeOptions}) => (
    <GridCell className="edit-language-info">
        <GridCell className="edit-language-top-bar">
            <GridCell columnSize={40} className="language-info">
                <img src={languageBeingEdited.Icon} className="language-flag" />
                <p>{languageBeingEdited.NativeName}</p>
            </GridCell>
            <GridCell columnSize={60} className="mode-container">
                <RadioButtons tooltipMessage="Placeholder" label="Mode:" buttonGroup="mode" options={ModeOptions} />
            </GridCell>
        </GridCell>
        <GridCell className="edit-resource-files">
            <GridCell columnSize={50}>
                <DropdownWithError label={resx.get("ResourceFolder")} options={[{ label: "blah", value: "blah" }]} />
                <Switch label={resx.get("HighlightPendingTranslations")} value={false} onChange={() => { } } />
            </GridCell>
            <GridCell columnSize={50}>
                <DropdownWithError label={resx.get("ResourceFile")} options={[{ label: "blah", value: "blah" }]} />
                <Button type="primary">{resx.get("SaveTranslationsToFile")}</Button>
                <Button type="secondary">{resx.get("Cancel")}</Button>
            </GridCell>
        </GridCell>
    </GridCell>
);

languageInfoView.propTypes = {
    languageBeingEdited: PropTypes.object,
    ModeOptions: PropTypes.array
};

export default languageInfoView;