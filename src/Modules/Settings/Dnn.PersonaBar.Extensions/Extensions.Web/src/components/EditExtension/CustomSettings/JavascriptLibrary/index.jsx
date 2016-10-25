import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import Button from "dnn-button";
import Localization from "localization";
import styles from "./style.less";

const inputStyle = { width: "100%" };
function formatVersionNumber(n) {
    return n > 9 ? "" + n : "0" + n;
}
class JavascriptLibrary extends Component {
    render() {
        const {props, state} = this;

        return (
            <GridCell className={styles.editJavascriptLibrary}>
                <GridSystem className="with-right-border top-half">
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditJavascriptLibrary_LibraryName.Label")}
                            tooltipMessage={Localization.get("EditJavascriptLibrary_LibraryName.HelpText")}
                            style={inputStyle}
                            enabled={false} />
                        <SingleLineInputWithError
                            label={Localization.get("EditJavascriptLibrary_FileName.Label")}
                            tooltipMessage={Localization.get("EditJavascriptLibrary_FileName.HelpText")}
                            style={inputStyle}
                            enabled={false} />
                        <SingleLineInputWithError
                            label={Localization.get("EditJavascriptLibrary_PreferredLocation.Label")}
                            tooltipMessage={Localization.get("EditJavascriptLibrary_PreferredLocation.HelpText")}
                            style={inputStyle}
                            enabled={false} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditJavascriptLibrary_LibraryVersion.Label")}
                            tooltipMessage={Localization.get("EditJavascriptLibrary_LibraryVersion.HelpText")}
                            style={inputStyle}
                            enabled={false} />
                        <SingleLineInputWithError
                            label={Localization.get("EditJavascriptLibrary_ObjectName.Label")}
                            tooltipMessage={Localization.get("EditJavascriptLibrary_ObjectName.Label")}
                            style={inputStyle}
                            enabled={false} />
                        <SingleLineInputWithError
                            label={Localization.get("EditJavascriptLibrary_DefaultCDN.Label")}
                            tooltipMessage={Localization.get("EditJavascriptLibrary_DefaultCDN.HelpText")}
                            style={inputStyle}
                            enabled={false} />
                        <SingleLineInputWithError
                            label={Localization.get("EditJavascriptLibrary_CustomCDN.Label")}
                            tooltipMessage={Localization.get("EditJavascriptLibrary_CustomCDN.HelpText")}
                            style={inputStyle} />
                    </div>
                </GridSystem>
                <GridCell><hr /></GridCell>

                <GridSystem className="with-right-border top-half">
                    <div>
                        <GridCell>Depends On</GridCell>
                    </div>
                    <div>
                        <GridCell>Used By</GridCell>
                    </div>
                </GridSystem>
                {!props.actionButtonsDisabled &&
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary" onClick={props.onCancel.bind(this)}>Cancel</Button>
                    <Button type="primary">{props.primaryButtonText}</Button>
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

export default JavascriptLibrary;