import React, { Component, PropTypes } from "react";
import Tag from "./Tag";
import "./style.less";

const KEY = {
    BACKSPACE: 8,
    TAB: 9,
    ENTER: 13,
    COMMA: 188
};

class Tags extends Component {
    constructor(props) {
        super(props);
        this.state = {
            newTagText: "",
            tags: this.props.tags || [],
            inputWidth: 13,
            isInputVisible: true,
            tagInputActive: false
        };
        this.onKeyDown = this.onKeyDown.bind(this);
    }

    componentDidMount() {
        document.addEventListener("keypress", this.onKeyDown, false);
        this.resizeInputField();
    }

    componentWillUnmount() {
        document.removeEventListener("keypress", this.onKeyDown, false);
    }

    hasClass(className, element) {
        return (" " + element.className + " ").indexOf(" " + className + " ") > -1;
    }

    onTagFocus() {
        this.setState({ tagInputActive: true });
    }

    onTagBlur() {
        this.setState({ tagInputActive: false });
    }

    resizeInputField() {
        let width = this.refs.tagsField.getBoundingClientRect().width - 30;
        if (this.state.tags.length) {
            width = this.state.newTagText.length ? this.state.newTagText.length * 8 + 6 : 13;
        }
        this.setState({ inputWidth: width, isInputVisible: true });
    }

    addTag() {
        const newTagText = this.state.newTagText;
        if (!newTagText) {
            return;
        }
        this.internalAddTag(newTagText);
    }

    internalAddTag(newTagText) {
        if (this.props.tags.find(t=> t === newTagText)) {
            this.setState({ newTagText: "" });
            return;
        }

        this.setState({ isInputVisible: false });
        const tags = this.props.tags.slice();
        tags.push(newTagText.trim());         
        this.updateTags(tags);
        this.setState({ newTagText: "" });
        setTimeout(() => { this.refs.inputField.focus(); }, 0);
    }

    removeTagByName(tag) {
        const tags = this.props.tags.filter((_tag) => { return _tag !== tag; }).slice();
        this.updateTags(tags);
    }

    removeLastTag() {
        if (this.state.newTagText) {
            return;
        }
        const tags = this.props.tags.slice();
        tags.pop();
        this.updateTags(tags);
    }

    updateTags(tags) {
        this.setState({ tags, newTagText: ""}, () => {
            this.resizeInputField();
            this.props.onUpdateTags(tags);
        });
    }

    onKeyDown(event) {
        switch (event.keyCode) {
            case KEY.ENTER:
            case KEY.COMMA:
            case KEY.TAB:
                if (this.state.newTagText) {
                    event.preventDefault();
                    this.addTag();
                }
                break;
            case KEY.BACKSPACE:
                this.removeLastTag();
                break;
        }
    }

    onChange(event) {
        this.setState({ newTagText: event.target.value }, this.resizeInputField.bind(this));
        if (this.props.autoSuggest && typeof(this.props.onAddingNewTagChange) === "function") {
            this.props.onAddingNewTagChange(event.target.value);
        }
    }
    
    focusInput() {
        this.refs.inputField.focus();
    }

    onSelectSuggestion(suggestion) {
        this.internalAddTag(suggestion);
        if (typeof (this.props.onSelectSuggestion) === "function") {
            this.props.onSelectSuggestion(suggestion);
        }
    }

    getSuggestions() {
        if (!this.props.autoSuggest) {
            return null;
        }

        return (<ul className="suggestions">
            {this.props.suggestions.map((suggestion, index) => {
                return <li className="suggestion" key={index} onClick={this.onSelectSuggestion.bind(this, suggestion.value)}>{suggestion.value}</li>;
            })}
            </ul>);
    }
    render() {
        let Tags;
        
        if (typeof this.props.renderItem === "function") {
            Tags = this.props.tags.map((tag, index) => {
                return this.props.renderItem(tag, index, this.removeTagByName.bind(this, tag), this.props.enabled);
            });
        } else {
            Tags = this.props.tags.map((tag, index) => {
                return <Tag tag={tag} key={index} onRemove={this.removeTagByName.bind(this, tag) } enabled={this.props.enabled} />;
            });
        }
        
        const inputStyle = {
            width: this.state.inputWidth,
            display: (this.state.isInputVisible === false ? "none" : "block")
        };
        const placeholderText =  this.state.tags.length ? "" : "Add Tags";

        let className = "dnn-uicommon-tags-field-input" +
            (this.state.tagInputActive ? " active " : "");

        const opts = {};

        if (!this.props.enabled) {
            opts["disabled"] = "disabled";
            className += " disabled";
        }
        
        return (
            <div className={className} 
                onClick={this.focusInput.bind(this) }
                ref="tagsField" style={this.props.style}>
                <div type="text">
                    {Tags}
                    <div className="input-container">
                        <input
                            ref="inputField"
                            type="text"
                            placeholder={placeholderText}
                            style={inputStyle}
                            onKeyDown={this.onKeyDown}
                            value={this.state.newTagText}
                            onChange={this.onChange.bind(this) }
                            onFocus={this.onTagFocus.bind(this) }
                            onBlur={this.onTagBlur.bind(this) }
                            {...opts}
                        />
                        {this.props.autoSuggest &&
                            this.getSuggestions()}
                    </div>
                </div>                
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
    onSelectSuggestion: PropTypes.func,
    suggestions: PropTypes.arrayOf(PropTypes.object),
    renderItem: PropTypes.func
};

Tags.defaultProps = {
    enabled: true,
    autoSuggest: false,
    suggestions: []
};

export default Tags;
export { Tag };
