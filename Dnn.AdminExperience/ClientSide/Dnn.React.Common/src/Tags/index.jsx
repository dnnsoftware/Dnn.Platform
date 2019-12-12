import React, { Component } from "react";
import PropTypes from "prop-types";
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
            isInputVisible: false,
            selectedIndex:-1
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
        if (!this.props.tags.find(t => t.toUpperCase().trim() === newTagText.toUpperCase().trim())) {
            const tags = this.props.tags.slice();
            tags.push(newTagText.trim());
            this.updateTags(tags);
        }
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
        this.setState({selectedIndex:-1});
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
        if (tag) {
            this.internalAddTag(tag);
        } else if (this.state.selectedIndex > -1) {
            let selectedTag = this.props.suggestions[this.state.selectedIndex];
            this.internalAddTag(selectedTag.value);
            this.setState({selectedIndex:-1});
        }
    }

    onAddingNewTagChange(value) {
        this.setState({newTagText:value});

        if (typeof(this.props.onAddingNewTagChange) === "function") {
            this.props.onAddingNewTagChange(value);
        }
    }

    onArrowDown() {
        const maxIndex = this.props.suggestions.length - 1;
        const current = this.state.selectedIndex;
        const selectedIndex = current < maxIndex ? current + 1 : current;
        this.setState({selectedIndex});
    }

    onArrowUp() {
        const current = this.state.selectedIndex;
        if (current > 0) {
            const selectedIndex = current - 1;
            this.setState({selectedIndex});
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
                style={this.props.style}
                ref={ref => this.containerRef = ref}>
                <div type="text">
                    {Tags.length > 0 && Tags}
                    <TagInput
                        value={this.state.newTagText}
                        addTag={this.addTag.bind(this)}
                        onAddingNewTagChange={this.onAddingNewTagChange.bind(this)}
                        onArrowUp={this.onArrowUp.bind(this)}
                        onArrowDown={this.onArrowDown.bind(this)}
                        onClose={this.onInputClose.bind(this) }
                        opts={opts}
                        onFocus={this.props.onInputFocus}
                        newTagText={this.state.newTagText}
                        removeLastTag={this.removeLastTag.bind(this)}
                        addTagsPlaceholder={this.props.addTagsPlaceholder}
                        container={this.containerRef}
                        selectedIndex={this.state.selectedIndex}
                        suggestions={this.props.suggestions}
                    />
                </div>
                {this.state.isInputVisible &&
                    this.state.newTagText.length > 0 &&
                    this.props.autoSuggest &&
                    this.props.suggestions &&
                    this.props.suggestions.length > 0 &&
                <div className="suggestions-container">
                    <Suggestions suggestions={this.props.suggestions}
                        onSelectSuggestion={this.addTag.bind(this)}
                        selectedIndex={this.state.selectedIndex}
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
