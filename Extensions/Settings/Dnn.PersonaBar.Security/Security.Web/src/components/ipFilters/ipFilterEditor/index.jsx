import React, {Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import { SingleLineInputWithError, Grid, Label, InputGroup, Button, RadioButtons, Dropdown } from "@dnnsoftware/dnn-react-common";
import { security as SecurityActions } from "../../../actions";
import resx from "../../../resources";

let specificityOptions = [];
let typeOptions = [];
const re = /^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;

class IpFilterEditor extends Component {
    constructor() {
        super();
        this.state = {
            ipFilter: {},
            error: {
                ip: true,
                mask: true
            },
            triedToSubmit: false,
            isIpRange: false,
            formModified: false
        };
    }

    UNSAFE_componentWillMount() {
        const {props} = this;
        if (props.ipFilterId) {
            props.dispatch(SecurityActions.getIpFilter({
                filterId: props.ipFilterId
            }));
        }

        specificityOptions = [];
        specificityOptions.push({ label: resx.get("SingleIP"), value: 1 });
        specificityOptions.push({ label: resx.get("IPRange"), value: 2 });

        typeOptions = [];
        typeOptions.push({ label: resx.get("AllowIP"), value: 1 });
        typeOptions.push({ label: resx.get("DenyIP"), value: 2 });
    }

    UNSAFE_componentWillReceiveProps(props) {
        let {state} = this;

        let ip = props.ipFilter["IPAddress"];
        let mask = props.ipFilter["SubnetMask"];
        if (ip === "" || !re.test(ip)) {
            state.error["ip"] = true;
        }
        else if (ip !== "" && re.test(ip)) {
            state.error["ip"] = false;
        }
        if (mask === "" || !re.test(mask)) {
            state.error["mask"] = true;
        }
        else if (mask !== "" && re.test(mask)) {
            state.error["mask"] = false;
        }
        this.setState({
            ipFilter: Object.assign({}, props.ipFilter),
            triedToSubmit: false,
            error: state.error,
            isIpRange: props.ipFilter.SubnetMask.length > 0 ? true : false
        });
    }

    getValue(selectKey) {
        const {state} = this;
        switch (selectKey) {
            case "IPAddress":
                return state.ipFilter.IPAddress !== undefined ? state.ipFilter.IPAddress.toString() : "";
            case "RuleType":
                return state.ipFilter.RuleType !== undefined ? state.ipFilter.RuleType.toString() : "";
            case "SubnetMask":
                return state.ipFilter.SubnetMask !== undefined ? state.ipFilter.SubnetMask.toString() : "";
            default:
                break;
        }
    }

    toggle(event) {
        if (this.state.isIpRange && parseInt(event) === 2) {
            return;
        }
        else if (!this.state.isIpRange && parseInt(event) === 1) {
            return;
        }
        let {ipFilter} = this.state;
        if (this.state.isIpRange) {
            ipFilter["SubnetMask"] = "";
            this.setState({
                isIpRange: !this.state.isIpRange,
                ipFilter: ipFilter,
                formModified: true
            });
        }
        else {
            this.setState({
                isIpRange: !this.state.isIpRange,
                formModified: true
            });
        }
    }

    onSettingChange(key, event) {
        let {state} = this;
        let {ipFilter} = this.state;

        if (key === "RuleType") {
            ipFilter[key] = parseInt(event.value);
        }
        else {
            ipFilter[key] = typeof (event) === "object" ? event.target.value : event;
        }
        if (!re.test(ipFilter[key]) && key === "IPAddress") {
            state.error["ip"] = true;
        }
        else if (re.test(ipFilter[key]) && key === "IPAddress") {
            state.error["ip"] = false;
        }
        if (!re.test(ipFilter[key]) && key === "SubnetMask") {
            state.error["mask"] = true;
        }
        else if (re.test(ipFilter[key]) && key === "SubnetMask") {
            state.error["mask"] = false;
        }

        this.setState({
            ipFilter: ipFilter,
            triedToSubmit: false,
            error: state.error,
            formModified: true
        });
    }

    onUpdateItem(event) {
        event.preventDefault();
        const {state} = this;
        let {ipFilter} = this.state;

        /*eslint-disable eqeqeq*/
        if (this.state.ipFilter.RuleType == undefined) {
            ipFilter["RuleType"] = 1;
            this.setState({
                ipFilter: ipFilter,
                triedToSubmit: true
            });
        }
        else {
            this.setState({
                triedToSubmit: true
            });
        }
        if (state.isIpRange) {
            if (state.error.ip || state.error.mask) {
                return;
            }
        }
        else {
            if (state.error.ip) {
                return;
            }
        }

        this.props.onUpdate(this.state.ipFilter);
    }

    /* eslint-disable react/no-danger */
    render() {
        const columnOne = <div className="container">
            <InputGroup>
                <Label
                    labelType="inline"
                    tooltipMessage={resx.get("plRuleSpecifity.Help") }
                    label={resx.get("plRuleSpecifity") } />
                {this.state.isIpRange &&
                    <RadioButtons
                        onChange={this.toggle.bind(this) }
                        options={specificityOptions}
                        buttonGroup="specificity"
                        value={2}/>
                }
                {!this.state.isIpRange &&
                    <RadioButtons
                        onChange={this.toggle.bind(this) }
                        options={specificityOptions}
                        buttonGroup="specificity"
                        value={1}/>
                }
            </InputGroup>
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("plRuleType.Help") }
                    label={resx.get("plRuleType") } />
                <Dropdown
                    options={typeOptions }
                    value={this.state.ipFilter.RuleType != undefined ? this.state.ipFilter.RuleType : 1}
                    onSelect={this.onSettingChange.bind(this, "RuleType") } />
            </InputGroup>
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("plFirstIP.Help") }
                    label={resx.get("plFirstIP") } />
                <SingleLineInputWithError
                    inputStyle={{ margin: "0" }}
                    withLabel={false}
                    error={this.state.error.ip && this.state.triedToSubmit}
                    errorMessage={resx.get("IPValidation.ErrorMessage") }
                    value={this.state.ipFilter.IPAddress}
                    onChange={this.onSettingChange.bind(this, "IPAddress") } />
            </InputGroup>
            {this.state.isIpRange &&
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plSubnet.Help") }
                        label={resx.get("plSubnet") } />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={this.state.error.mask && this.state.triedToSubmit}
                        errorMessage={resx.get("IPValidation.ErrorMessage") }
                        value={this.state.ipFilter.SubnetMask}
                        onChange={this.onSettingChange.bind(this, "SubnetMask") } />
                </InputGroup>
            }
        </div>;

        let children = [];
        children.push(columnOne);
        /* eslint-disable react/no-danger */
        return (
            <div className="ip-filter-setting-editor">
                <Grid
                    numberOfColumns={1}>
                    {children}
                </Grid>
                <div className="buttons-box">
                    <Button                        
                        type="secondary"
                        onClick={this.props.Collapse.bind(this) }>
                        {resx.get("Cancel") }
                    </Button>
                    <Button
                        disabled={!this.state.formModified}
                        type="primary"
                        onClick={this.onUpdateItem.bind(this) }>
                        {resx.get("Save") }
                    </Button>
                </div>
            </div>
        );
    }
}

IpFilterEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    ipFilter: PropTypes.object,
    ipFilterId: PropTypes.number,
    Collapse: PropTypes.func,
    onUpdate: PropTypes.func
};

function mapStateToProps(state) {
    return {
        ipFilter: state.security.ipFilter
    };
}

export default connect(mapStateToProps)(IpFilterEditor);
