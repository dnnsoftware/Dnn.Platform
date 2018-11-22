import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import PersonaBarPageBody from "dnn-persona-bar-page-body";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import InputGroup from "dnn-input-group";
import RadioButtons from "dnn-radio-buttons";
import {
    vocabulary as VocabularyActions,
    pagination as PaginationActions
} from "../../actions";
import LocalizedResources from "../../resources";
import util from "../../utils";
import styles from "./style.less";

class CreateVocabulary extends Component {
    constructor() {
        super();
        this.state = {
            term: {
                Name: "",
                Description: "",
                TypeId: 1,
                ScopeTypeId: 2
            },
            error: {
                name: true
            },
            triedToSubmit: false
        };
    }

    componentDidMount() {
        this.setState({
            term: {
                Name: "",
                Description: "",
                TypeId: 1,
                ScopeTypeId: 2
            }
        });
    }

    onCloseVocabulary() {
        const {props} = this;
        props.onCloseVocabulary();
    }


    onTermValueChange(key, event) {
        const value = typeof event === "object" ? event.target.value : parseInt(event); //event is value

        let {state} = this;
        let {term} = this.state;
        term[key] = value;
        state.triedToSubmit = false;
        if (value === "" && key === "Name") {
            state.error["name"] = true;
        } else if (value !== "" && key === "Name") {
            state.error["name"] = false;
        }
        this.setState({
            state,
            term
        });
    }

    getNextPage(pageIndex, pageSize, scopeTypeId) {
        return {
            pageIndex: pageIndex || 0,
            pageSize: pageSize,
            scopeTypeId: scopeTypeId || 0
        };
    }

    onAddNewVocabulary(event) {
        event.preventDefault();
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.name) {
            return;
        }

        let totalCount = props.totalCount;
        props.dispatch(VocabularyActions.addVocabulary(state.term, ++totalCount, () => {
            this.onCloseVocabulary();
            props.dispatch(PaginationActions.loadTab(this.getNextPage(0, 10000, state.term.ScopeTypeId)));   //index acts as scopeTypeId 
        }));
    }

    render() {
        const {props, state} = this;
        const typeOptions = [
            {
                label: LocalizedResources.get("Simple"),
                value: 1
            },
            {
                label: LocalizedResources.get("Hierarchy"),
                value: 2
            }
        ];

        const scopeOptions = [
            {
                label: LocalizedResources.get("Portal"),
                value: 2
            },
            {
                label: LocalizedResources.get("Application"),
                value: 1
            }
        ];
        return (
            //inline style for height to allow proper calculating
            <PersonaBarPageBody backToLinkProps={{
                text: LocalizedResources.get("BackToVocabularies"),
                onClick: this.onCloseVocabulary.bind(this)
            }}
            className={styles.createVocabulary} style={{ height: "calc(100% - 100px)" }}>
                {props.isOpen &&
                    <GridCell className="create-box">
                        <InputGroup>
                            <SingleLineInputWithError
                                inputId={"create-vocabulary-name"}
                                withLabel={true}
                                label={LocalizedResources.get("TermName") + "*"}
                                error={state.error.name && state.triedToSubmit}
                                errorMessage={LocalizedResources.get("TermValidationError.Message")}
                                value={state.term.Name}
                                onChange={this.onTermValueChange.bind(this, "Name")}
                            />
                        </InputGroup>
                        <InputGroup>
                            <MultiLineInputWithError
                                inputId={"create-vocabulary-description"}
                                withLabel={true}
                                label={LocalizedResources.get("Description")}
                                value={state.term.Description}
                                onChange={this.onTermValueChange.bind(this, "Description")} />
                        </InputGroup>
                        <RadioButtons
                            onChange={this.onTermValueChange.bind(this, "TypeId")}
                            options={typeOptions}
                            label={LocalizedResources.get("Type.Header") + ":"}
                            buttonGroup="vocabularyType"
                            buttonWidth={130}
                            value={state.term.TypeId} />
                        {util.isHost() && <RadioButtons
                            onChange={this.onTermValueChange.bind(this, "ScopeTypeId")}
                            options={scopeOptions}
                            label={LocalizedResources.get("Scope.Header") + ":"}
                            buttonGroup="scopeType"
                            buttonWidth={130}
                            value={state.term.ScopeTypeId} />}
                        <GridCell className="action-buttons">
                            <Button type="secondary" onClick={this.onCloseVocabulary.bind(this)}>{LocalizedResources.get("cancelCreate")}</Button>
                            <Button type="primary" onClick={this.onAddNewVocabulary.bind(this)}>{LocalizedResources.get("CreateVocabulary")}</Button>
                            <span className="required-help-text">* {LocalizedResources.get("RequiredField")}</span>
                        </GridCell>
                    </GridCell>
                }
            </PersonaBarPageBody>
        );
    }
}

CreateVocabulary.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    totalCount: PropTypes.number,
    onCloseVocabulary: PropTypes.func,
    isOpen: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        totalCount: state.vocabulary.totalCount,
        vocabularyAddedIsValid: state.vocabulary.vocabularyAddedIsValid,
        pagination: state.pagination
    };
}

export default connect(mapStateToProps)(CreateVocabulary);