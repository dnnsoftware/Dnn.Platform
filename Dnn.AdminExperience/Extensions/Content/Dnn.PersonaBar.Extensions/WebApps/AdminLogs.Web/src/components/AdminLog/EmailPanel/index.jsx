import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import { Collapsible as Collapse, MultiLineInput, Button, Label, InputGroup, SingleLineInputWithError } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import {
    log as LogActions
} from "../../../actions";
import Localization from "localization";
import util from "../../../utils";

class EmailPanel extends Component {
    constructor() {
        super();
        this.state = {
            emailRequest: {
                Email: "",
                Subject: Localization.get("LogEntryDefaultSubject"),
                Message: Localization.get("LogEntryDefaultMsg"),
                LogIds: []
            },
            error: {
                email: false
            }
        };
    }

    componentWillMount() {
        this.SetErrorState();
    }

    SetErrorState() {
        let {state} = this;
        let {emailRequest} = this.state;
        if (!this.validateEmail(emailRequest.Email)) {
            state.error["email"] = true;
        }
        else {
            state.error["email"] = false;
        }
        state.triedToSubmit = false;
        this.setState({
            state
        });
    }

    onSendEmail(event) {
        event.preventDefault();
        const {props, state} = this;
        const { emailRequest } = this.state;
        emailRequest["LogIds"] = props.logIds;
        this.setState({
            emailRequest,
            triedToSubmit: true
        });
        if (state.error.email) {
            return;
        }
        if (props.logIds.length <= 0) {
            util.utilities.notifyError(Localization.get("SelectException"));
            return;
        }
        props.dispatch(LogActions.emailLogItems(state.emailRequest, (data) => {
            util.utilities.notify(data.ErrorMessage + data.ReturnMessage);
            if (data.Success) {
                this.onCloseEmailPanel();
            }
        }));
    }

    validateEmail(value) {
        const re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return value.split(/[ ,;]+/).every(v => re.test(v) || v.trim().length === 0);
    }

    onEmailValueChange(key, event) {
        const value = event.target.value;
        const { emailRequest } = this.state;
        emailRequest[key] = value;
        this.setState({
            emailRequest
        });
        this.SetErrorState();
    }

    onCloseEmailPanel() {
        const {props} = this;
        props.onCloseEmailPanel();
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <div className="collapsible-content-email">
                <Collapse
                    fixedHeight={props.fixedHeight}
                    keepCollapsedContent={props.keepCollapsedContent}
                    isOpened={props.isOpened}>
                    {props.fixedHeight &&
                        <div>
                            <div className="emailpanel-content-wrapper" style={{ height: "100%" }}>
                                <div className="">
                                    <InputGroup>
                                        <Label
                                            labelType="inline"
                                            tooltipMessage={Localization.get("plEmailAddress.Help")}
                                            label={Localization.get("plEmailAddress")}
                                            />
                                        <SingleLineInputWithError
                                            error={state.error.email && state.triedToSubmit}
                                            inputStyle={{ marginBottom: "15px" }}
                                            errorMessage={Localization.get("Email.Message")}
                                            value={state.emailRequest.Email}
                                            onChange={this.onEmailValueChange.bind(this, "Email")} />
                                    </InputGroup>
                                    <InputGroup>
                                        <Label
                                            labelType="inline"
                                            tooltipMessage={Localization.get("plSubject.Help")}
                                            label={Localization.get("plSubject")}
                                            />
                                        <SingleLineInputWithError
                                            error={false}
                                            inputStyle={{ marginBottom: "15px" }}
                                            value={state.emailRequest.Subject}
                                            onChange={this.onEmailValueChange.bind(this, "Subject")}
                                            />
                                    </InputGroup>
                                    <InputGroup>
                                        <Label
                                            labelType="inline"
                                            tooltipMessage={Localization.get("SendMessage.Help")}
                                            label={Localization.get("SendMessage")}
                                            />
                                        <MultiLineInput
                                            value={state.emailRequest.Message}
                                            onChange={this.onEmailValueChange.bind(this, "Message")}
                                            />
                                    </InputGroup>
                                    <div className="action-buttons">
                                        <Button type="secondary" onClick={this.onCloseEmailPanel.bind(this)}>{Localization.get("btnCancel")}</Button>
                                        <Button type="primary" onClick={this.onSendEmail.bind(this)}>{Localization.get("btnSend")}</Button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    }
                    {!props.fixedHeight && props.children}
                </Collapse>
            </div>
        );
    }
}

EmailPanel.PropTypes = {
    label: PropTypes.string,
    fixedHeight: PropTypes.number,
    collapsibleWidth: PropTypes.number,
    collapsibleHeight: PropTypes.number,
    keepCollapsedContent: PropTypes.bool,
    scrollAreaStyle: PropTypes.object,
    children: PropTypes.node,
    isOpened: PropTypes.bool,
    logIds: PropTypes.array,
    onCloseEmailPanel: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        logList: state.log.logList
    };
}

export default connect(mapStateToProps)(EmailPanel);