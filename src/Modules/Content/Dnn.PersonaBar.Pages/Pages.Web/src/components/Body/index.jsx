import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import Tabs from "dnn-tabs";
import {
    pagination as PaginationActions
} from "../../actions";
import Button from "dnn-button";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import Checkbox from "dnn-checkbox";
import EditableField from "dnn-editable-field";
import RadioButtons from "dnn-radio-buttons";
import CollapsibleRow from "dnn-collapsible-row";
import SocialPanelBody from "dnn-social-panel-body";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import TextOverflowWrapper from "dnn-text-overflow-wrapper";
import Dropdown from "dnn-dropdown";
import "./style.less";

const radioButtonOptions = [
    {
        label: "Button 1",
        value: 0
    },
    {
        label: "Button 2",
        value: 1
    }
];

class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);
        this.state = {
            selectedRadioButton: 0,
            value: "Test Me!",
            textAreaValue: "Multi line!",
            singleLineValue: "Single line!",
            selectValue: 1
        };
    }
    componentWillMount() {
        // const {props} = this;
        //props.dispatch(); //Dispatch action to get data here
    }

    handleSelect(index/*, last*/) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    onRadioButtonChange(value) {
        alert(value);
    }

    onEnter(key) {
        const { state } = this;
        alert("You pressed enter! My value is: " + state[key]);
    }

    onChange(key, event) {
        this.setState({
            [key]: event.target.value
        });
    }

    onSelectChange(option) {
        this.setState({
            selectValue: option.value
        });
    }

    render() {
        const {props, state} = this;

        return (
            <SocialPanelBody>
                <Tabs onSelect={this.handleSelect}
                    selectedIndex={props.tabIndex}
                    tabHeaders={["Pane 1", "Pane 2"]}>
                    <Tabs
                        tabHeaders={["Pane 3", "Pane 4"]}
                        type="secondary">
                        <CollapsibleRow label="Collapsible Row!" extraFooterButtons={
                            [<Button type="secondary">Disable</Button>, <Button type="primary">Save</Button>]
                        }>
                            <GridSystem>
                                <div>
                                    <EditableField
                                        label="Editable Single Line"
                                        onEnter={this.onEnter.bind(this, "value") }
                                        value={state.value}
                                        editable={true}
                                        />
                                    <EditableField
                                        label="Editable Multi Line"
                                        onEnter={this.onEnter.bind(this, "textAreaValue") }
                                        value={state.textAreaValue}
                                        editable={true}
                                        inputType="textArea"
                                        />
                                    <EditableField
                                        label="Read only"
                                        onEnter={this.onEnter.bind(this, "textAreaValue") }
                                        value={"My value"}
                                        editable={false}
                                        />
                                </div>
                                <div>
                                    <RadioButtons
                                        onChange={this.onRadioButtonChange.bind(this) }
                                        options={radioButtonOptions}
                                        label="Radio:"
                                        buttonGroup="sampleRadio"
                                        defaultValue={state.selectedRadioButton}/>
                                    <Checkbox value={true} label={"Checkbox 1"}/>
                                    <Checkbox value={false} label={"checkbox 2"}/>
                                    <GridCell columnSize={50}>
                                        <br/><br/>
                                        <p>I am 50% width<br/> (DNN-GRID-CELL) </p>
                                        <img src="http://placehold.it/500x250" className="img-responsive"/>
                                    </GridCell>
                                    <GridCell columnSize={100}>
                                        <br/><br/>
                                        <TextOverflowWrapper width={100} text="I am a very very long text, I should not be visible." />
                                    </GridCell>
                                </div>
                            </GridSystem>
                        </CollapsibleRow>
                        <p>hey2</p>
                    </Tabs>
                    <div></div>
                </Tabs>
                <GridSystem style={{ padding: 15, marginTop: 50 }}>
                    <div style={{ padding: 15 }}>
                        <SingleLineInputWithError
                            error={true}
                            errorMessage={"Error me!"}
                            label={"Single line input with error with tooltip"}
                            tooltipMessage="I have a tool tip"
                            onChange={this.onChange.bind(this, "singleLineValue") }
                            value={state.singleLineValue}/>
                        <p>{state.singleLineValue}</p>
                    </div>
                    <div style={{ padding: 15 }}>
                        <MultiLineInputWithError
                            error={false}
                            errorMessage={"Error me!"}
                            label="Multi line with no error"
                            onChange={this.onChange.bind(this, "textAreaValue") }
                            value={state.textAreaValue}/>
                        <p>{state.textAreaValue}</p>
                    </div>
                </GridSystem>
                <GridCell columnSize={50}>
                    <Dropdown options={radioButtonOptions} onSelect={this.onSelectChange.bind(this) } value={state.selectValue}/>
                </GridCell>
            </SocialPanelBody >
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(Body);