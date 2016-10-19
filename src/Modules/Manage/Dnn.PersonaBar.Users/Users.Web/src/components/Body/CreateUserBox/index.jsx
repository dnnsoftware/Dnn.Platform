import React, {PropTypes, Component} from "react";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Localization from "localization";
import Button from "dnn-button";
import Switch from "dnn-switch";
import CheckBox from "dnn-checkbox";
import styles from "./style.less";

const inputStyle = { width: "100%" };

class CreateUserBox extends Component {
    constructor(props) {
        super(props);
        this.state = {};
    }
    render() {
        const {props} = this;
        return (
            <GridCell className={styles.newExtensionModal} style={props.style}>
                <GridCell className="new-user-box">
                    <GridSystem className="with-right-border top-half">
                        <div>
                            <SingleLineInputWithError label={Localization.get("label_FirstName") } tooltipMessage="Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet"
                                style={inputStyle} inputStyle={{ marginBottom: 25 }}/>
                            <SingleLineInputWithError label={Localization.get("label_UserName") } tooltipMessage="hey" style={inputStyle} inputStyle={{ marginBottom: 25 }}/>
                            <Switch value={true} label={Localization.get("label_Authorize") + ":"}/>
                        </div>
                        <div>
                            <SingleLineInputWithError label={Localization.get("label_LastName") } tooltipMessage="hey" style={inputStyle} inputStyle={{ marginBottom: 25 }}/>
                            <SingleLineInputWithError label={Localization.get("label_EmailAddress") } tooltipMessage="hey" style={inputStyle} inputStyle={{ marginBottom: 25 }}/>
                            <Switch value={true} label={Localization.get("label_RandomPassword") + ":" }/>
                        </div>
                    </GridSystem>
                    <GridCell><hr/></GridCell>
                    <GridSystem>
                        <div>
                            <SingleLineInputWithError label={Localization.get("label_Password") } tooltipMessage="Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet" style={inputStyle} inputStyle={{ marginBottom: 15 }}/>
                        </div>
                        <div>
                            <SingleLineInputWithError label={Localization.get("label_ConfirmPassword") } tooltipMessage="Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet Lorem ipsum dolor sit amet" style={inputStyle} inputStyle={{ marginBottom: 15 }}/></div>
                    </GridSystem>
                    <GridCell columnSize={100} className="email-notification-line">
                        <CheckBox value={true} label="Send an Email Notification to New User"/>
                    </GridCell>
                    <GridCell columnSize={100} className="modal-footer">
                        <Button id="cancelbtn"  type="secondary" onClick={props.onCancel.bind(this) }>{Localization.get("btn_Cancel") }</Button>
                        <Button id="confirmbtn" type="primary">{Localization.get("btn_Save") }</Button>
                    </GridCell>
                </GridCell>
            </GridCell>
        );
    }
}

CreateUserBox.propTypes = {
    onCancel: PropTypes.func.isRequired,
    style: PropTypes.object
};

export default (CreateUserBox);