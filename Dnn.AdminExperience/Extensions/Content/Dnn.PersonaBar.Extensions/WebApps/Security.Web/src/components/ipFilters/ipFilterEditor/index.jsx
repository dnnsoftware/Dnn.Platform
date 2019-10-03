import React, {Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import { SingleLineInputWithError, GridSystem, Label, InputGroup, Button, RadioButtons, Dropdown } from "@dnnsoftware/dnn-react-common";
import { security as SecurityActions } from "../../../actions";
import resx from "../../../resources";

let specificityOptions = [];
let typeOptions = [];
const re = /^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;
const SINGLE_IP = "SingleIP";
const IP_RANGE = "IPRange";

class IpFilterEditor extends Component {
    constructor() {
        super();
        this.state = {
            ipFilter: {RuleType: 1},
            error: {
                ip: true,
                mask: true
            },
            triedToSubmit: false,
            formModified: false,
            ruleSpecificity: SINGLE_IP
        };
        
        specificityOptions = [];
        specificityOptions.push({ label: resx.get(SINGLE_IP), value: SINGLE_IP });
        specificityOptions.push({ label: resx.get(IP_RANGE), value: IP_RANGE });

        typeOptions = [];
        typeOptions.push({ label: resx.get("AllowIP"), value: 1 });
        typeOptions.push({ label: resx.get("DenyIP"), value: 2 });
    }

    componentDidMount() {
        const {props} = this;
        if (props.ipFilterId) {
            props.dispatch(SecurityActions.getIpFilter({
                    filterId: props.ipFilterId
                }, (data) => {
                    let ipFilter = Object.assign({}, data.Results);
                    this.setState({
                        error: {
                            ip: false,
                            mask: false
                        },
                        ruleSpecificity: ipFilter.SubnetMask === "" ? SINGLE_IP : IP_RANGE,
                        ipFilter
                    });
                }));
        }
    }

    onRuleSpecificityChange(event) {
        this.setState({
            ruleSpecificity: event,
            formModified: true,
            triedToSubmit: false
        });
    }

    onSettingChange(key, event) {
        let {ipFilter} = this.state;

        if (key === "RuleType") {
            ipFilter[key] = parseInt(event.value);
        }
        else {
            ipFilter[key] = typeof (event) === "object" ? event.target.value : event;
        }
        
        this.setState({
            ipFilter: ipFilter,
            triedToSubmit: false,
            formModified: true
        });
    }

    onUpdateItem(event) {
        event.preventDefault();
        const {state} = this;
        
        this.setState({
            triedToSubmit: true
        });
        
        if(this.validateIPAddressContainsError()) {
            return;
        }
        
        if (state.ruleSpecificity === SINGLE_IP) {
            state.ipFilter["SubnetMask"] = "";
        }

        this.props.onUpdate(this.state.ipFilter);
    }
    
    validateIPAddressContainsError() {
        const {state} = this;
        
        state.error["ip"] = !this.isValidIpAddress(state.ipFilter["IPAddress"]);
        
        if (state.ruleSpecificity === IP_RANGE) {
            state.error["mask"] = !this.isValidIpAddress(state.ipFilter["SubnetMask"]);
        } else {
            state.error["mask"] = false;
        }
        
        this.setState({
            error: state.error
        });
        
        return (state.error.ip || state.error.mask);
    }
     
    isValidIpAddress(ipAddress) {
        return re.test(ipAddress);
    }

    /* eslint-disable react/no-danger */
    render() {
        const columnOne = <div className="container">
            <InputGroup>
                <Label
                    labelType="inline"
                    tooltipMessage={resx.get("plRuleSpecifity.Help") }
                    label={resx.get("plRuleSpecifity") } />
                    <RadioButtons
                        onChange={this.onRuleSpecificityChange.bind(this) }
                        options={specificityOptions}
                        buttonGroup="specificity"
                        value={this.state.ruleSpecificity}/>
            </InputGroup>
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("plRuleType.Help") }
                    label={resx.get("plRuleType") } />
                <Dropdown
                    options={typeOptions }
                    value={this.state.ipFilter.RuleType}
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
            {this.state.ruleSpecificity === IP_RANGE &&
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
                <GridSystem
                    numberOfColumns={1}>
                    {children}
                </GridSystem>
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
