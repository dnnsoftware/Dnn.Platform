import React, { Component, PropTypes } from "react";
import Tag from "./Tag";
import Suggestions from "./Suggestions";
import TagInput from "./TagInput";
import "./style.less";

class Tags extends Component {
    constructor(props) {
        super(props);
        this.state = {
            newTagText: "",
            tags: this.props.tags || [],
            inputWidth: 13,
            isInputVisible: false
        };
    }

    hasClass(className, element) {
        return (" " + element.className + " ").indexOf(" " + className + " ") > -1;
    }

    internalAddTag(newTagText) {
        if (newTagText && typeof(this.props.onNewTag) === "function") {
            this.props.onNewTag(newTagText);
        }

        this.setState({ newTagText: "" });
        if (this.props.tags.find(t=> t === newTagText)) {
            return;
        }

        const tags = this.props.tags.slice();
        tags.push(newTagText.trim());         
        this.updateTags(tags);
    }

    removeTagByName(tag) {
        const tags = this.props.tags.filter((_tag) => { return _tag !== tag; }).slice();
        this.updateTags(tags);
    }

    removeLastTag() {
        const tags = this.props.tags.slice();
        tags.pop();
        this.updateTags(tags);
    }

    updateTags(tags) {
        this.setState({ tags }, () => {
            this.props.onUpdateTags(tags);
        });
    }
    
    onClick() {
        if (this.state.isInputVisible) {
            return;
        }

        this.setState({ isInputVisible: true });
    }

    onInputClose() {
        if (!this.state.isInputVisible) {
            return;
        }

        this.setState({ isInputVisible: false,  newTagText: ""});
        
        if (typeof(this.props.onAddingNewTagChange) === "function") {
            this.props.onAddingNewTagChange("");
        }
    }

    addTag(tag) {
        if (!tag) {
            return;
        }
        this.internalAddTag(tag);
    }

    onAddingNewTagChange(value) {
        this.setState({newTagText:value});

        if (typeof(this.props.onAddingNewTagChange) === "function") {
            this.props.onAddingNewTagChange(value);
        }
    }

    render() {
        let Tags;
        if (typeof(this.props.renderItem) === "function") {
            Tags = this.props.tags.map((tag, index) => {
                return this.props.renderItem(tag, index, this.removeTagByName.bind(this, tag), this.props.enabled);
            });
        } else {
            Tags = this.props.tags.map((tag, index) => {
                return <Tag tag={tag} key={index} onRemove={this.removeTagByName.bind(this, tag)} enabled={this.props.enabled} />;
            });
        }
        
        let className = "dnn-uicommon-tags-field-input" +
            (this.state.isInputVisible ? " active " : "");

        const opts = {};

        if (!this.props.enabled) {
            opts["disabled"] = "disabled";
            className += " disabled";
        }
        
        const typingText = this.props.autoSuggest ? this.props.searchTagsPlaceholder : this.props.addTagsPlaceholder;
        return (
            <div className={className} 
                onClick={this.onClick.bind(this) }
                ref="tagsField" style={this.props.style}>
                <div type="text">
                    {Tags.length > 0 && Tags}
                    {!this.state.isInputVisible && Tags.length === 0 &&
                        <div className="typing-text">{typingText}</div>}
                    {this.state.isInputVisible && 
                    <TagInput
                        value={this.state.newTagText}
                        addTag={this.addTag.bind(this)}
                        onAddingNewTagChange={this.onAddingNewTagChange.bind(this)}
                        onClose={this.onInputClose.bind(this) }
                        opts={opts}
                        onFocus={this.props.onInputFocus}
                        newTagText={this.state.newTagText}
                        suggestions={this.props.suggestions}
                        removeLastTag={this.removeLastTag.bind(this)}
                        addTagsPlaceholder={this.props.addTagsPlaceholder} />}
                </div>
                {this.state.isInputVisible && 
                    this.props.autoSuggest && 
                    this.props.suggestions && 
                    this.props.suggestions.length > 0 &&
                <div className="suggestions-container">
                    <Suggestions suggestions={this.props.suggestions} 
                        onSelectSuggestion={this.addTag.bind(this)}
                        onScrollUpdate={this.props.onScrollUpdate} />
                </div>}
            </div>
        );
    }
}

Tags.propTypes = {
    tags: PropTypes.array.isRequired,
    onUpdateTags: PropTypes.func.isRequired,
    style: PropTypes.object,
    enabled: PropTypes.bool.isRequired,
    autoSuggest: PropTypes.bool.isRequired,
    onAddingNewTagChange: PropTypes.func,
    onNewTag: PropTypes.func,
    suggestions: PropTypes.arrayOf(PropTypes.object),
    renderItem: PropTypes.func,
    onScrollUpdate: PropTypes.func,
    onInputFocus: PropTypes.func,
    addTagsPlaceholder: PropTypes.string.isRequired,
    searchTagsPlaceholder: PropTypes.string
};

Tags.defaultProps = {
    addTagsPlaceholder: "Add Tags",
    searchTagsPlaceholder: "Begin typing to search tags",
    enabled: true,
    autoSuggest: false,
    suggestions: []
};

export default Tags;
export { Tag };
