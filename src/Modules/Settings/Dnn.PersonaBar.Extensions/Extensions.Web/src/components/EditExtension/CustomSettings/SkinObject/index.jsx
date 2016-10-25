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
class SkinObject extends Component {
    render() {
        const {props, state} = this;

        return (
            <GridCell className={styles.editSkinObject}>
                <GridSystem className="with-right-border top-half">
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditSkinObject_ControlKey.Label")}
                            tooltipMessage={Localization.get("EditSkinObject_ControlKey.HelpText")}
                            style={inputStyle} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditSkinObject_ControlSrc.Label")}
                            tooltipMessage={Localization.get("EditSkinObject_ControlSrc.HelpText")}
                            style={inputStyle} />
                        <Switch value={true}
                            label={Localization.get("EditSkinObject_SupportsPartialRender.Label")}
                            tooltipMessage={Localization.get("EditSkinObject_SupportsPartialRender.HelpText")} />
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

SkinObject.PropTypes = {
    onCancel: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    onChange: PropTypes.func,
    disabled: PropTypes.func,
    primaryButtonText: PropTypes.string
};

export default SkinObject;