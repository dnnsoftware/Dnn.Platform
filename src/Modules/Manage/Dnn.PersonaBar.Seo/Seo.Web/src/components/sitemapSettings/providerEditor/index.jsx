import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import "./style.less";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Grid from "dnn-grid-system";
import Label from "dnn-label";
import InputGroup from "dnn-input-group";
import Button from "dnn-button";
import Switch from "dnn-switch";
import Dropdown from "dnn-dropdown";
import util from "../../../utils";
import resx from "../../../resources";
import styles from "./style.less";

let priorityOptions = [];

class ProviderEditor extends Component {
    constructor() {
        super();
        this.state = {
            settings: {}
        };
    }

    componentWillMount() {
        const {props} = this;

        priorityOptions = [];
        priorityOptions.push({ "value": -1, "label": resx.get("None") });
        priorityOptions.push({ "value": 1, "label": "1" });
        priorityOptions.push({ "value": 0.9, "label": "0.9" });
        priorityOptions.push({ "value": 0.8, "label": "0.8" });
        priorityOptions.push({ "value": 0.7, "label": "0.7" });
        priorityOptions.push({ "value": 0.6, "label": "0.6" });
        priorityOptions.push({ "value": 0.5, "label": "0.5" });
        priorityOptions.push({ "value": 0.4, "label": "0.4" });
        priorityOptions.push({ "value": 0.3, "label": "0.3" });
        priorityOptions.push({ "value": 0.2, "label": "0.2" });
        priorityOptions.push({ "value": 0.1, "label": "0.1" });
        priorityOptions.push({ "value": 0, "label": "0" });

        this.setState({
            settings: Object.assign({}, props.settings)
        });
    }

    onSettingChange(key, event) {
        let {state} = this;
        let {settings} = this.state;

        if (key === "Priority") {
            settings[key] = parseFloat(event.value);
        }
        else {
            settings[key] = typeof (event) === "object" ? event.target.value : event;
        }

        this.setState({
            settings: settings
        });
    }

    onUpdateItem(event) {
        event.preventDefault();
        const {props, state} = this;
        this.props.onUpdate(state.settings);
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        if (state.settings) {            
            const columnOne = <div className="left-column">
                <InputGroup>
                    <div className="providerSettings-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("enableSitemapProvider.Help")}
                            label={resx.get("enableSitemapProvider")}
                            />
                        <Switch
                            labelHidden={true}
                            value={state.settings.Enabled}
                            onChange={this.onSettingChange.bind(this, "Enabled")}
                            />
                    </div>
                </InputGroup>
            </div>;

            const columnTwo = <div className="right-column">
                <InputGroup>
                    <div className="providerSettings-row_dropdown">
                        <Label
                            tooltipMessage={resx.get("overridePriority.Help")}
                            label={resx.get("overridePriority")}
                            />
                        <Dropdown
                            options={priorityOptions}
                            value={!state.settings.OverridePriority ? -1: state.settings.Priority}
                            onSelect={this.onSettingChange.bind(this, "Priority")}
                            />
                    </div>
                </InputGroup>
            </div>;

            /* eslint-disable react/no-danger */
            return (
                <div className={styles.providerSettingEditor}>
                    <Grid children={[columnOne, columnTwo]} numberOfColumns={2} />
                    <div className="buttons-box-secondary">
                        <Button
                            type="secondary"
                            onClick={this.props.Collapse.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            type="primary"
                            onClick={this.onUpdateItem.bind(this)}>
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

ProviderEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    settings: PropTypes.object,
    Collapse: PropTypes.func,
    onUpdate: PropTypes.func
};

export default (ProviderEditor);