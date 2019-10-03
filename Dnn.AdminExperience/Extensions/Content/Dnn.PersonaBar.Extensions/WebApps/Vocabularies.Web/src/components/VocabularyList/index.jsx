import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Button, GridSystem, PersonaBarPageBody, DnnTabs as Tabs } from "@dnnsoftware/dnn-react-common";
import {
    vocabulary as VocabularyActions,
    pagination as PaginationActions,
    vocabularyTermList as VocabularyTermListActions
} from "../../actions";
import TermHeader from "./TermHeader";
import LeftPane from "./LeftPane";
import RightPane from "./RightPane";
import util from "../../utils";
import LocalizedResources from "../../resources";
import "./style.less";

const loadMoreButtonStyle = {
    display: "block",
    margin: "20px auto"
};

function getTermUpdates(term) {
    return {
        TermId: term.TermId,
        Name: term.Name,
        Description: term.Description,
        VocabularyId: term.VocabularyId,
        ParentTermId: term.ParentTermId
    };
}
function getRootParentTerm(termId, term, termList) {
    while (term.TermId !== termId && term.ParentTermId > -1) {
        term = termList.find(_term => {
            return _term.TermId === term.ParentTermId;
        });
    }
    return term;
}

function removeTermFromList(termId, vocabularyTermList) {
    return vocabularyTermList.filter((term) => {
        const rootParentTerm = getRootParentTerm(termId, term, vocabularyTermList);
        return rootParentTerm.TermId !== termId;
    });
}

class VocabularyListComponent extends Component {
    constructor() {
        super();
        this.onDescriptionUpdate = this.onDescriptionUpdate.bind(this);
        this.state = {
            vocabularyList: [],
            isOpened: false
        };
    }
    getNextPage(pageIndex, pageSize, scopeTypeId) {
        return {
            pageIndex: pageIndex || 0,
            pageSize: pageSize,
            scopeTypeId: scopeTypeId || 0
        };
    }

    componentDidMount() {
        const {props} = this;
        props.dispatch(VocabularyActions.getVocabularyList(this.getNextPage(props.pagination.pageIndex, props.pagination.pageSize, props.pagination.scopeTypeId)));
    }

    getVocabularyTerms(vocabularyId, index) {
        const {props} = this;
        props.dispatch(VocabularyTermListActions.getVocabularyTerms(vocabularyId, index));
    }

    updateVocabulary(vocabularyList, key, index, value) {
        let newValue = Object.assign({}, vocabularyList[index]);
        newValue[key] = value;
        return newValue;
    }

    onUpdateTerm(termBeingEdited, editMode, callback) {
        const {props} = this;

        if (editMode) {
            const index = props.vocabularyTerms.findIndex((term) => {
                return term.TermId === termBeingEdited.TermId;
            });
            props.dispatch(VocabularyTermListActions.updateVocabularyTerm(getTermUpdates(termBeingEdited), index, callback));
        } else {
            let {totalTermCount} = props;
            props.dispatch(VocabularyTermListActions.addVocabularyTerm(getTermUpdates(termBeingEdited), ++totalTermCount, callback));
        }
    }

    onDescriptionUpdate(key, value, index) {
        const {props} = this;
        let vocabularyList = props.vocabularyList;
        const newValue = this.updateVocabulary(vocabularyList, key, index, value);
        props.dispatch(VocabularyActions.updateVocabulary(newValue, index));
    }
    onLoadMore(event) {
        event.preventDefault();

        const {props} = this;
        let {pageIndex} = props.pagination; //copy page index
        let nextPage = this.getNextPage(++pageIndex, props.pagination.pageSize, props.pagination.scopeTypeId);
        props.dispatch(PaginationActions.loadMore(nextPage));
    }
    handleSelect(index /*,last*/) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(this.getNextPage(0, props.pagination.pageSize, index)));   //index acts as scopeTypeId
    }
    onDeleteVocabulary(vocabulary, index, callback) {
        const {props} = this;

        util.utilities.confirm(LocalizedResources.get("ConfirmDeletion_Vocabulary").replace("{0}", vocabulary.Name), //confirm delete message
            LocalizedResources.get("DeleteVocabulary"), //confirm delete text
            LocalizedResources.get("cancelCreate"),  //cancel text
            () => {
                let totalCount = props.totalCount;
                if (callback) {
                    callback();
                }
                props.dispatch(VocabularyActions.deleteVocabulary(vocabulary.VocabularyId, index, --totalCount, () => {
                    if (props.vocabularyList.length < props.totalCount) {
                        let {pageIndex} = props.pagination; //copy page index
                        let currentPage = this.getNextPage(pageIndex, props.pagination.pageSize, props.pagination.scopeTypeId);
                        props.dispatch(PaginationActions.loadMore(currentPage));
                    }
                }));
            });
    }

    onDeleteTerm(termId) {
        const {props} = this;
        const vocabularyTerms = removeTermFromList(termId, props.vocabularyTerms);
        props.dispatch(VocabularyTermListActions.deleteVocabularyTerm(termId, vocabularyTerms, vocabularyTerms.length));
    }

    selectParentTerm(term) {
        const {props} = this;

        const vocabularyTerms = JSON.parse(JSON.stringify(props.vocabularyTerms));

        const previousTerms = vocabularyTerms.map((_term) => {
            delete _term.selected;
            return _term;
        });

        const index = props.vocabularyTerms.findIndex((_term) => {
            return _term.TermId === term.TermId;
        });
        term.selected = true;
        props.dispatch(VocabularyTermListActions.clearSelected(previousTerms));
        props.dispatch(VocabularyTermListActions.setTermSelected(term, index));
    }

    render() {
        const {vocabularyList} = this.props;
        const {props} = this;

        const renderedVocabularyList = vocabularyList.map((term, index) => {
            return (
                <TermHeader
                    header={term.Name}
                    type={term.TypeId}
                    term={term}
                    index={index}
                    key={"vocabularyTerm-" + index}
                    closeOnClick={true}
                    onDelete={this.onDeleteVocabulary.bind(this)}>
                    <GridSystem>
                        <LeftPane
                            description={term.Description}
                            type={term.Type}
                            onEnter={this.onDescriptionUpdate}
                            index={index}
                            scopeType={term.ScopeType}
                        />
                        <RightPane
                            vocabularyId={term.VocabularyId}
                            getVocabularyTerms={this.getVocabularyTerms.bind(this)}
                            onUpdateTerm={this.onUpdateTerm.bind(this)}
                            vocabularyTerms={props.vocabularyTerms}
                            selectParentTerm={this.selectParentTerm.bind(this)}
                            totalTermCount={props.totalTermCount}
                            index={index}
                            type={term.Type}
                            scopeType={term.ScopeType}
                            onDeleteTerm={this.onDeleteTerm.bind(this)}
                            parentTerms={vocabularyList} />
                    </GridSystem>
                </TermHeader>
            );
        });
        const loadMoreEnabled = props.vocabularyList.length < props.totalCount;
        return (
            <div className="vocabulary-list">
                <PersonaBarPageBody>
                    <Tabs onSelect={this.handleSelect.bind(this)}
                        selectedIndex={props.tabIndex}
                        tabHeaders={[LocalizedResources.get("All"), LocalizedResources.get("Application"), LocalizedResources.get("Portal")]}
                        type="secondary">
                        {(renderedVocabularyList.length > 0 && renderedVocabularyList) || <p className="vocabulary-error">{LocalizedResources.get("NoVocabularyTerms.Error")}</p>}
                        {(renderedVocabularyList.length > 0 && renderedVocabularyList) || <p className="vocabulary-error">{LocalizedResources.get("NoVocabularyTerms.Error")}</p>}
                        {(renderedVocabularyList.length > 0 && renderedVocabularyList) || <p className="vocabulary-error">{LocalizedResources.get("NoVocabularyTerms.Error")}</p>}
                    </Tabs>
                </PersonaBarPageBody>
                {loadMoreEnabled && <Button type="primary" style={loadMoreButtonStyle} onClick={this.onLoadMore.bind(this)}>{LocalizedResources.get("LoadMore")}</Button>}
            </div>
        );
    }
}

VocabularyListComponent.propTypes = {
    dispatch: PropTypes.func.isRequired,
    vocabularyList: PropTypes.array,
    totalCount: PropTypes.number,
    totalTermCount: PropTypes.number,
    vocabularyTerms: PropTypes.array,
    pagination: PropTypes.object,
    tabIndex: PropTypes.number,
    scopeTypeId: PropTypes.oneOfType([
        PropTypes.string,
        PropTypes.number
    ])
};

function mapStateToProps(state) {
    return {
        vocabularyList: state.vocabulary.vocabularyList,
        totalCount: state.vocabulary.totalCount,
        totalTermCount: state.vocabularyTermList.totalCount,
        vocabularyTerms: state.vocabularyTermList.vocabularyTerms,
        pagination: state.pagination,
        tabIndex: state.pagination.tabIndex,
        scopeTypeId: state.pagination.scopeTypeId
    };
}

export default connect(mapStateToProps)(VocabularyListComponent);