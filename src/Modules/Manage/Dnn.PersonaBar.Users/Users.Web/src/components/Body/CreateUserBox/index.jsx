import React, {PropTypes, Component} from "react";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import SocialPanelBody from "dnn-social-panel-body";
import Localization from "localization";
import Button from "dnn-button";
import Switch from "dnn-switch";
import styles from "./style.less";

const inputStyle = { width: "100%" };

class NewExtensionModal extends Component {
    constructor() {
        super();
        this.state = {};
    }

    render() {
        const {props} = this;
        return (
            <GridCell className={styles.newExtensionModal} style={props.style}>
                <GridCell className="new-extension-box">
                    <GridCell className="new-user-box">
                        <GridCell>
                            <h3 className="box-title">{Localization.get("label_CreateNewUser") }</h3>
                        </GridCell>
                        <GridSystem className="with-right-border top-half">
                            <div>
                                <SingleLineInputWithError label={Localization.get("label_FirstName") } tooltipMessage="Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet" style={inputStyle}/>
                                <SingleLineInputWithError label={Localization.get("label_UserName") } tooltipMessage="hey" style={inputStyle} inputStyle={{ marginBottom: 0 }}/>
                                <Switch value={true} label={Localization.get("label_Authorize") + ":"}/>
                            </div>
                            <div>
                                <SingleLineInputWithError label={Localization.get("label_LastName") } tooltipMessage="hey" style={inputStyle}/>
                                <SingleLineInputWithError label={Localization.get("label_EmailAddress") } tooltipMessage="hey" style={inputStyle} inputStyle={{ marginBottom: 0 }}/>
                                <Switch value={true} label={Localization.get("label_Notify") + ":"}/>
                            </div>
                        </GridSystem>
                        <GridCell><hr/></GridCell>
                        <GridCell columnSize={50}>
                            <Switch value={true} label={Localization.get("label_RandomPassword") + ":" }/>
                        </GridCell>
                        <GridSystem>
                            <div>
                                <SingleLineInputWithError label={Localization.get("label_Password") } tooltipMessage="Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet" style={inputStyle}/>
                            </div>
                            <div>
                                <SingleLineInputWithError label={Localization.get("label_ConfirmPassword") } tooltipMessage="Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet" style={inputStyle}/></div>
                        </GridSystem>
                        <GridCell columnSize={100} className="modal-footer">
                            <Button type="secondary">{Localization.get("btn_Cancel")}</Button>
                            <Button type="primary">{Localization.get("btn_Save")}</Button>
                        </GridCell>
                    </GridCell>
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

NewExtensionModal.PropTypes = {
    onCancel: PropTypes.func,
    style: PropTypes.object
};

export default NewExtensionModal;