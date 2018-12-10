import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Button from "dnn-button";
import Localization from "localization";
import Label from "dnn-label";
import styles from "./style.less";

const inputStyle = { width: "100%" };
class JavascriptLibrary extends Component {
    getNameVersionRows(list) {
        return list.map((item, i) => {
            return <GridCell key={i} className="js-library-info-row">
                <GridCell columnSize={70} className="js-library-info-name">
                    <p>{item.name}</p>
                </GridCell>
                <GridCell columnSize={30} className="js-library-info-version">
                    <p>{item.version}</p>
                </GridCell>
            </GridCell>;
        });
    }
    getNameVersionTable(list, label, labelTooltip) {
        return [
            <Label
                label={label}
                tooltipMessage={labelTooltip}
                key="first" />,
            <hr className="jslibrary-table-separator" key="second" />,
            <GridCell className="js-library-info-table" key="third">
                <GridCell className="js-library-info-name-header" columnSize={70}>
                    {Localization.get("EditJavascriptLibrary_TableName.Header")}
                </GridCell>
                <GridCell className="js-library-info-version-header" columnSize={30}>
                    {Localization.get("EditJavascriptLibrary_TableVersion.Header")}
                </GridCell>
                {this.getNameVersionRows(list)}
            </GridCell>
        ];
    }
    render() {
        const {props} = this;
        const { extensionBeingEdited } = props;
        return (
            <GridCell className={styles.editJSLibrary + (props.className ? " " + props.className : "")}>
                <GridSystem className="with-right-border top-half">
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditJavascriptLibrary_LibraryName.Label")}
                            value={extensionBeingEdited.name.value}
                            tooltipMessage={Localization.get("EditJavascriptLibrary_LibraryName.HelpText")}
                            style={inputStyle}
                            enabled={false} />
                        <SingleLineInputWithError
                            label={Localization.get("EditJavascriptLibrary_FileName.Label")}
                            value={extensionBeingEdited.fileName.value}
                            tooltipMessage={Localization.get("EditJavascriptLibrary_FileName.HelpText")}
                            style={inputStyle}
                            enabled={false} />
                        <SingleLineInputWithError
                            label={Localization.get("EditJavascriptLibrary_PreferredLocation.Label")}
                            value={extensionBeingEdited.location.value}
                            tooltipMessage={Localization.get("EditJavascriptLibrary_PreferredLocation.HelpText")}
                            style={inputStyle}
                            enabled={false} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditJavascriptLibrary_LibraryVersion.Label")}
                            value={extensionBeingEdited.version.value}
                            tooltipMessage={Localization.get("EditJavascriptLibrary_LibraryVersion.HelpText")}
                            style={inputStyle}
                            enabled={false} />
                        <SingleLineInputWithError
                            label={Localization.get("EditJavascriptLibrary_ObjectName.Label")}
                            value={extensionBeingEdited.objectName.value}
                            tooltipMessage={Localization.get("EditJavascriptLibrary_ObjectName.Label")}
                            style={inputStyle}
                            enabled={false} />
                        <SingleLineInputWithError
                            label={Localization.get("EditJavascriptLibrary_DefaultCDN.Label")}
                            value={extensionBeingEdited.defaultCdn.value}
                            tooltipMessage={Localization.get("EditJavascriptLibrary_DefaultCDN.HelpText")}
                            style={inputStyle}
                            enabled={false} />
                        <SingleLineInputWithError
                            label={Localization.get("EditJavascriptLibrary_CustomCDN.Label")}
                            value={extensionBeingEdited.customCdn.value}
                            onChange={props.onChange.bind(this, "customCdn")}
                            tooltipMessage={Localization.get("EditJavascriptLibrary_CustomCDN.HelpText")}
                            enabled={!props.disabled}
                            style={inputStyle} />
                    </div>
                </GridSystem>
                <GridCell><hr /></GridCell>

                <GridSystem className="with-right-border top-half">
                    <div>
                        {this.getNameVersionTable(extensionBeingEdited.dependencies.value, Localization.get("EditJavascriptLibrary_DependsOn.Label"), Localization.get("EditJavascriptLibrary_DependsOn.HelpText"))}
                    </div>
                    <div>
                        {this.getNameVersionTable(extensionBeingEdited.usedBy.value, Localization.get("EditJavascriptLibrary_UsedBy.Label"), Localization.get("EditJavascriptLibrary_UsedBy.HelpText"))}
                    </div>
                </GridSystem>
                {!props.actionButtonsDisabled &&
                    <GridCell columnSize={100} className="modal-footer">
                        <Button type="secondary" onClick={props.onCancel.bind(this)}>{Localization.get("Cancel.Button")}</Button>
                        {!props.disabled && <Button type="primary" onClick={props.onSave.bind(this, true)}>{Localization.get("EditModule_SaveAndClose.Button")}</Button>}
                        {!props.disabled && <Button type="primary">{props.primaryButtonText}</Button>}
                    </GridCell>
                }
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

JavascriptLibrary.PropTypes = {
    onCancel: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    onChange: PropTypes.func,
    disabled: PropTypes.func,
    primaryButtonText: PropTypes.string
};
function mapStateToProps(state) {
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        extensionBeingEditedIndex: state.extension.extensionBeingEditedIndex
    };
}
export default connect(mapStateToProps)(JavascriptLibrary);
