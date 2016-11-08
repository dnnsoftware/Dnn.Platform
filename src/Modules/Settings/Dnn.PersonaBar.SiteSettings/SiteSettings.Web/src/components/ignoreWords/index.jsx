import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    pagination as PaginationActions,
    search as SearchActions
} from "../../actions";
import IgnoreWordsRow from "./ignoreWordsRow";
import IgnoreWordsEditor from "./ignoreWordsEditor";
import Collapse from "react-collapse";
import "./style.less";
import DropDown from "dnn-dropdown";
import { AddIcon } from "dnn-svg-icons";
import util from "../../utils";
import resx from "../../resources";

let tableFields = [];

class IgnoreWordsPanel extends Component {
    constructor() {
        super();
        this.state = {
            ignoreWords: undefined,
            openId: "",
            culture: "en-US"
        };
    }

    componentWillMount() {
        const {props, state} = this;

        if (tableFields.length === 0) {
            tableFields.push({ "name": resx.get("IgnoreWords"), "id": "IgnoreWords" });
        }

        if (!props.cultures) {
            props.dispatch(SearchActions.getCultureList(props.portalId));
        }

        if (props.ignoreWords) {
            this.setState({
                ignoreWords: props.ignoreWords
            });
        }
        else {
            props.dispatch(SearchActions.getIgnoreWords(props.portalId, state.culture, (data) => {
                this.setState({
                    ignoreWords: Object.assign({}, data.IgnoreWords)
                });
            }));
        }
    }

    componentWillReceiveProps(props) {
        let {state} = this;

        if (props.ignoreWords) {
            this.setState({
                ignoreWords: props.ignoreWords
            });
            return;
        }
    }

    renderHeader() {
        let tableHeaders = tableFields.map((field) => {
            let className = "words-items header-" + field.id;
            return <div className={className} key={"header-" + field.id}>
                <span>{field.name}&nbsp; </span>
            </div>;
        });
        return <div className="header-row">{tableHeaders}</div>;
    }

    uncollapse(id) {
        setTimeout(() => {
            this.setState({
                openId: id
            });
        }, this.timeout);
    }

    collapse() {
        if (this.state.openId !== "") {
            this.setState({
                openId: ""
            });
        }
    }

    toggle(openId) {
        if (openId !== "") {
            this.uncollapse(openId);
        }
    }

    onUpdateIgnoreWords(words) {
        const {props, state} = this;

        if (words.StopWordsId) {
            props.dispatch(SearchActions.updateIgnoreWords(words, (data) => {
                util.utilities.notify(resx.get("IgnoreWordsUpdateSuccess"));
                this.collapse();
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
        else {
            props.dispatch(SearchActions.addIgnoreWords(words, (data) => {
                util.utilities.notify(resx.get("IgnoreWordsCreateSuccess"));
                this.collapse();
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
    }

    onDeleteIgnoreWords(words) {
        const {props, state} = this;
        util.utilities.confirm(resx.get("IgnoreWordsDeletedWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SearchActions.deleteIgnoreWords(words, () => {
                util.utilities.notify(resx.get("IgnoreWordsDeleteSuccess"));
                this.collapse();
            }, (error) => {
                util.utilities.notify(resx.get("IgnoreWordsDeleteError"));
            }));
        });
    }

    onSelectCulture(event) {
        let {state, props} = this;

        this.setState({
            culture: event.value
        });

        props.dispatch(SearchActions.getIgnoreWords(props.portalId, event.value, (data) => {
            this.setState({
                ignoreWords: Object.assign({}, data.IgnoreWords)
            });
        }));
    }

    getCultureOptions() {
        const {props, state} = this;
        let options = [];
        if (props.cultures !== undefined) {
            options = props.cultures.map((item) => {
                return {
                    label: <div style={{ float: "left", display: "flex" }}>
                        <div className="language-flag">
                            <img src={item.Icon} />
                        </div>
                        <div className="language-name">{item.Name}</div>
                    </div>, value: item.Code
                };
            });
        }
        return options;
    }

    /* eslint-disable react/no-danger */
    renderedIgnoreWords() {
        const {props, state} = this;
        if (props.ignoreWords) {
            return (
                <IgnoreWordsRow
                    wordsId={props.ignoreWords.StopWordsId}
                    tags={props.ignoreWords.StopWords}
                    index={0}
                    key={"wordsItem-0"}
                    closeOnClick={true}
                    openId={this.state.openId}
                    OpenCollapse={this.toggle.bind(this)}
                    Collapse={this.collapse.bind(this)}
                    onDelete={this.onDeleteIgnoreWords.bind(this, props.ignoreWords)}
                    id={"row-1"}>
                    <IgnoreWordsEditor
                        words={props.ignoreWords}
                        culture={this.state.culture}
                        Collapse={this.collapse.bind(this)}
                        onUpdate={this.onUpdateIgnoreWords.bind(this)}
                        id={"row-1"}
                        openId={this.state.openId} />
                </IgnoreWordsRow>
            );
        }
    }

    render() {
        const {props, state} = this;
        let opened = (state.openId === "add");
        return (
            <div>
                <div className="words-group-items">
                    <div className="AddItemRow">
                        <div className="sectionTitle">{resx.get("IgnoreWords")}</div>
                        {!props.ignoreWords &&
                            <div className={opened ? "AddItemBox-active" : "AddItemBox"} onClick={this.toggle.bind(this, opened ? "" : "add")}>
                                <div className="add-icon" dangerouslySetInnerHTML={{ __html: AddIcon }}>
                                </div> Add Word
                        </div>
                        }
                        {this.props.cultures &&
                            <div className="synonyms-filter">
                                <DropDown
                                    value={this.state.culture}
                                    fixedHeight={200}
                                    style={{ width: "auto" }}
                                    options={this.getCultureOptions()}
                                    withBorder={false}
                                    onSelect={this.onSelectCulture.bind(this)}
                                    />
                            </div>
                        }
                    </div>
                    <div className="words-items-grid">
                        {this.renderHeader()}
                        <Collapse isOpened={opened} style={{ float: "left", width: "100%" }}>
                            <IgnoreWordsRow
                                tags={"-"}
                                index={"add"}
                                key={"wordsItem-add"}
                                closeOnClick={true}
                                openId={state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                onDelete={this.onDeleteIgnoreWords.bind(this)}
                                id={"add"}>
                                <IgnoreWordsEditor
                                    Collapse={this.collapse.bind(this)}
                                    culture={this.state.culture}
                                    onUpdate={this.onUpdateIgnoreWords.bind(this)}
                                    id={"add"}
                                    openId={state.openId} />
                            </IgnoreWordsRow>
                        </Collapse>
                        {this.renderedIgnoreWords()}
                    </div>
                </div>

            </div >
        );
    }
}

IgnoreWordsPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    ignoreWords: PropTypes.object,
    portalId: PropTypes.number,
    cultures: PropTypes.array
};

function mapStateToProps(state) {
    return {
        ignoreWords: state.search.ignoreWords,
        tabIndex: state.pagination.tabIndex,
        cultures: state.search.cultures
    };
}

export default connect(mapStateToProps)(IgnoreWordsPanel);