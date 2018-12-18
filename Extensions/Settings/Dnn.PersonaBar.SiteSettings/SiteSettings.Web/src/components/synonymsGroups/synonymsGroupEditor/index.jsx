import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import { Label, Button, Tags, InputGroup } from "@dnnsoftware/dnn-react-common";
import {
    search as SearchActions
} from "../../../actions";
import util from "../../../utils";
import resx from "../../../resources";

class SynonymsGroupEditor extends Component {
    constructor() {
        super();

        this.state = {
            group: {
                SynonymsTags: "",
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
        let group = Object.assign({}, props.group);
        group.CultureCode = props.culture;
        this.setState({
            group: group
        });
    }    

    onSettingChange(key, event) {
        let {state, props} = this;
        let group = Object.assign({}, state.group);
        
        group[key] = event.join();

        if (group[key] === "" && key === "SynonymsTags") {
            state.error["tags"] = true;
        }
        else if (group[key] !== "" && key === "SynonymsTags") {
            state.error["tags"] = false;
        }

        group.CultureCode = props.culture;

        this.setState({
            group: group,
            triedToSubmit: false,
            error: state.error
        });

        props.dispatch(SearchActions.synonymsGroupClientModified());
    }

    onSave() {
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.tags) {
            return;
        }

        props.onUpdate(state.group);
    }
    
    onCancel() {
        const {props} = this;
        if (props.synonymsGroupClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(SearchActions.cancelSynonymsGroupClientModified());
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
        if (this.state.group !== undefined || this.props.id === "add") {  
            return (
                <div className="synonyms-editor">
                    <InputGroup>
                        <Label
                            label={resx.get("Synonyms")}
                        />
                        <Tags
                            tags={this.state.group.SynonymsTags ? this.state.group.SynonymsTags.split(",") : []}
                            onUpdateTags={this.onSettingChange.bind(this, "SynonymsTags")}
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

SynonymsGroupEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    group: PropTypes.object,
    Collapse: PropTypes.func,
    onUpdate: PropTypes.func,
    id: PropTypes.string,
    culture: PropTypes.string,
    synonymsGroupClientModified: PropTypes.bool
};

function mapStateToProps(state) {
    return {              
        synonymsGroupClientModified: state.search.synonymsGroupClientModified
    };
}

export default connect(mapStateToProps)(SynonymsGroupEditor);