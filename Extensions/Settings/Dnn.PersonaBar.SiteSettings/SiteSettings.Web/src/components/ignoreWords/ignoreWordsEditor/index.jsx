import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import Label from "dnn-label";
import Button from "dnn-button";
import Tags from "dnn-tags";
import InputGroup from "dnn-input-group";
import {
    search as SearchActions
} from "../../../actions";
import util from "../../../utils";
import resx from "../../../resources";

class IgnoreWordsEditor extends Component {
    constructor() {
        super();

        this.state = {
            words: {
                StopWords: "",
                CultureCode: "en-US"
            },            
            error: {
                tags: true
            },
            triedToSubmit: false
        };
    }

    componentDidMount() {
        const {props} = this;
        let words = Object.assign({}, props.words);
        words.CultureCode = props.culture;
        this.setState({
            words: Object.assign({}, props.words)
        });
    }

    onSettingChange(key, event) {
        let {state, props} = this;
        let words = Object.assign({}, state.words);
        
        words[key] = event.join();

        if (words[key] === "" && key === "StopWords") {
            state.error["tags"] = true;
        }
        else if (words[key] !== "" && key === "StopWords") {
            state.error["tags"] = false;
        }

        words.CultureCode = props.culture;

        this.setState({
            words: words,
            triedToSubmit: false,
            error: state.error
        });

        props.dispatch(SearchActions.ignoreWordsClientModified());
    }

    onSave() {
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.tags) {
            return;
        }

        props.onUpdate(state.words, state.culture);
    }
    
    onCancel() {
        const {props} = this;
        if (props.ignoreWordsClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(SearchActions.cancelIgnoreWordsClientModified());
                props.Collapse();
            });
        }
        else {
            props.Collapse();
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        /* eslint-disable react/no-danger */
        if (this.state.words !== undefined || this.props.id === "add") {  
            return (
                <div className="words-editor">
                    <InputGroup>
                        <Label
                            label={resx.get("IgnoreWords")}
                        />
                        <Tags
                            tags={this.state.words.StopWords ? this.state.words.StopWords.split(",") : []}
                            onUpdateTags={this.onSettingChange.bind(this, "StopWords")}
                        />
                    </InputGroup>
                    <div className="editor-buttons-box">
                        <Button
                            type="secondary"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>                        
                        <Button
                            type="primary"
                            onClick={this.onSave.bind(this)}>
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

IgnoreWordsEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    words: PropTypes.object,
    Collapse: PropTypes.func,
    onUpdate: PropTypes.func,
    id: PropTypes.string,
    culture: PropTypes.string,
    ignoreWordsClientModified: PropTypes.bool
};

function mapStateToProps(state) {
    return {              
        ignoreWordsClientModified: state.search.ignoreWordsClientModified
    };
}

export default connect(mapStateToProps)(IgnoreWordsEditor);