import React, {Component, PropTypes} from "react";
import Tooltip from "dnn-tooltip";
import Tag from "./Tag";

import "./style.less";

const KEY = {
    BACKSPACE: 8,
    TAB: 9,
    ENTER: 13,
    ESCAPE: 27,
    UP: 38,
    DOWN: 40

};

export default class SearchableTags extends Component {
    constructor(props) {
        super(props);
        this.timeout = 300;
        this.setTimeout = null;
        this.onKeyDown = this.onKeyDown.bind(this);
        this.state = {
            tags: this.props.tags || [],
            newTagText: "",
            isInputVisible: true,
            tagInputActive: false,
            selectedIndex: 0,
            loading: false,
            inputWidth: 13,
            searchResults: []
        };
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

    getServiceFramework() {
        let sf = this.props.utils.utilities.sf;
        sf.controller = "ProfileService";
        sf.moduleRoot = "InternalServices";
        return sf;
    }

    getSearchResults() {
        this.resizeInputField();
        const searchQuery = this.state.newTagText && typeof this.state.newTagText === "string" ? this.state.newTagText : "";
        if (!searchQuery) {
            return this.setSearchResults([]);
        }
        this.setState({ loading: true });
        clearTimeout(this.setTimeout);
        this.setTimeout = setTimeout(() => {
            const sf = this.getServiceFramework();
            return sf.get("Search", { q: searchQuery }, this.setSearchResults.bind(this), this.handleError.bind(this));
        }, this.timeout);
    }

    setSearchResults(searchResults) {
        this.setState({ searchResults, selectedIndex: 0, loading: false });
    }

    handleError(error) {
        this.setState({ loading: false });
        console.log('ERROR:', error);
    }

    resizeInputField() {
        let width = this.refs.tagsField.getBoundingClientRect().width - 30;
        if (this.state.tags.length) {
            width = this.state.newTagText.length ? this.state.newTagText.length * 7 + 6 : 13;
        }
        this.setState({ inputWidth: width, isInputVisible: true });
    }

    addTag(_tag) {
        if (!this.state.searchResults.length || this.props.isDisabled) {
            return;
        }
        const index = this.state.selectedIndex || 0;
        const tag = _tag || this.state.searchResults[index];
        if (!tag) {
            return;
        }
        this.setState({ isInputVisible: false });
        let {tags} = this.state;
        tags.push(tag);
        this.updateTags(tags);
        setTimeout(() => { this.refs.inputField.focus(); }, 0);
    }

    updateSelectedIndex(diff) {
        if (this.props.isDisabled) {
            return;
        }
        let selectedIndex = this.state.selectedIndex + diff;
        if (selectedIndex < 0) {
            selectedIndex = 0;
        }
        if (selectedIndex >= this.state.searchResults.length) {
            selectedIndex = this.state.searchResults.length - 1;
        }
        this.setState({ selectedIndex });
    }

    onKeyDown(event) {
        switch (event.keyCode) {
            case KEY.ENTER:
                return this.addTag();
            case KEY.TAB:
                event.preventDefault();
                return this.addTag();
            case KEY.BACKSPACE:
                return this.removeLastTag();
            case KEY.UP:
                return this.updateSelectedIndex(-1);
            case KEY.DOWN:
                return this.updateSelectedIndex(1);
        }
    }

    onTagFocus() {
        this.setState({ tagInputActive: true });
    }

    onTagBlur() {
        this.setState({ tagInputActive: false });
    }

    removeLastTag() {
        if (this.state.newTagText || this.props.isDisabled) {
            return;
        }
        let tags = this.state.tags;
        tags.pop();
        this.updateTags(tags);
    }

    onChange(event) {
        if (this.props.isDisabled) {
            return;
        }
        this.setState({ newTagText: event.target.value }, this.getSearchResults.bind(this));
    }

    selectItem(selectedIndex) {
        this.setState({ selectedIndex });
    }

    removeTagByName(tagId) {
        if (this.props.isDisabled) {
            return;
        }
        let tags = this.state.tags;
        tags = tags.filter((_tag) => { return _tag.id !== tagId; });
        this.updateTags(tags);
    }

    updateTags(tags) {
        this.setState({ tags, newTagText: "", searchResults: [] }, () => {
            this.resizeInputField();
            this.props.onUpdateTags(tags);
        });
    }

    getResultsItems() {
        if (this.state.searchResults.length) {
            return this.state.searchResults.map((result, index) => {
                const className = index === this.state.selectedIndex ? "selected" : "";
                return <div
                    key={result.id}
                    className={"result-item " + className}
                    onClick={this.addTag.bind(this, result) }
                    onMouseEnter={this.selectItem.bind(this, index) }
                    >
                    {result.name}</div>;
            });
        } else if (!this.state.searchResults.length && this.state.newTagText) {
            const text = this.state.loading ? "Searching..." : "No Results";
            return <div className="result-item">{text}</div>;
        } else {
            return false;
        }
    }

    focusInput() {
        this.refs.inputField.focus();
    }

    render() {
        const Tags = this.state.tags.map((tag, index) => {
            return <Tag tag={tag.name} key={tag.id  + index} onRemove={this.removeTagByName.bind(this, tag.id) }/>;
        });
        
        const inputStyle = {
            width: this.state.inputWidth,
            display: (this.state.isInputVisible === false ? "none" : "block")
        };
        const searchResults = this.getResultsItems();
        const placeholderText = this.props.isDisabled || this.state.tags.length ? "" : "Begin typing to search tags";
        let className = "tags-field" +
            (this.state.tagInputActive ? " active " : "") +
            (this.props.isDisabled ? " disabled" : "") +
            (this.props.error ? " error" : "");

        return <div
            className={ className }
            onClick={this.focusInput.bind(this) }
            ref="tagsField">
            <div type="text" className="dark-form-control">
                {Tags}
                <input
                    disabled={this.props.isDisabled}
                    ref="inputField"
                    type="text"
                    placeholder={placeholderText}
                    style={inputStyle}
                    onKeyDown={this.onKeyDown.bind(this) }
                    value={this.state.newTagText}
                    onChange={this.onChange.bind(this) }
                    onFocus={this.onTagFocus.bind(this) }
                    onBlur={this.onTagBlur.bind(this) } 
                    aria-label="Tag" />
            </div>
            {searchResults && <div className="tag-search-results">
                {searchResults}
            </div>}
            {this.props.error && this.props.errorMessage && <Tooltip
                messages={[this.props.errorMessage]}
                type="error"
                tooltipPlace={"top"}
                rendered={this.props.error}/>}
        </div>;
    }
}

SearchableTags.propTypes = {
    utils: PropTypes.object.isRequired,
    tags: PropTypes.array.isRequired,
    onUpdateTags: PropTypes.func.isRequired,
    
    error: PropTypes.bool,
    errorMessage: PropTypes.string,
    
    isDisabled: PropTypes.bool
};